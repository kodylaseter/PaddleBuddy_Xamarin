using Android.Content;
using Android.Graphics;
using Android.Support.V4.Content;
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
            AddView(Inflate(Context, Resource.Layout.view_searchitem, null));
            ItemTextView = FindViewById<TextView>(Resource.Id.searchitem_textview);
            IconImageView = FindViewById<ImageView>(Resource.Id.searchitem_imageview);
            IconImageView.SetColorFilter(new Color(ContextCompat.GetColor(Context, Resource.Color.gray)), PorterDuff.Mode.SrcAtop);
        }

        public void UpdateData(SearchItem searchItem)
        {
            SearchItem = searchItem;
            ItemTextView.Text = searchItem.Title;
            IconImageView.SetImageResource(Resource.Drawable.ic_place_black_24dp);
        }

        public SearchItemView(Context context) : base(context)
        {
            Initialize();
        }
    }
}