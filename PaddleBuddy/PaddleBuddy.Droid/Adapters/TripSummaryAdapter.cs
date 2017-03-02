using System.Collections.Generic;
using Android.App;
using Android.Support.V7.Widget;
using Android.Views;
using PaddleBuddy.Core.Models;
using PaddleBuddy.Core.Services;
using PaddleBuddy.Droid.Controls;

namespace PaddleBuddy.Droid.Adapters
{
    public class TripSummaryAdapter : RecyclerView.Adapter
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
            TripSummaries = DatabaseService.GetInstance().GetSortedSummaries();
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var tripSummaryViewHolder = holder as TripSummaryViewHolder;
            tripSummaryViewHolder?.CardView.UpdateData(TripSummaries[position]);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            TripSummaryCardView itemView = new TripSummaryCardView(_activity, true);
            return new TripSummaryViewHolder(itemView);
        }

        public override int ItemCount => TripSummaries.Count;

        private class TripSummaryViewHolder : RecyclerView.ViewHolder
        {
            public TripSummaryCardView CardView { get; }
            public TripSummaryViewHolder(TripSummaryCardView cardView) : base(cardView)
            {
                CardView = cardView;
            }
        }
    }
}