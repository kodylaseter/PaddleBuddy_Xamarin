using Android.OS;
using Java.Lang;

namespace PaddleBuddy.Droid.Utilities
{
    public class AndroidPBUtilities
    {
        public static bool OnMainThread => Looper.MyLooper() == Looper.MainLooper;
    }
}