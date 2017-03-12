using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace PaddleBuddy.Droid.Controls
{
    public class ProgressBarOverlay : RelativeLayout
    {
        private TextView TextView { get; set; }
        private ProgressBar ProgressBar { get; set; }
        private void Initialize()
        {
            var view = Inflate(Context, Resource.Layout.view_progressbar, null);
            ProgressBar = view.FindViewById<ProgressBar>(Resource.Id.progress_bar);

            TextView = new TextView(Context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent),
                Text = "notset!"
            };
            ((LayoutParams)TextView.LayoutParameters).AddRule(LayoutRules.Below, ProgressBar.Id);

            AddView(TextView);
            AddView(ProgressBar);
        }

        public void SetText(string str)
        {
            if (str != null)
            {
                TextView.Text = str;
            }
        }

#region constructors

        private void HandleAttributes(IAttributeSet attrs)
        {
            var typedArray = Context.Theme.ObtainStyledAttributes(attrs, Resource.Styleable.ProgressBarOverlay, 0, 0);
            var text = typedArray.GetString(Resource.Styleable.ProgressBarOverlay_text);
            SetText(text);
        }
    
        public ProgressBarOverlay(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
            Initialize();
        }

        public ProgressBarOverlay(Context context) : base(context)
        {
            Initialize();
        }

        public ProgressBarOverlay(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Initialize();
            HandleAttributes(attrs);
        }

        public ProgressBarOverlay(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            Initialize();
            HandleAttributes(attrs);
        }

        public ProgressBarOverlay(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
            Initialize();
            HandleAttributes(attrs);
        }
        #endregion

    }
}