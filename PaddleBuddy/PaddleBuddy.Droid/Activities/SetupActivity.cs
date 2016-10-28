using System.Threading.Tasks;
using Android.App;
using Android.OS;
using PaddleBuddy.Core;
using PaddleBuddy.Core.Models.Messages;
using PaddleBuddy.Core.Services;
using PaddleBuddy.Droid.Services;

namespace PaddleBuddy.Droid.Activities
{
    [Activity(Label = "PaddleBuddy", MainLauncher = true, Icon = "@drawable/icon")]
    public class SetupActivity : Activity
    {
        private bool _mainActivityStarted;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            _mainActivityStarted = false;
            SysPrefs.Device = SysPrefs.Devices.Android;
            SetContentView(Resource.Layout.activity_setup);
            MessengerService.Messenger.Register<DbReadyMessage>(this, StartMainActivity);
            Task.Run(() => Setup());
        }

        private async void Setup()
        {
            await DatabaseService.GetInstance().Setup(true);
        }

        private void StartMainActivity(object e)
        {
            if (!_mainActivityStarted)
            {
                StartActivity(typeof(MainActivity));
            }
            _mainActivityStarted = true;
        }
    }
}

