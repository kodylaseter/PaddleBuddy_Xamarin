using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using PaddleBuddy.Core.Models;

namespace PaddleBuddy.Droid.Controls
{
    public class SearchItemCardView : CardView
    {
        private TextView ItemTextView { get; set; }
        private SearchItem SearchItem { get; set; }

        private void Initialize()
        {
            UseCompatPadding = true;
            SetContentPadding(5, 5, 5, 5);
            LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            AddView(Inflate(Context, Resource.Layout.cardview_searchitem, null));

            ItemTextView = FindViewById<TextView>(Resource.Id.cardview_item_text);
        }

        public void UpdateData(SearchItem searchItem)
        {
            SearchItem = searchItem;
            ItemTextView.Text = SearchItem.SearchString;
        }

        public SearchItemCardView(Context context) : base(context)
        {
            Initialize();
        }
    }
}