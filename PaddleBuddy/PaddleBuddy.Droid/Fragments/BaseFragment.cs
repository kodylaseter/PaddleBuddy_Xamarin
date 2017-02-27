using Android.Support.V4.App;
using Newtonsoft.Json;
using PaddleBuddy.Droid.Activities;

namespace PaddleBuddy.Droid.Fragments
{
    public class BaseFragment : Fragment
    {
        public void NavigateTo(BaseFragment fragment, string key = null, object obj = null)
        {
            if (string.IsNullOrWhiteSpace(key) || obj == null)
            {
                ((MainActivity) Activity).HandleNavigation(fragment);
            }
            else
            {
                ((MainActivity) Activity).HandleNavigationWithData(fragment, key, JsonConvert.SerializeObject(obj));
            }
        }

    }
}