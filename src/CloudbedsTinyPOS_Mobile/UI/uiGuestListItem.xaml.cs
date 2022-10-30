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
    public partial class uiGuestListItem : StackLayout 
    {
        private readonly CloudbedsGuest _guest = null;

        bool _isSelected;
        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    UpdateIsSelectedUi();
                }
            }
        }

        internal CloudbedsGuest Guest
        {
            get
            {
                return _guest;
            }
        }

        /// <summary>
        /// Delegate and Event for when a Guest gets clicked/selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal delegate void GuestSelectedEventHandler(object sender, GuestSelectedEventArgs e);
        internal event GuestSelectedEventHandler GuestSelected;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="guest"></param>
        internal uiGuestListItem(CloudbedsGuest guest) : this()
        {
            _guest = guest;

            UserControl_Loaded();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public uiGuestListItem()
        {
            InitializeComponent();
            UpdateIsSelectedUi();
        }

        private void UpdateIsSelectedUi()
        {
            if (_isSelected)
            {
                txtIsSelectedMarker.Text = "✓";
            }
            else
            {
                txtIsSelectedMarker.Text = "";
            }
        }

        private void UserControl_Loaded()
        {
            var guest = _guest;
            //Fill in the UI elements
            if (guest != null)
            {
                txtGuestName.Text = guest.Guest_Name;
                txtGuestRoomNumber.Text = guest.Room_Name;
                //txtGuestPhone.Text = guest.Guest_CellPhone;
            }
        }


        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            //Raise the event
            GuestSelected(this, new GuestSelectedEventArgs(_guest));
        }
    }
}
