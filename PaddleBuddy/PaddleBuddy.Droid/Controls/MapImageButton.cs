using System;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Support.V4.Content;
using Android.Util;
using Android.Widget;

namespace PaddleBuddy.Droid.Controls
{
    public class MapImageButton : ImageButton
    {
        private const int BUTTON_SIZE = 40;
        private const int BUTTON_MARGIN = 3;

        private void Initialize()
        {
            SetColorFilter(new Color(ContextCompat.GetColor(Context, Resource.Color.map_button_contents)));
            SetBackgroundResource(Resource.Drawable.map_image_button_bg);
            Elevation = 5;
        }

        public MapImageButton(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
            Initialize();
        }

        public MapImageButton(Context context) : base(context)
        {
            Initialize();
        }

        public MapImageButton(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Initialize();
        }

        public MapImageButton(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            Initialize();
        }

        public MapImageButton(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
            Initialize();
        }
    }
}