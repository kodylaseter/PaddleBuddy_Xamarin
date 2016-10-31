using System.Threading.Tasks;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.App;
using PaddleBuddy.Core;
using PaddleBuddy.Core.Models;
using PaddleBuddy.Core.Models.Messages;
using PaddleBuddy.Core.Services;
using PaddleBuddy.Droid.Services;

namespace PaddleBuddy.Droid.Activities
{
    [Activity(Label = "PaddleBuddy", MainLauncher = true, Icon = "@drawable/icon", Theme = "@style/AppTheme.NoActionBar")]
    public class SetupActivity : AppCompatActivity
    {
        private bool _mainActivityStarted;
        private bool _dbReady;
        private bool _locationPermissionApproved;
        private bool _locationReady;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            _mainActivityStarted = false;
            _dbReady = false;
            _locationPermissionApproved = false;
            _locationReady = false;
            SysPrefs.Device = SysPrefs.Devices.Android;
            SetContentView(Resource.Layout.activity_setup);
            MessengerService.Messenger.Register<DbReadyMessage>(this, DbReadyReceived);
            MessengerService.Messenger.Register<PermissionMessage>(this, PermissionMessageReceived);
            MessengerService.Messenger.Register<LocationUpdatedMessage>(this, LocationUpdatedReceived);
        }

        protected override void OnResume()
        {
            base.OnResume();
            Setup();
        }

        private void Setup()
        {

            Task.Run(() => DatabaseService.GetInstance().Setup(true));
            Task.Run(() => PermissionService.SetupLocation(this));
        }

        private void TryToStartMainActivity()
        {
            if (!_mainActivityStarted & _dbReady & _locationPermissionApproved & _locationReady)
            {
                _mainActivityStarted = true;
                MessengerService.Messenger.Unregister<DbReadyMessage>(this);
                MessengerService.Messenger.Unregister<PermissionMessage>(this);
                MessengerService.Messenger.Unregister<LocationUpdatedMessage>(this);
                StartActivity(typeof(MainActivity));
            }
        }

        private void LocationUpdatedReceived(LocationUpdatedMessage obj)
        {
            LocationReady = true;
        }

        private void DbReadyReceived(DbReadyMessage obj)
        {
            DbReady = true;
        }


        private void PermissionMessageReceived(PermissionMessage obj)
        {
            if (obj.PermissionCode == PermissionCodes.LOCATION)
            {
                if (!_locationPermissionApproved)
                {
                    Core.Services.LocationService.SetupLocation();
                }
                LocationPermissionApproved = true;
            }
        }

        public bool DbReady
        {
            get { return _dbReady; }
            set
            {
                _dbReady = value;
                TryToStartMainActivity();
            }
        }

        public bool LocationPermissionApproved
        {
            get { return _locationPermissionApproved; }
            set
            {
                _locationPermissionApproved = value;
                TryToStartMainActivity();
            }
        }

        public bool LocationReady
        {
            get { return _locationReady; }
            set
            {
                if (_locationReady) return;
                _locationReady = value;
                TryToStartMainActivity();
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            switch (requestCode)
            {
                case PermissionCodes.LOCATION:
                    {
                        if (grantResults == null || grantResults.Length < 1 || grantResults[0] != Permission.Granted)
                        {
                            var alert = new Android.Support.V7.App.AlertDialog.Builder(this);
                            alert.SetTitle("Permission Required");
                            alert.SetMessage("Location services are required. Please approve the request.");
                            alert.SetPositiveButton("Ok", (sendAlert, args) =>
                            {
                                PermissionService.SetupLocation(this);
                            });
                            alert.SetNegativeButton("Quit", (senderAler, args) =>
                            {
                                FinishAffinity();
                            });
                            var dialog = alert.Create();
                            dialog.Show();
                        }
                        else
                        {
                            LocationPermissionApproved = true;
                        }
                        break;
                    }
            }
        }

        public void OnGlobalLayout()
        {
            //Setup();
            
        }
    }
}

