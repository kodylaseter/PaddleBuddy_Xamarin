using System.Collections.Generic;
using Android.App;
using Android.Views;
using Android.Widget;
using PaddleBuddy.Core.Models;
using PaddleBuddy.Core.Services;
using PaddleBuddy.Droid.Controls;

namespace PaddleBuddy.Droid.Adapters
{
    public class TripSummaryAdapter : BaseAdapter<TripSummary>
    {
        private readonly Activity _activity;
        public List<TripSummary> TripSummaries { get; set; }

        public TripSummaryAdapter(Activity activity)
        {
            _activity = activity;
            UpdateTripSummaries();
        }

        private void UpdateTripSummaries()
        {
            TripSummaries = DatabaseService.GetInstance().TripSummaries;
        }

        public override long GetItemId(int position)
        {
            return TripSummaries[position].Id;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var currentSummary = TripSummaries[position];
            return convertView ?? new TripSummaryCardView(_activity, currentSummary, true);
        }

        public override int Count => TripSummaries.Count;

        public override TripSummary this[int position]
        {
            get { throw new System.NotImplementedException(); }
        }
    }
}