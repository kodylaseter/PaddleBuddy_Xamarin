using Android.App;
using Android.Content;
using PaddleBuddy.Core.DependencyServices;
using PaddleBuddy.Core.Utilities;

namespace PaddleBuddy.Droid.Services
{
    public class UserServiceAndroid : SharedPreferenceService, IUserService
    {
        public bool IsLoggedIn()
        {
            var id = GetUserId();
            var token = GetJwt();
            return id > 0 && !string.IsNullOrWhiteSpace(token);
        }

        public int GetUserId()
        {
            var id = GetInt(PBPrefs.KEY_USER_ID);
            return id;
        }

        public void SetUserId(int id)
        {
            PutInt(PBPrefs.KEY_USER_ID, id);
        }

        public void ClearUserId()
        {
            SetUserId(int.MinValue);
        }

        public string GetJwt()
        {
            return GetString(PBPrefs.KEY_JWT_TOKEN);
        }

        public void SetJwt(string token)
        {
            PutString(PBPrefs.KEY_JWT_TOKEN, token);
        }

        public void ClearJwt()
        {
            PutString(PBPrefs.KEY_JWT_TOKEN, "");
        }
    }
}
