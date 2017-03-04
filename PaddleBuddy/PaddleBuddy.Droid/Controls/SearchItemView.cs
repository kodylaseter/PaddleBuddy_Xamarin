using Android.Content;
using Android.Views;
using Android.Widget;
using PaddleBuddy.Core.Models;

namespace PaddleBuddy.Droid.Controls
{
    public class SearchItemView : LinearLayout
    {
        private TextView ItemTextView { get; set; }

        private void Initialize()
        {
            LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            Orientation = Orientation.Horizontal;
            ItemTextView = new TextView(Context)
            {
                LayoutParameters =
                    new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            ItemTextView.SetTextAppearance(Resource.Style.textMediumWhite);
            AddView(ItemTextView);
        }

        public void UpdateData(SearchItem searchItem)
        {
            ItemTextView.Text = searchItem.SearchString;
        }

        public SearchItemView(Context context) : base(context)
        {
            Initialize();
        }
    }
}