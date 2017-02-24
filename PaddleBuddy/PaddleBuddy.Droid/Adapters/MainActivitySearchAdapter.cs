using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Views;
using Android.Widget;
using Java.Lang;
using PaddleBuddy.Core.Models;
using PaddleBuddy.Core.Services;
using PaddleBuddy.Droid.Utilities;

namespace PaddleBuddy.Droid.Adapters
{
    public class MainActivitySearchAdapter : BaseAdapter<SearchItem>
    {
        public SearchService SearchService { get; private set; }
        private Activity _activity;
        public Filter Filter { get; private set; }

        public MainActivitySearchAdapter(Activity activity)
        {
            _activity = activity;
            SearchService = new SearchService();
            SearchService.AddData(DatabaseService.GetInstance().Points.ToArray<object>());
            SearchService.AddData(DatabaseService.GetInstance().Rivers.ToArray<object>());
        }

        public override int Count => SearchService.Data.Count;

        public override long GetItemId(int position)
        {
            var element = SearchService.Data.ElementAtOrDefault(position);
            return element?.Id ?? int.MaxValue;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var view = convertView ??
                       _activity.LayoutInflater.Inflate(Resource.Layout.list_item_searchitem, parent, false);
            view.FindViewById<TextView>(Resource.Id.text1).Text = SearchService.Data[position].SearchString;
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
                if (_adapter.SearchService.Data == null || constraint == null)
                {
                    return returnObj;
                }
                if (_adapter.SearchService.Data.Any())
                {
                    results.AddRange(_adapter.SearchService.Filter(constraint.ToString()));
                }
                returnObj.Values = FromArray(results.Select(a => a.ToJavaObject()).ToArray());
                returnObj.Count = results.Count;
                constraint.Dispose();
                return returnObj;
            }

            protected override void PublishResults(ICharSequence constraint, FilterResults results)
            {
                throw new System.NotImplementedException();
            }
        }
    }


}