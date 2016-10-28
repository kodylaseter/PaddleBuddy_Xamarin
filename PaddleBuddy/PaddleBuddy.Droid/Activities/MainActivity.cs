using Android.App;
using Android.OS;

namespace PaddleBuddy.Droid.Activities
{
    [Activity(Label = "MainActivity")]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_main);
        }
    }
}