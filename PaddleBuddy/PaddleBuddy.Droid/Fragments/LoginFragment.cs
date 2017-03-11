using Android.OS;
using Android.Views;
using PaddleBuddy.Droid.Activities;

namespace PaddleBuddy.Droid.Fragments
{
    public class LoginFragment : BaseFragment
    {

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.fragment_login, container, false);
            view.FindViewById(Resource.Id.register_button).Click += (s, e) =>
            {
                ((LoginRegisterActivity) Activity).ShowRegisterFragment();
            };
            return view;
        }

        public static LoginFragment NewInstance()
        {
            var fragment = new LoginFragment();
            return fragment;
        }
    }
}