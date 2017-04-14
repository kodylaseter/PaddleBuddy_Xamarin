using System.Threading.Tasks;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using PaddleBuddy.Core.Models.Messages;
using PaddleBuddy.Core.Services;
using PaddleBuddy.Core.Utilities;
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

        #region init
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.activity_setup);
            LogService.Log("setup activity started");
        }

        protected override void OnResume()
        {
            base.OnResume();//hide keyboard
            Window.SetSoftInputMode(SoftInput.StateAlwaysHidden);
            _mainActivityStarted = false;
            _dbReady = false;
            _locationPermissionApproved = false;
            _locationReady = false;
            SetupSysPrefs();
            if (!Services.UserService.IsLoggedIn(Application.Context))
            {
                StartActivity(typeof(LoginRegisterActivity));
            }
            else
            {
                Task.Run(Setup);
            }
        }

        private void SetupSysPrefs()
        {
            PBPrefs.Device = PBPrefs.Devices.Android;
            if (PBPrefs.TestOffline)
            {
                DatabaseService.GetInstance().SeedData();
            }
        }
        #endregion

        #region setup for mainactivity
        private async Task Setup()
        {
            //todo: consider making these async again
            LogService.Log(PBPrefs.TestOffline ? "Testing offline" : "Testing online");
            if (!PBPrefs.TestOffline)
            {
                MessengerService.Messenger.Register<DbReadyMessage>(this, DbReadyReceived);
                MessengerService.Messenger.Register<PermissionMessage>(this, PermissionMessageReceived);
                MessengerService.Messenger.Register<LocationUpdatedMessage>(this, LocationUpdatedReceived);
                Services.StorageService.GetInstance().Setup();
                PermissionService.SetupLocation(this);
            }
            else
            {
                RunOnUiThread(() =>
                {
                    StartActivity(typeof(MainActivity));
                    //StartActivity(typeof(TestActivity));
                });
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
                RunOnUiThread(() =>
                {
                    StartActivity(typeof(MainActivity));
                });
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
            if (obj.PermissionCode == PBPrefs.PERMISSION_LOCATION && !_locationPermissionApproved)
            {
                LocationPermissionApproved = true;
            }
        }

        private bool DbReady
        {
            get { return _dbReady; }
            set
            {
                _dbReady = value;
                TryToStartMainActivity();
            }
        }

        private bool LocationPermissionApproved
        {
            get { return _locationPermissionApproved; }
            set
            {
                _locationPermissionApproved = value;
                Core.Services.LocationService.GetInstance().SetupLocation();
                TryToStartMainActivity();
            }
        }

        private bool LocationReady
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
                case PBPrefs.PERMISSION_LOCATION:
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
        #endregion
    }
}

