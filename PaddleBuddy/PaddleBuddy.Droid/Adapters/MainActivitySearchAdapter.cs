using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Java.Lang;
using PaddleBuddy.Core.Models;
using PaddleBuddy.Core.Services;
using PaddleBuddy.Droid.Controls;
using PaddleBuddy.Droid.Utilities;

namespace PaddleBuddy.Droid.Adapters
{
    public class MainActivitySearchAdapter : RecyclerView.Adapter
    {
        public SearchService SearchService { get; }
        public Filter Filter { get; set; }
        private readonly Activity _activity;


        public MainActivitySearchAdapter(Activity activity)
        {
            _activity = activity;
            SearchService = new SearchService();
            InitData();
            Filter = new SearchItemFilter(this);
        }

        private void InitData()
        {
            SearchService.AddData(DatabaseService.GetInstance().Points.ToArray<object>());
            //SearchService.AddData(DatabaseService.GetInstance().Rivers.ToArray<object>());
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var searchItemViewHolder = holder as SearchItemViewHolder;
            searchItemViewHolder?.SearchItemView.UpdateData(SearchService.Items[position]);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var itemView = new SearchItemView(_activity);
            return new SearchItemViewHolder(itemView);
        }

        public override int ItemCount => SearchService.Items.Count;

        private class SearchItemViewHolder : RecyclerView.ViewHolder
        {
            public SearchItemView SearchItemView { get; }
            public SearchItemViewHolder(SearchItemView searchItemView) : base(searchItemView)
            {
                SearchItemView = searchItemView;
            }
        }

        /// <summary>
        /// Custom filter inspired by cheesebaron
        /// https://gist.github.com/Cheesebaron/9838325
        /// </summary>
        private class SearchItemFilter : Filter
        {
            private readonly MainActivitySearchAdapter _adapter;

            public SearchItemFilter(MainActivitySearchAdapter adapter)
            {
                _adapter = adapter;
            }

            protected override FilterResults PerformFiltering(ICharSequence constraint)
            {
                var returnObj = new FilterResults();
                var results = new List<SearchItem>();
                if (_adapter.SearchService.OriginalData == null || constraint == null)
                {
                    return returnObj;
                }
                if (_adapter.SearchService.OriginalData.Any())
                {
                    results.AddRange(_adapter.SearchService.Filter(constraint.ToString()));
                }
                else
                {
                    throw new NotImplementedException();
                }
                returnObj.Values = FromArray(results.Select(a => a.ToJavaObject()).ToArray());
                returnObj.Count = results.Count;
                constraint.Dispose();
                return returnObj;
            }

            protected override void PublishResults(ICharSequence constraint, FilterResults results)
            {
                using (var values = results.Values)
                {
                    _adapter.SearchService.Items =
                        values.ToArray<Java.Lang.Object>().Select(a => a.ToNetObject<SearchItem>()).ToList();

                    _adapter.NotifyDataSetChanged();
                    constraint.Dispose();
                    results.Dispose();
                }
            }
        }
    }
}