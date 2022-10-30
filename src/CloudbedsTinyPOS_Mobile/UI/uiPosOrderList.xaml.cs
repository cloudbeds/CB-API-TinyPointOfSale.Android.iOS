using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace CloudbedsTinyPOS_Mobile
{
    /// <summary>
    /// This UI CONTROL shows the menu of orderable items and allows the user
    /// to add items to their order menu
    /// </summary>
    public partial class uiPosOrderList : StackLayout
    {

        /// <summary>
        /// Delegate and Event for when a PosOrderItem gets clicked/selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal delegate void PosOrderItemSelectedEventHandler(object sender, PosItemSelectedEventArgs e);
        internal event PosOrderItemSelectedEventHandler? PosOrderItemSelected;


        List<uiPosOrderListItem> _menuUiItems = new List<uiPosOrderListItem>();


                

        /// <summary>
        /// The array of list items
        /// </summary>
        internal ICollection<uiPosOrderListItem> PosOrderListItems
        {
            get { return _menuUiItems.AsReadOnly(); }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public uiPosOrderList()
        {
            InitializeComponent();
        }


        /// <summary>
        /// # menu items
        /// </summary>
        public int MenuItemsCount
        {
            get
            {
                if (_menuUiItems == null)
                {
                    return 0;

                }
                return _menuUiItems.Count;
            }
        }


        /// <summary>
        /// Called when an individual PosOrderItem list item is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void EventHander_PosOrderItemSelectedEventHandler(object sender, PosItemSelectedEventArgs e)
        {
            if (PosOrderItemSelected == null)
            {
                return;
            }

            //Bubble the event upward
            PosOrderItemSelected(this, e);
        }

        /// <summary>
        /// Fill the visible list with controls...
        /// </summary>
        /// <param name="posItems"></param>
        internal void FillPosOrderItemsList(PosItemManager posItemManager)
        {
            //==========================================================
            //Get rid of all the existing items
            //==========================================================
            var uiChildren = spListOfPosOrderItems.Children;
            uiChildren.Clear();
            _menuUiItems = null;
            var menuItemsList = new List<uiPosOrderListItem>();


            if ((posItemManager == null) || (posItemManager.ItemCount == 0))
            {

                //We could show a fancier custom control here to indicate
                //that there are no controls
                var txtCtl = new Label();
                txtCtl.Text = "No POS Items in list";

                uiChildren.Add(txtCtl);
                return;
            }


            //====================================================================
            //Group the items by category
            //====================================================================
            foreach (var thisCategory in posItemManager.GetCategories())
            {
                var posItems = posItemManager.GetPosItemsInCategory(thisCategory);
                //UNDONE: We could SORT these items alphabetically
                if((posItems != null) && (posItems.Count > 0))
                {
                    helper_addCategoryHeaderUI(thisCategory, uiChildren);
                    foreach(var thisPosItem in posItems)
                    {
                        helper_addListItemForPosItem(thisPosItem, uiChildren, menuItemsList);
                        
                    }
                }
            }

            //Store the list of PosItem UI controls
            _menuUiItems = menuItemsList;

            //Update the totals
            RecalculateAndUpdateOrderSummaryText();
        }

        /// <summary>
        /// Add a list item
        /// </summary>
        /// <param name="thisPosItem"></param>
        /// <param name="uiChildren"></param>
        /// <param name="menuUiItems"></param>
        private void helper_addListItemForPosItem(PosItem thisPosItem, IList<View> uiChildren, List<uiPosOrderListItem> menuItems)
        {
            //Create the control
            var ctlPosOrderItemListItem = new uiPosOrderListItem(thisPosItem);
            //Hook up the event to listen to it here...
            ctlPosOrderItemListItem.PosItemSelected += EventHander_PosOrderItemSelectedEventHandler;

            //Listen to events of the order being updated
            ctlPosOrderItemListItem.OrderUpdated += CtlPosOrderItemListItem_OrderUpdated;

            uiChildren.Add(ctlPosOrderItemListItem);

            //Add it to our logical list of menu items we care about
            menuItems.Add(ctlPosOrderItemListItem);
        }

        /// <summary>
        /// Header item UI in list
        /// </summary>
        /// <param name="thisCategory"></param>
        /// <param name="uiChildren"></param>
        private void helper_addCategoryHeaderUI(string thisCategory, IList<View> uiChildren)
        {
            var ctlHeader = new uiPosOrderListItemHeader(thisCategory);
            /*
            ctlHeader.Text = thisCategory;
            ctlHeader.BackgroundColor = Color.Black;
            ctlHeader.TextColor = Color.White;
            ctlHeader.FontSize = 8;*
            */
            uiChildren.Add(ctlHeader);
        }

        /*
        /// <summary>
        /// Fill the visible list with controls...
        /// </summary>
        /// <param name="posItems"></param>
        internal void FillPosOrderItemsList(ICollection<PosItem>? posItems)
        {
            var uiChildren = spListOfPosOrderItems.Children;
            _menuUiItems = null;

            var menuItems = new List<uiPosOrderListItem>();

            //Get rid of all the existing items
            uiChildren.Clear();
            if ((posItems == null) || (posItems.Count == 0))
            {

                //We could show a fancier custom control here to indicate
                //that there are no controls
                var txtCtl = new Label();
                txtCtl.Text = "No POS Items in list";

                uiChildren.Add(txtCtl);
                return;
            }


            //==============================================
            //Add a control for each guest
            //==============================================
            foreach (var thisPosOrderItem in posItems)
            {
                //Create the control
                var ctlPosOrderItemListItem = new uiPosOrderListItem(thisPosOrderItem);
                //Hook up the event to listen to it here...
                ctlPosOrderItemListItem.PosItemSelected += EventHander_PosOrderItemSelectedEventHandler;

                //Listen to events of the order being updated
                ctlPosOrderItemListItem.OrderUpdated += CtlPosOrderItemListItem_OrderUpdated;

                uiChildren.Add(ctlPosOrderItemListItem);

                //Add it to our logical list of menu items we care about
                menuItems.Add(ctlPosOrderItemListItem);
            }

            _menuUiItems = menuItems;

            //Update the totals
            RecalculateAndUpdateOrderSummaryText();
        }
        */

        /// <summary>
        /// Called whenever any of the order sub-items is updates
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void CtlPosOrderItemListItem_OrderUpdated(object sender, EventArgs e)
        {
            RecalculateAndUpdateOrderSummaryText();
        }


        /// <summary>
        /// Sum up the menu items
        /// </summary>
        private void RecalculateAndUpdateOrderSummaryText()
        {
            decimal runningTotal = 0;
            foreach (var ctlMenuItem in _menuUiItems)
            {
                runningTotal += ctlMenuItem.CalculatePrice_PreTax();
            }

            txtSummaryText.Text = "Pre tax/tip total: " + GlobalStrings.FormatCurrency(runningTotal);
        }

    }
}
