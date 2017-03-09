using System.Threading.Tasks;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using PaddleBuddy.Core;
using PaddleBuddy.Core.Models.Messages;
using PaddleBuddy.Core.Services;
using PaddleBuddy.Droid.Services;

namespace PaddleBuddy.Droid.Activities
{
    [Activity(Label = "PaddleBuddy", MainLauncher = true, Icon = "@drawable/icon", Theme = "@style/AppTheme")]
    public class SetupActivity : AppCompatActivity
    {
        private bool _mainActivityStarted;
        private bool _dbReady;
        private bool _locationPermissionApproved;
        private bool _locationReady;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            //hide keyboard
            Window.SetSoftInputMode(SoftInput.StateAlwaysHidden);
            LogService.Log("setup activity started");
            _mainActivityStarted = false;
            _dbReady = false;
            _locationPermissionApproved = false;
            _locationReady = false;
            SetupSysPrefs(testOffline: true);
            SetContentView(Resource.Layout.activity_setup);
        }

        protected override void OnResume()
        {
            base.OnResume();
            Setup();
        }

        private void SetupSysPrefs(bool testOffline = false)
        {
            SysPrefs.Device = SysPrefs.Devices.Android;
            SysPrefs.TestOffline = testOffline;
            SysPrefs.DisableMap = testOffline;
            SysPrefs.Simulate = testOffline;
            if (testOffline)
            {
                DatabaseService.GetInstance().SeedData();
            }
        }

        private void Setup()
        {
            //todo: consider making these async again
            LogService.Log(SysPrefs.TestOffline ? "Testing offline" : "Testing online");
            if (!SysPrefs.TestOffline)
            {
                MessengerService.Messenger.Register<DbReadyMessage>(this, DbReadyReceived);
                MessengerService.Messenger.Register<PermissionMessage>(this, PermissionMessageReceived);
                MessengerService.Messenger.Register<LocationUpdatedMessage>(this, LocationUpdatedReceived);
                Task.Run(() => Services.StorageService.GetInstance().Setup());
                PermissionService.SetupLocation(this);
            }
            else
            {
                StartActivity(typeof(MainActivity));
                //StartActivity(typeof(TestActivity));
            }
        }

        private void TryToStartMainActivity()
        {
            LogService.Log(
                $"trying to start main activity... _mainactivitystarted = {_mainActivityStarted} _dbready = {_dbReady} _locationpermissionapproved = {_locationPermissionApproved} _locationready = {_locationReady}");
            if (!_mainActivityStarted & _dbReady & _locationPermissionApproved)
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
            LogService.Log("locationupdate message received");
            if (!_locationReady)
            {
                LocationReady = true;
            }
        }

        private void DbReadyReceived(DbReadyMessage obj)
        {
            LogService.Log("dbready message received");
            if (!_dbReady)
            {
                DbReady = true;
            }
        }


        private void PermissionMessageReceived(PermissionMessage obj)
        {
            LogService.Log("permission message received");
            if (obj.PermissionCode == SysPrefs.PERMISSION_LOCATION && !_locationPermissionApproved)
            {
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
                Core.Services.LocationService.GetInstance().SetupLocation();
                TryToStartMainActivity();
            }
        }

        public bool LocationReady
        {
            get { return _locationReady; }
            set
            {
                _locationReady = value;
                TryToStartMainActivity();
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        { 
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            switch (requestCode)
            {
                case SysPrefs.PERMISSION_LOCATION:
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

