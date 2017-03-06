using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Views;
using Android.Widget;
using Java.Lang;
using PaddleBuddy.Core.Models;
using PaddleBuddy.Core.Services;
using PaddleBuddy.Droid.Controls;
using PaddleBuddy.Droid.Utilities;

namespace PaddleBuddy.Droid.Adapters
{
    public class MainActivitySearchAdapter : BaseAdapter<SearchItem>
    {
        public SearchService SearchService { get; }
        private readonly Activity _activity;
        public Filter Filter { get; private set; }

        public MainActivitySearchAdapter(Activity activity)
        {
            _activity = activity;
            SearchService = new SearchService();
            Filter = new SearchItemFilter(this);
            SearchService.AddData(DatabaseService.GetInstance().Points.ToArray<object>());
            SearchService.AddData(DatabaseService.GetInstance().Rivers.ToArray<object>());
        }

        public override int Count => SearchService.Items.Count;

        public override long GetItemId(int position)
        {
            return SearchService.Items[position].Id;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var view = convertView != null && convertView.GetType() == typeof(SearchItemView) ? 
                       convertView : new SearchItemView(_activity);
            ((SearchItemView)view).UpdateData(SearchService.Items[position]);
            return view;
        }

        public override SearchItem this[int position]
        {
            get { throw new System.NotImplementedException(); }
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