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
    public partial class uiGuestList : StackLayout
    {
        /// Delegate and Event for when a Guest gets clicked/selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal delegate void GuestSelectedEventHandler(object sender, GuestSelectedEventArgs e);
        internal event GuestSelectedEventHandler GuestSelected;

        CloudbedsGuest _selectedGuest = null;

        /// <summary>
        /// True if the guest list was filled at least once
        /// </summary>
        SimpleLatch _wasGuestListStocked = new SimpleLatch();

        /// <summary>
        /// UI items for each guest
        /// </summary>
        List<uiGuestListItem> _ctlGuestListItems = new List<uiGuestListItem>();

        /// <summary>
        /// True if the guest list was filled at least once
        /// </summary>
        public bool IsGuestsListStocked
        {
            get
            {
                return _wasGuestListStocked.Value;
            }
        }

        internal CloudbedsGuest SelecedGuest
        {
            get
            {
                return _selectedGuest;
            }
            set
            {
                if(_selectedGuest == value)
                {
                    return;
                }
                _selectedGuest = value;
                UpdateSelectedGuestUi();
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="guests"></param>
        internal uiGuestList(ICollection<CloudbedsGuest> guests) : this()
        {
            FillGuestsList(guests);
            UpdateSelectedGuestUi();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public uiGuestList()
        {
            InitializeComponent();
        }


        /// <summary>
        /// Clear out any guest selection
        /// </summary>
        public void ClearGuestSelection()
        {
            _selectedGuest = null;
            UpdateSelectedGuestUi();
        }

        /// <summary>
        /// Called when an individual Guest list item is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void EventHander_GuestSelectedEventHandler(object sender, GuestSelectedEventArgs e)
        {
            _selectedGuest = e.Guest;
            UpdateSelectedGuestUi();

            //Bubble the event upward
            var funcFireEvent = this.GuestSelected;
            if(funcFireEvent != null)
            {
                funcFireEvent(this, e);
            }
        }

        /// <summary>
        /// Update the selected state ui for each guest item
        /// </summary>
        private void UpdateSelectedGuestUi()
        {
            //=======================================================
            //Update the summary text
            //=======================================================
            var selectedGuest = _selectedGuest;
            if(selectedGuest != null)
            {
                txtSummaryText.Text = selectedGuest.Guest_Name + "\r\n(" + selectedGuest.Room_Name + ")";
            }
            else
            {
                txtSummaryText.Text = "Select a Guest";
            }

            //=======================================================
            //Update the visual list of guests
            //=======================================================
            var listCtls = _ctlGuestListItems;
            if (listCtls == null)
            {
                return;
            }

            //Update each of the itms
            foreach (var thisCtl in listCtls)
            {
                thisCtl.IsSelected = (thisCtl.Guest == _selectedGuest);
            }
        }


        /// <summary>
        /// Fill the visible list with controls...
        /// </summary>
        /// <param name="guests"></param>
        internal void FillGuestsList(ICollection<CloudbedsGuest> guests)
        {
            _wasGuestListStocked.Trigger();

            var uiChildren = spListOfGuests.Children;
            _ctlGuestListItems = new List<uiGuestListItem>();

            //Get rid of all the existing items
            uiChildren.Clear();
            if ((guests == null) || (guests.Count == 0))
            {

                //We could show a fancier custom control here to indicate
                //that there are no controls
                var txtCtl = new Label();
                txtCtl.Text = "No guests in list";

                uiChildren.Add(txtCtl);
                return;
            }


            //==============================================
            //Add a control for each guest
            //==============================================
            foreach (var thisGuest in guests)
            {
                //Create the control
                var ctlGuestListItem = new uiGuestListItem(thisGuest);
                //Hook up the event to listen to it here...
                ctlGuestListItem.GuestSelected += EventHander_GuestSelectedEventHandler;

                uiChildren.Add(ctlGuestListItem);
                _ctlGuestListItems.Add(ctlGuestListItem);
            }

        }

    }
}
