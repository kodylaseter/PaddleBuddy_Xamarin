using Android.App;
using Android.Content;
using Android.Preferences;

namespace PaddleBuddy.Droid.Services
{
    public class SharedPreferenceService
    {
        private static ISharedPreferences SharedPreferences => PreferenceManager.GetDefaultSharedPreferences(Application
            .Context);

        protected static void PutInt(string key, int i)
        {
            SharedPreferences.Edit().PutInt(key, i).Apply();
        }

        protected static void PutString(string key, string str)
        {
            SharedPreferences.Edit().PutString(key, str).Apply();
        }

        protected static int GetInt(string key)
        {
            return SharedPreferences.GetInt(key, int.MinValue);
        }

        protected static string GetString(string key)
        {
            return SharedPreferences.GetString(key, "");
        }
    }
}