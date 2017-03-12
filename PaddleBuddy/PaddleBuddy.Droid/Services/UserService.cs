﻿using Android.Content;
using PaddleBuddy.Core.Utilities;

namespace PaddleBuddy.Droid.Services
{
    public class UserService : SharedPreferenceService
    {
        public static bool IsLoggedIn(Context context)
        {
            return GetUserId(context) != int.MinValue && !PBPrefs.TestLoggedOut;
        }

        public static int GetUserId(Context context)
        {
            var id = GetInt(context, PBPrefs.KEY_USER_ID);
            return id;
        }

        public static void SetUserId(Context context, int id)
        {
            PutInt(context, PBPrefs.KEY_USER_ID, id);
        }
    }
}