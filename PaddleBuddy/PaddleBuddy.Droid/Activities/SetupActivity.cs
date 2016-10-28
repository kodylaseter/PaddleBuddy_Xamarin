using System.Threading.Tasks;
using Android.App;
using Android.OS;
using PaddleBuddy.Core;
using PaddleBuddy.Droid.Services;

namespace PaddleBuddy.Droid.Activities
{
    [Activity(Label = "PaddleBuddy", MainLauncher = true, Icon = "@drawable/icon")]
    public class SetupActivity : Activity
    {
        int count = 1;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SysPrefs.Device = SysPrefs.Devices.Android;
            // Set our view from the "main" layout resource

            SetContentView(Resource.Layout.activity_setup);
            Task.Run(() => Setup());
        }

        private async void Setup()
        {
            await DatabaseService.GetInstance().Setup();
        }
    }
}

