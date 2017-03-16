using Android.Support.V4.App;
using Android.Views.InputMethods;
using Newtonsoft.Json;
using PaddleBuddy.Droid.Activities;

namespace PaddleBuddy.Droid.Fragments
{
    public class BaseFragment : Fragment
    {

        protected InputMethodManager InputMethodManager => (InputMethodManager)Activity.GetSystemService(Android.Content.Context.InputMethodService);

        protected void HideKeyboard()
        {
            var view = Activity.CurrentFocus;
            if (view != null)
            {
                InputMethodManager.HideSoftInputFromWindow(view.WindowToken,
                    HideSoftInputFlags.None);
            }
        }

        protected void NavigateTo(BaseFragment fragment, string key = null, object obj = null)
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