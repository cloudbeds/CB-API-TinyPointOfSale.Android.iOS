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
    /// This UI CONTROL shows the list of hotel guests
    /// </summary>
    public partial class uiPosOrderSummary : StackLayout, IUpdateStatusText
    {

        private readonly CloudbedsGuest _selecedGuest;
        PosOrderManager _posOrderManager;

        /// Delegate and Event for when a Guest gets clicked/selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal delegate void CommitTransactionEventHandler(object sender, EventArgs e);
        internal event CommitTransactionEventHandler CommitTransaction;

        /// <summary>
        /// Constructor
        /// </summary>
        public uiPosOrderSummary()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="posOrderManager"></param>
        internal uiPosOrderSummary(PosOrderManager posOrderManager, CloudbedsGuest selecedGuest) : this()
        {
            _selecedGuest = selecedGuest;
            _posOrderManager = posOrderManager;

            FillUiFromOrderDetails(posOrderManager, selecedGuest);
        }

        /// <summary>
        /// Sets the summary text (called by our parent contro)
        /// </summary>
        /// <param name="text"></param>
        public void SetStatusText(string text)
        {
            txtCloudbedsSubmitResponse.Text = text;
        }

        /// <summary>
        /// Fill in the UI
        /// </summary>
        /// <param name="orderManager"></param>
        /// <param name="selectedGuest"></param>
        private void FillUiFromOrderDetails(PosOrderManager orderManager, CloudbedsGuest selectedGuest)
        {
            if (selectedGuest == null)
            {
                txtPOSSubmit_GuestName.Text = "PLEASE SELECT GUEST";
                txtPOSSubmit_GuestName.BackgroundColor = Color.FromRgb(1, 1, 0.5);
            }
            else
            {
                txtPOSSubmit_GuestName.Text = selectedGuest.Guest_Name + " (" + selectedGuest.Room_Name + ")";
                txtPOSSubmit_GuestName.BackgroundColor = Color.White;
            }

            var calculatedTotals = orderManager.CalculateTotals();

            txtPOSSubmit_ItemsOrdered.Text = calculatedTotals.NumberItems.ToString();
            txtPOSSubmit_SubTotal.Text = GlobalStrings.FormatCurrency(calculatedTotals.TotalItemsPrice);
            txtPOSSubmit_Tax.Text = GlobalStrings.FormatCurrency(calculatedTotals.TotalTax);


            string gratuityFractionText = GlobalStrings.PercentText(calculatedTotals.Gratuity, calculatedTotals.TotalItemsPrice + calculatedTotals.TotalTax);
            if (!string.IsNullOrWhiteSpace(gratuityFractionText))
            {
                gratuityFractionText = " (" + gratuityFractionText + ")";
            }
            txtPOSSubmit_Gratuity.Text = GlobalStrings.FormatCurrency(calculatedTotals.Gratuity) + gratuityFractionText;

            txtPOSSubmit_Total.Text = GlobalStrings.FormatCurrency(calculatedTotals.GrandTotal);

        }


        /// <summary>
        /// User wants to submit the transaction...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSubmitToCloudbeds_Clicked(object sender, EventArgs e)
        {

            var selectedGuest = _selecedGuest;
            if (selectedGuest == null)
            {
                txtCloudbedsSubmitResponse.Text = "No guest selected. Cannot post charge";
                return;
            }

            var posOrderManager = _posOrderManager;
            if (posOrderManager == null)
            {
                txtCloudbedsSubmitResponse.Text = "No point of sale data. Cannot post charge";
                return;
            }


            var calculatedTotals = posOrderManager.CalculateTotals();
            if (calculatedTotals.GrandTotal <= 0)
            {
                txtCloudbedsSubmitResponse.Text = "No charges to post";
                return;
            }

            //Set a note that will be shown on the order items..
            posOrderManager.DefaultNoteForLineItems =
                "Guest: " + selectedGuest.Guest_Name + ", Date: " + DateTime.Now.ToString();

            //Send the signal to our parent to commit the transaction
            FireEvent_CommitTransaction();
        }

        /// <summary>
        /// Fire the event
        /// </summary>
        void FireEvent_CommitTransaction()
        {
            var evt = this.CommitTransaction;
            if(evt != null)
            {
                evt(this, EventArgs.Empty);
            }
        }
    }
}
