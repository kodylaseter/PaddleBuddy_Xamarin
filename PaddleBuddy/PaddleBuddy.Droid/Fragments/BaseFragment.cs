using Android.Support.V4.App;
using PaddleBuddy.Droid.Activities;

namespace PaddleBuddy.Droid.Fragments
{
    public class BaseFragment : Fragment
    {
        public void NavigateTo(BaseFragment fragment)
        {
            ((MainActivity)Activity).HandleNavigation(fragment);
        }

    }
}