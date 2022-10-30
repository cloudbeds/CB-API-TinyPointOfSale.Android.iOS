using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace CloudbedsTinyPOS_Mobile
{
    public partial class MainPage : ContentPage
    {
        PosOrderManager _currentPosOrderManger = null;
        //What Guest has been selected in the UI presently?
        CloudbedsGuest _currentSelectedGuest = null;
        //CloudbedsGuestManager _currentGuestManager;

        /// <summary>
        /// States we can be in
        /// </summary>
        enum NavState
        {
            None,
            PosRestart,
            PosChooseMenuItemsForOrder,
            PosChooseGuest,
            PosChooseGratuity,
            PosFinalize
        }
        NavState _navState = NavState.None;

        /// <summary>
        /// We will keep the UI for the order-manager in memory for the full
        /// duration of a guest's order process (rather than recreate it on demand)
        /// because it contains state (e.g. the prices and # of each item ordered)
        /// 
        /// If we re-created it on-demand we would need to repoulate this and re-calculate all #s of items ordered 
        /// </summary>
        uiPosOrderList _cachedUi_uiOrderManager = null;

        public MainPage()
        {
            InitializeComponent();
        }
        /// <summary>
        /// The created UI area for the current POS data entry stage we are in...
        /// </summary>
        View _navState_ActiveUI = null;



        /// <summary>
        /// Perform any actions we need to do when EXITING a state
        /// </summary>
        /// <param name="currentState"></param>
        private void StateMachine_PrepareToExitState(NavState currentState)
        {
            switch(currentState)
            {
                case NavState.PosRestart:
                    //NOTHING TO DO
                    return;

                case NavState.PosChooseMenuItemsForOrder:
                    RefreshOrderManagerFromOrderMenuUi(_navState_ActiveUI as uiPosOrderList);
                    return;

                case NavState.PosChooseGuest:
                    StateMachine_PrepareToExitState_PosChooseGuest();
                    return;

                case NavState.PosChooseGratuity:
                    StateMachine_PrepareToExitState_PosChooseGratuity();
                    return;

                case NavState.PosFinalize:
                    StateMachine_PrepareToExitState_PosFinalize();
                    return;

                case NavState.None:
                    //Nothing to do
                    return;
                    
                default:
                    IwsDiagnostics.Assert(false, "1028-448: Uknown state");
                    return;
            }
        }

        /// <summary>
        /// State exit logic -- pull the data we need from the UI
        /// </summary>
        private void StateMachine_PrepareToExitState_PosChooseGuest()
        {
            //Get the selected guest (or null) from the guests chooser.
            var ctlAsGuestsList = _navState_ActiveUI as uiGuestList;
            if (ctlAsGuestsList != null)
            {
                _currentSelectedGuest = ctlAsGuestsList.SelecedGuest;
            }
            else
            {
                IwsDiagnostics.Assert(false, "1028-544: Internal error. No guest selector");
            }
        }

        /// <summary>
        /// State exit logic -- pull the data we need from the UI
        /// </summary>
        private void StateMachine_PrepareToExitState_PosChooseGratuity()
        {
            var uiPart = _navState_ActiveUI as uiPosGratuityChooser;
            if (uiPart != null)
            {
                EnsurePosOrderManager().Gratuity = uiPart.CurrentGratuityAmount;
            }
            else
            {
                IwsDiagnostics.Assert(false, "1028-618: Internal error. Gratuity UI");
            }
        }

        /// <summary>
        /// State exit logic -- pull the data we need from the UI
        /// </summary>
        private void StateMachine_PrepareToExitState_PosFinalize()
        {
            //Nothing to do
        }

        /// <summary>
        /// Perform any actions we need to do when EXITING a state
        /// </summary>
        /// <param name="currentState"></param>
        private void StateMachine_PrepareToEnterState(NavState currentState)
        {
            switch (currentState)
            {
                case NavState.PosRestart:
                    //NOTHING TO DO
                    btnStateNav_Advance.Text = ">";
                    var ctlRestart = new uiPosOrderStartPage();
                    ctlRestart.PosRestartOrder += CtlRestart_PosRestartOrder;
                    StateMachine_ReplaceActiveUIArea(ctlRestart);
                    return;

                case NavState.PosChooseMenuItemsForOrder:
                    //If there is an existing UI page we are caching, then show it.  Otherwise create it.
                    StateMachine_ReplaceActiveUIArea(
                        EnsureCachedUI_PosOrderItemsAndCounts());
                    btnStateNav_Advance.Text = ">";
                    return;

                case NavState.PosChooseGuest:
                    var guestManager = CloudbedsSingletons.CloudbedsGuestManager;
                    guestManager.EnsureCachedData();
                    var ctlChooseGuest = new uiGuestList(guestManager.Guests);
                    ctlChooseGuest.SelecedGuest = _currentSelectedGuest;
                    StateMachine_ReplaceActiveUIArea(ctlChooseGuest);
                    btnStateNav_Advance.Text = ">";
                    return;

                case NavState.PosChooseGratuity:
                    var ctlChooseGratuity = new uiPosGratuityChooser();
                    var posOrderTotals = EnsurePosOrderManager().CalculateTotals();
                    ctlChooseGratuity.CurrentGratuityAmount = posOrderTotals.Gratuity;
                    ctlChooseGratuity.SetBaseItemTotalForGratuity(posOrderTotals.TotalItemsPrice + posOrderTotals.TotalTax);
                    StateMachine_ReplaceActiveUIArea(ctlChooseGratuity);
                    btnStateNav_Advance.Text = ">";
                    return;

                case NavState.PosFinalize:
                    var ctlFinalize = new uiPosOrderSummary(EnsurePosOrderManager(), _currentSelectedGuest);
                    ctlFinalize.CommitTransaction += CtlFinalize_CommitTransaction;
                    StateMachine_ReplaceActiveUIArea(ctlFinalize);
                    btnStateNav_Advance.Text = ">";
                    return;

                case NavState.None:
                    //Nothing to do
                    btnStateNav_Advance.Text = "Start";
                    return;

                default:
                    IwsDiagnostics.Assert(false, "1028-448: Uknown state");
                    btnStateNav_Advance.Text = ">";
                    return;
            }
        }

        /// <summary>
        /// Called when showing hte POS Order Summary page, when the user decides to commit the transaction
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CtlFinalize_CommitTransaction(object sender, EventArgs e)
        {
            string statusText = "1029-754: Unknown failure"; //This text should get replaced blow

            try
            {
                statusText = CtlFinalize_CommitTransaction_Inner();
            }
            catch(Exception ex)
            {
                statusText = "FAILURE: " + ex.Message;
            }

            //The currently showing UI panel should support having its status text updated
            IUpdateStatusText updateStatusText = _navState_ActiveUI as IUpdateStatusText;
            if(updateStatusText != null)
            {
                updateStatusText.SetStatusText(statusText);
            }
            else
            {
                IwsDiagnostics.Assert(false, "1029-757.  Active UI part does not support status update, " + statusText);
            }
        }

        /// <summary>
        /// Try to commit and finalize hte transaction
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private string CtlFinalize_CommitTransaction_Inner()
        {
            var selectedGuest = _currentSelectedGuest;
            if (selectedGuest == null)
            {
                throw new Exception("No guest selected. Cannot post charge");
            }

            var posOrderManager = _currentPosOrderManger;
            if (posOrderManager == null)
            {
                throw new Exception("No point of sale data. Cannot post charge");
            }

            var calculatedTotals = posOrderManager.CalculateTotals();
            if (calculatedTotals.GrandTotal <= 0)
            {
                throw new Exception("No transactions to post");
            }


            //Make sure we are signed in...
            //EnsureSignedIn();

            //===============================================================
            //Post the order up to the server
            //===============================================================
            var statusLogs = TaskStatusLogsSingleton.Singleton;

            var postCharge = new CloudbedsPostChargeToGuest(
                CloudbedsSingletons.CloudbedsServerInfo,
                CloudbedsSingletons.CloudbedsAuthSession,
                CloudbedsSingletons.StatusLogs,
                selectedGuest,
                posOrderManager);

            var wasSuccess = postCharge.ExecuteRequest();

            //Show the basic result
            if (!wasSuccess)
            {
                throw new Exception("FAILURE posting charge");
            }

            //Tesponse for the UI
            return postCharge.CommandResults_SummaryText;
        }

        /// <summary>
        /// Event triggered when the user requests RESTARTING the order
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void CtlRestart_PosRestartOrder(object sender, EventArgs e)
        {
            //Clear the guest selection
            _currentSelectedGuest = null;
            //Clear out the order manager
            _currentPosOrderManger = new PosOrderManager();
            //Clear out the cached UI for the order manager
            _cachedUi_uiOrderManager = null;

            //Navigate the UI...
            StateMachine_PerformTransition(_navState, NavState.PosChooseMenuItemsForOrder);
        }


        /// <summary>
        /// Pull the order items from the Menu Ordering UI
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private void RefreshOrderManagerFromOrderMenuUi(uiPosOrderList ctlPosOrderList)
        {
            if(ctlPosOrderList == null)
            {
                IwsDiagnostics.Assert(false, "1028-511: No order manager supplied");
                return;
            }

            var posOrderManager = EnsurePosOrderManager();

            //Out with the old....
            posOrderManager.ClearOrderItems();

            //In with the new...
            foreach (var ctlListItem in ctlPosOrderList.PosOrderListItems)
            {
                int itemCount = ctlListItem.ItemOrderCount;
                if (itemCount > 0)
                {
                    //If there are multiple items, add them each as individual line items
                    for (int itemIdx = 0; itemIdx < itemCount; itemIdx++)
                    {
                        posOrderManager.AddItemToOrder(
                            ctlListItem.Item_UnitPrice,
                            ctlListItem.Item_TaxAmount,
                            ctlListItem.Item_TaxName,
                            ctlListItem.Item_ClassId,
                            ctlListItem.Item_Name,
                            ctlListItem.Item_CategoryName);
                    }
                }
            }
        }



        /// <summary>
        /// Called to move from 1 state into another
        /// </summary>
        /// <param name="currentState"></param>
        /// <param name="nextState"></param>
        void StateMachine_PerformTransition(NavState currentState, NavState nextState)
        {
            //Nothing to do
            if (currentState == nextState)
            {
                return;
            }

            StateMachine_PrepareToExitState(currentState);
            StateMachine_PrepareToEnterState(nextState);
            _navState = nextState;
        }


        /// <summary>
        /// What is the next state we want to go into...
        /// </summary>
        /// <param name="currentState"></param>
        /// <returns></returns>
        private NavState GetNextState_Advance(NavState currentState)
        {
            switch (currentState)
            {
                case NavState.None:
                    return NavState.PosChooseMenuItemsForOrder;

                case NavState.PosRestart:
                    return NavState.PosChooseMenuItemsForOrder;

                case NavState.PosChooseMenuItemsForOrder:
                    return NavState.PosChooseGuest;

                case NavState.PosChooseGuest:
                    return NavState.PosChooseGratuity;

                case NavState.PosChooseGratuity:
                    return NavState.PosFinalize;

                case NavState.PosFinalize:
                    return NavState.PosRestart;

                //Nothing to do
                default:
                    IwsDiagnostics.Assert(false, "1028-452: Uknown state");
                    return currentState; //Stay where we are..
            }
        }

        /// <summary>
        /// What is the next state we want to go into...
        /// </summary>
        /// <param name="currentState"></param>
        /// <returns></returns>
        private NavState GetNextState_Retreat(NavState currentState)
        {
            switch (currentState)
            {
                case NavState.None:
                    return NavState.PosChooseMenuItemsForOrder;

                case NavState.PosRestart:
                    return NavState.PosRestart;

                case NavState.PosChooseMenuItemsForOrder:
                    return NavState.PosRestart;

                case NavState.PosChooseGuest:
                    return NavState.PosChooseMenuItemsForOrder;

                case NavState.PosChooseGratuity:
                    return NavState.PosChooseGuest;

                case NavState.PosFinalize:
                    return NavState.PosChooseGratuity;

                //Nothing to do
                default:
                    IwsDiagnostics.Assert(false, "1028-452: Uknown state");
                    return currentState; //Stay where we are..
            }

        }
        void btnStateNav_Advance_Clicked(object sender, EventArgs e)
        {
            var currentState = _navState;
            StateMachine_PerformTransition(currentState, GetNextState_Advance(currentState));
        }

        void btnStateNav_Previous_Clicked(object sender, EventArgs e)
        {
            var currentState = _navState;
            StateMachine_PerformTransition(currentState, GetNextState_Retreat(currentState));
        }


        /// <summary>
        /// Fills the dynamic part of the screen with new content
        /// </summary>
        /// <param name="newContent"></param>
        private void StateMachine_ReplaceActiveUIArea(View newContent, bool resetScrollPosition = true)
        {
            //Out with the old...
            uiStackDyamicContent.Children.Clear();
            //In with the new
            uiStackDyamicContent.Children.Add(newContent);

            //Store the state we are at...
            _navState_ActiveUI = newContent;

            //Force the scroll back to the top
            if(resetScrollPosition)
            {
                //Scroll to the top
                ctlMainScrollArea.ScrollToAsync(0, 0, false);
            }
        }

            /****************************************************************************************
            ENSURE XXXXXX Functions
            Ensures necessary objects have been loaded and are ready to use. These are typically
            called before common actions are performed to make sure the system is in a fully
            prepared state
            *****************************************************************************************/
            #region "Ensure XXXXXXX"
            
            /// <summary>
            /// Create an order manager if we need one
            /// </summary>
            /// <returns></returns>
            PosOrderManager EnsurePosOrderManager()
            {
                var orderManager = _currentPosOrderManger;
                if (orderManager == null)
                {
                    orderManager = new PosOrderManager();
                    _currentPosOrderManger = orderManager;
                }

                return orderManager;
            }
        

        /// <summary>
        /// Ensures we have a cachced version of the Order entry UI availalbe
        /// </summary>
        /// <returns></returns>
        private uiPosOrderList EnsureCachedUI_PosOrderItemsAndCounts()
        {
            var ctl = _cachedUi_uiOrderManager;
            if(ctl != null)
            {
                return ctl;
            };

            ctl = new uiPosOrderList();
            ctl.FillPosOrderItemsList(CloudbedsSingletons.PointOfSaleItemManager);
            _cachedUi_uiOrderManager = ctl;
            return ctl;

        }

        #endregion


        /// <summary>
        /// Clicked if the user wants to give feedback on the application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TapGestureRecognizer_TappedGiveApplicationFeedback(object sender, EventArgs e)
        {
            Launcher.OpenAsync("https://github.com/cloudbeds/CB-API-TinyPointOfSale.Android.iOS/issues");
        }

        
        /// <summary>
        /// Puts the Point of Sale UI going...
        /// </summary>
        private void AdvanceFromHomePage_StartApp()
        {
            //Show the navigation controls to move between Point of Sale steps
            groupNavPreviousNext.IsVisible = true;

            //=======================================================================
            //Start the query of any data (e.g. lists of guests) that we are are
            //going to need in the Point of Sale workflow.  This will help reduce
            //any time lag in going to screens that need this data, since it will have
            //already been fetched (or is being fetched) by the time the user gets to 
            //these screens
            //=======================================================================
            CloudbedsSingletons.WarmUpCloudbedsDataCachesIfNeeded_Async();

            //Start the Point of Sale workflow
            StateMachine_PerformTransition(_navState, NavState.PosChooseMenuItemsForOrder);
        }

        private void uiAppHomePage_StartAppWithFakeData(object sender, EventArgs e)
        {
            //Simulate the data
            AppSettings.UseSimulatedGuestData = true;
            AdvanceFromHomePage_StartApp();
        }

        private void uiAppHomePage_StartAppWithRealData(object sender, EventArgs e)
        {
            //Simulate the data
            AppSettings.UseSimulatedGuestData = false;
            AdvanceFromHomePage_StartApp();

        }
    }
}
