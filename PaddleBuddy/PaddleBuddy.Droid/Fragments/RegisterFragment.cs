using System;
using System.Threading.Tasks;
using Android.OS;
using Android.Views;
using Android.Widget;
using PaddleBuddy.Core.Services;
using Newtonsoft.Json.Linq;
using PaddleBuddy.Droid.Controls;
using PaddleBuddy.Core.Models;

namespace PaddleBuddy.Droid.Fragments
{
    public class RegisterFragment : BaseFragment
    {
        private View ProgressOverlay { get; set; }
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.fragment_register, container, false);
            ProgressOverlay = view.FindViewById(Resource.Id.register_progressbaroverlay);
            view.FindViewById<Button>(Resource.Id.register_submit_button).Click += OnRegisterClicked;
            return view;
        }

        private void OnRegisterClicked(object sender, EventArgs e)
        {
            ProgressOverlay.Visibility = ViewStates.Visible;
            Task.Run(Register);
        }

		private async Task Register()
		{
		    UserService.GetInstance().ClearUserPrefs();
            var email = View.FindViewById<ClearEditText>(Resource.Id.email).Text;
			if (string.IsNullOrEmpty(email)) email = PaddleBuddy.Core.Utilities.PBUtilities.RandomString(8);
			var password = View.FindViewById<ClearEditText>(Resource.Id.password).Text;
			if (string.IsNullOrEmpty(password)) password = PaddleBuddy.Core.Utilities.PBUtilities.RandomString(8);
            LogService.Log($"EMail: {email}");
            LogService.Log($"Password: {password}");
            var response = await UserService.GetInstance().Register(email, password);
            LogService.Log(response.Success ? "Registered!" : $"Failure details: {response.Detail}");
			if (response.Success)
			{
				Activity.RunOnUiThread(() => Activity.Finish());
			}
			else
			{
				LogService.Log("Register failed");
			}
		}

        public static RegisterFragment NewInstance()
        {
            var fragment = new RegisterFragment();
            return fragment;
        }
    }
}