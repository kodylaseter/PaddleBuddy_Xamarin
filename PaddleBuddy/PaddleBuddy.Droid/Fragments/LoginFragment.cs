using System;
using System.Threading.Tasks;
using Android.OS;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json.Linq;
using PaddleBuddy.Core.Models;
using PaddleBuddy.Core.Services;
using PaddleBuddy.Droid.Activities;
using PaddleBuddy.Droid.Controls;

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
            UpdateProgressBarOverlayVisibilityTo(true);
            Task.Run(Login);
        }


        private async Task Login()
        {
			UserService.GetInstance().ClearUserPrefs();
            var user = new User
            {
                Email = View.FindViewById<ClearEditText>(Resource.Id.email).Text,
                Password = View.FindViewById<ClearEditText>(Resource.Id.password).Text
            };
            var error = user.Validate();
            if (!string.IsNullOrEmpty(error)) {
                HandleError(error);
                UpdateProgressBarOverlayVisibilityTo(false);
                return;
            }

            var response = await UserService.GetInstance().Login(user);

			LogService.Log(response.Success ? "Success!" : response.Detail);
            if (response.Success)
            {
                var responseUser = ((JObject) response.Data).ToObject<User>();
                UserService.GetInstance().SetUserPrefs(responseUser);
                Activity.RunOnUiThread(() => Activity.Finish());
            }
            else
            {
                LogService.Log("Login failed");
                UpdateProgressBarOverlayVisibilityTo(false);
            }
        }

        private void UpdateProgressBarOverlayVisibilityTo(bool isVisible)
        {
            Activity.RunOnUiThread(() => ProgressOverlay.Visibility = isVisible ? ViewStates.Visible : ViewStates.Gone);
        }

        private void HandleError(string error)
        {
            LogService.Log(error);
        }

        public static LoginFragment NewInstance()
        {
            var fragment = new LoginFragment();
            return fragment;
        }
    }
}