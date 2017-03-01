using System;
using Android.Content;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using PaddleBuddy.Core.Models;
using PaddleBuddy.Core.Services;
using PaddleBuddy.Core.Utilities;

namespace PaddleBuddy.Droid.Controls
{
    public class TripSummaryCardView : CardView
    {
        public bool IsExpandable { get; set; }
        public TripSummary TripSummary { get; set; }
        private TextView _titleTextView;
        private View _detailsGroup;
        private void Initialize()
        {
            UseCompatPadding = true;
            SetContentPadding(5, 5, 5, 5);
            AddView(Inflate(Context, Resource.Layout.cardview_tripsummary, null));

            var titleString = DatabaseService.GetInstance().GetRiverName(TripSummary.RiverId);
            titleString += " - ";
            titleString += TripSummary.StartDateTime.ToShortDateString();
            var timesString = TripSummary.StartDateTime.ToStringHrsMinsAmPm();
            timesString += " - " + TripSummary.EndTime.ToStringHrsMinsAmPm();
            timesString += " | " + TripSummary.EndTime.Subtract(TripSummary.StartDateTime).ToStringHrsOrMins();
            _titleTextView = FindViewById<TextView>(Resource.Id.tripsummary_title);
            _titleTextView.Text = titleString;
            FindViewById<TextView>(Resource.Id.tripsummary_times).Text =
                timesString;
            _detailsGroup = FindViewById(Resource.Id.tripsummary_details);

            if (IsExpandable)
            {
                _detailsGroup.Visibility = ViewStates.Gone;
            }
        }

        public void ToggleExpansion()
        {
            _detailsGroup.Visibility = _detailsGroup.Visibility == ViewStates.Gone ? ViewStates.Visible : ViewStates.Gone;
        }

        //private void OnCardClicked(object sender, EventArgs e)
        //{
        //    _detailsGroup.Visibility = _detailsGroup.Visibility == ViewStates.Gone
        //        ? ViewStates.Visible
        //        : ViewStates.Gone;
        //}


        public TripSummaryCardView(Context context, TripSummary tripSummary, bool isExpandable) : base(context)
        {
            IsExpandable = isExpandable;
            TripSummary = tripSummary;
            Initialize();
        }

        #region old constructors

        public TripSummaryCardView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
            throw new NotImplementedException();
        }

        public TripSummaryCardView(Context context) : base(context)
        {
            throw new NotImplementedException();
        }

        public TripSummaryCardView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            throw new NotImplementedException();
        }

        public TripSummaryCardView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            throw new NotImplementedException();
        }
#endregion
    }
}