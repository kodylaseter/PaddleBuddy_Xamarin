using System;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Runtime;
using Android.Support.V4.Content;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace PaddleBuddy.Droid.Controls
{
    public class ClearEditText : RelativeLayout
    {
        public EditText EditText { get; set; }
        private ImageButton ImageButton { get; set; }

        private void Initialize()
        {
            
            LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

            ImageButton = new ImageButton(Context)
            {
                Id = GenerateViewId(),
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
            };
            ImageButton.SetImageResource(Resource.Drawable.ic_clear_white_24dp);
            ImageButton.SetColorFilter(new Color(ContextCompat.GetColor(Context, Resource.Color.gray)));
            ImageButton.Visibility = ViewStates.Gone;
            ((LayoutParams)ImageButton.LayoutParameters).AddRule(LayoutRules.AlignParentRight);
            var typedValue = new TypedValue();
            Context.Theme.ResolveAttribute(Resource.Attribute.selectableItemBackgroundBorderless, typedValue, true);
            ImageButton.SetBackgroundResource(typedValue.ResourceId);
            ImageButton.Click += OnButtonClicked;
            
            EditText = new EditText(Context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            EditText.TextChanged += OnTextChanged;
            ((LayoutParams)EditText.LayoutParameters).AddRule(LayoutRules.LeftOf, ImageButton.Id);
            EditText.SetBackgroundColor(new Color(ContextCompat.GetColor(Context, Android.Resource.Color.Transparent)));
            EditText.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.black)));

            AddView(ImageButton);
            AddView(EditText);
        }

        public void SetHint(string str)
        {
            if (str != null)
            {
                EditText.Hint = str;
            }
        }

        private void OnButtonClicked(object sender, EventArgs e)
        {
            EditText.Text = "";
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            ImageButton.Visibility = string.IsNullOrWhiteSpace(e.Text.ToString()) ? ViewStates.Gone : ViewStates.Visible;
        }

#region constructors
        public ClearEditText(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
            Initialize();
        }

        public ClearEditText(Context context) : base(context)
        {
            Initialize();
        }

        public ClearEditText(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Initialize();
        }

        public ClearEditText(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            Initialize();
        }

        public ClearEditText(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
            Initialize();
        }
#endregion
    }
}