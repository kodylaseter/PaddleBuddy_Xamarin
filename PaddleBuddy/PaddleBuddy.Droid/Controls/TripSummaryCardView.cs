using Android.Content;
using Android.Support.V7.Widget;
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
        private TextView TimesTextView { get; set; }
        private TextView TitleTextView { get; set; }
        private View DetailsGroup { get; set; }
        private LinearLayout TitleGroup { get; set; }

        private void Initialize()
        {
            UseCompatPadding = true;
            SetContentPadding(5, 5, 5, 5);
            LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            AddView(Inflate(Context, Resource.Layout.cardview_tripsummary, null));

            TitleTextView = FindViewById<TextView>(Resource.Id.tripsummary_title);
            TimesTextView = FindViewById<TextView>(Resource.Id.tripsummary_times);
            DetailsGroup = FindViewById(Resource.Id.tripsummary_details);
            TitleGroup = FindViewById<LinearLayout>(Resource.Id.tripsummary_title_group);
            if (IsExpandable)
            {
                DetailsGroup.Visibility = ViewStates.Gone;
                TitleGroup.Clickable = true;
                TitleGroup.Click += (s, e) =>
                {
                    ToggleExpansion();
                };
            }
        }

        public void UpdateData(TripSummary tripSummary)
        {
            TripSummary = tripSummary;
            var titleString = DatabaseService.GetInstance().GetRiverName(TripSummary.RiverId);
            titleString += " - ";
            titleString += TripSummary.StartDateTime.ToShortDateString();
            var timesString = TripSummary.StartDateTime.ToStringHrsMinsAmPm();
            timesString += " - " + TripSummary.EndTime.ToStringHrsMinsAmPm();
            timesString += " | " + TripSummary.EndTime.Subtract(TripSummary.StartDateTime).ToStringHrsMinsText();
            TimesTextView.Text = timesString;
            TitleTextView.Text = titleString;
        }

        public void ToggleExpansion()
        {
            DetailsGroup.Visibility = DetailsGroup.Visibility == ViewStates.Gone ? ViewStates.Visible : ViewStates.Gone;
        }

        public TripSummaryCardView(Context context, bool isExpandable) : base(context)
        {
            IsExpandable = isExpandable;
            Initialize();
        }
    }
}