using System.Linq;
using Android.Views;
using Android.Widget;
using PaddleBuddy.Core.Models;
using PaddleBuddy.Core.Services;

namespace PaddleBuddy.Droid.Adapters
{
    public class MainActivitySearchAdapter : BaseAdapter<SearchItem>
    {

        private SearchService _searchService;

        public MainActivitySearchAdapter()
        {
            _searchService = new SearchService();
            _searchService.AddData(DatabaseService.GetInstance().Points.ToArray<object>());
            _searchService.AddData(DatabaseService.GetInstance().Rivers.ToArray<object>());
        }

        public override int Count => _searchService.Data.Count;

        public override long GetItemId(int position)
        {
            return 
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            throw new System.NotImplementedException();
        }

        public override SearchItem this[int position]
        {
            get { throw new System.NotImplementedException(); }
        }
    }
}