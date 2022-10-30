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
    public partial class uiPosOrderListItemHeader : StackLayout
    {

        /// <summary>
        /// Constructor
        /// </summary>
        public uiPosOrderListItemHeader()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public uiPosOrderListItemHeader(string text) : this()
        {
            txtHeader.Text = text;
        }

    }
}
