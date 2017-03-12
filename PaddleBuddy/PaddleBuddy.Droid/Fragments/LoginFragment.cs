using System;
using System.Threading.Tasks;
using Android.OS;
using Android.Views;
using Android.Widget;
using PaddleBuddy.Droid.Activities;
using PaddleBuddy.Droid.Services;

namespace PaddleBuddy.Droid.Fragments
{
    public class LoginFragment : BaseFragment
    {
        private View ProgressOverlay { get; set; }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.fragment_login, container, false);
            ProgressOverlay = view.FindViewById(Resource.Id.login_progressbaroverlay);
            view.FindViewById<Button>(Resource.Id.register_button).Click += OnRegisterClicked;
            view.FindViewById<Button>(Resource.Id.login_button).Click += OnLoginClicked;
            return view;
        }

        private void OnRegisterClicked(object sender, EventArgs e)
        {
            ((LoginRegisterActivity)Activity).ShowRegisterFragment();
        }

        private void OnLoginClicked(object sender, EventArgs e)
        {
            ProgressOverlay.Visibility = ViewStates.Visible;
            Task.Run(Login);
        }


        private async Task Login()
        {
            await Task.Delay(1000);
            UserService.SetUserId(Context, 1);
            Activity.RunOnUiThread(() => Activity.Finish());
        }

        public static LoginFragment NewInstance()
        {
            var fragment = new LoginFragment();
            return fragment;
        }
    }
}