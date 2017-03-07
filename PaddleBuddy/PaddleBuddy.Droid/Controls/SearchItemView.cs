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
            AddView(IconImageView);
            AddView(ItemTextView);
        }

        public void UpdateData(SearchItem searchItem)
        {
            SearchItem = searchItem;
            ItemTextView.Text = searchItem.SearchString;
            IconImageView.SetImageResource(Resource.Drawable.ic_place_black_24dp);
            IconImageView.SetColorFilter(new Color(Resource.Color.colorAccent), PorterDuff.Mode.SrcAtop);
        }

        public SearchItemView(Context context) : base(context)
        {
            Initialize();
        }
    }
}