using Android.Content;
using Android.Graphics;
using Android.Support.V4.Content;
using Android.Views;
using Android.Widget;
using PaddleBuddy.Core.Models;

namespace PaddleBuddy.Droid.Controls
{
    public class SearchItemView : LinearLayout
    {
        public SearchItem SearchItem { get; set; }
        private TextView ItemTextView { get; set; }
        private ImageView IconImageView { get; set; }

        private void Initialize()
        {
            LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            Orientation = Orientation.Horizontal;
            var padding = (int) Resources.GetDimension(Resource.Dimension.search_inner_padding);
            SetPadding(padding, padding, padding, padding);
            IconImageView = new ImageView(Context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent)
            };
            ItemTextView = new TextView(Context)
            {
                LayoutParameters =
                    new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            ItemTextView.SetTextAppearance(Resource.Style.textMediumGray);
            ItemTextView.Gravity = GravityFlags.CenterVertical;
            var dividerView = new View(Context)
            {
                LayoutParameters = new LayoutParams(1, 8)
            };
            dividerView.SetBackgroundColor(new Color(ContextCompat.GetColor(Context, Resource.Color.search_divider)));
            AddView(IconImageView);
            AddView(dividerView);
            AddView(ItemTextView);
        }

        public void UpdateData(SearchItem searchItem)
        {
            SearchItem = searchItem;
            ItemTextView.Text = searchItem.SearchString;
            IconImageView.SetImageResource(Resource.Drawable.ic_place_black_24dp);
            IconImageView.SetColorFilter(new Color(ContextCompat.GetColor(Context, Resource.Color.gray)), PorterDuff.Mode.SrcAtop);
        }

        public SearchItemView(Context context) : base(context)
        {
            Initialize();
        }
    }
}