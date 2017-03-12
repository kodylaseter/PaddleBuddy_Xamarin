using Android.Content;
using Android.Preferences;

namespace PaddleBuddy.Droid.Services
{
    public class SharedPreferenceService
    {
        private static ISharedPreferences GetSharedPreferences(Context context)
        {
            return PreferenceManager.GetDefaultSharedPreferences(context);
        }

        protected static void PutInt(Context context, string key, int i)
        {
            GetSharedPreferences(context).Edit().PutInt(key, i).Apply();
        }

        protected static void PutString(Context context, string key, string str)
        {
            GetSharedPreferences(context).Edit().PutString(key, str).Apply();
        }

        protected static int GetInt(Context context, string key)
        {
            return GetSharedPreferences(context).GetInt(key, int.MinValue);
        }

        protected static string GetString(Context context, string key)
        {
            return GetSharedPreferences(context).GetString(key, "");
        }
    }
}