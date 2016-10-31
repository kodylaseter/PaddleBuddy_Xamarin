using Android;
using Android.App;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using PaddleBuddy.Core.Models;
using PaddleBuddy.Core.Models.Messages;
using PaddleBuddy.Core.Services;

namespace PaddleBuddy.Droid.Services
{
    public class PermissionService : BaseAndroidService
    {
        private static readonly string[] PermissionsLocation =
                {
                  Manifest.Permission.AccessCoarseLocation,
                  Manifest.Permission.AccessFineLocation
                };

        private const string Permission = Manifest.Permission.AccessFineLocation;

        public static void RequestLocation(AppCompatActivity activity)
        {
            activity.RequestPermissions(PermissionsLocation, PermissionCodes.LOCATION);
        }

        public static bool CheckLocation()
        {
            var approved = ContextCompat.CheckSelfPermission(Application.Context, Permission) == Android.Content.PM.Permission.Granted;
            if (approved)
            {
                MessengerService.Messenger.Send(new PermissionMessage {PermissionCode = PermissionCodes.LOCATION});
            }
            return approved;
        }

        public static void SetupLocation(AppCompatActivity activity)
        {
            if (!CheckLocation())
            {
                RequestLocation(activity);
            }
            else
            {
                LogService.Log("Location permission already given!");
            }
        }
    }
}