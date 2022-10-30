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
    /// This UI CONTROL shows options to Reset the current POS Ordering 
    /// </summary>
    public partial class uiPosOrderStartPage : StackLayout
    {

        /// <summary>
        /// Delegate and Event for Re-Starting the order
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal delegate void PosRestartOrderEventHandler(object sender, EventArgs e);
        internal event PosRestartOrderEventHandler PosRestartOrder;


        /// <summary>
        /// Constructor
        /// </summary>
        public uiPosOrderStartPage()
        {
            InitializeComponent();
        }


        /// <summary>
        /// Fire the restart-requested event...
        /// </summary>
        private void FireEvent_PosRestartOrder()
        {
            var evt = PosRestartOrder;
            if(evt != null)
            {
                evt(this, EventArgs.Empty);
            }
        }

        private void btnResetPOS_Clicked(object sender, EventArgs e)
        {
            FireEvent_PosRestartOrder();
            txtResetResponse.Text = "Restarting the order;";
        }
    }
}
