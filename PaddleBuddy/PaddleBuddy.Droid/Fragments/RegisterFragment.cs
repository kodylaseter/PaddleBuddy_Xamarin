using System;
using System.Threading.Tasks;
using Android.OS;
using Android.Views;
using Android.Widget;
using PaddleBuddy.Core.Services;
using PaddleBuddy.Droid.Services;
using PaddleBuddy.Droid.Controls;

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
			var response = await UserService.GetInstance().Register(
				View.FindViewById<ClearEditText>(Resource.Id.email).Text,
				View.FindViewById<ClearEditText>(Resource.Id.password).Text);
		    ProgressOverlay.Visibility = ViewStates.Gone;
			LogService.Log(response.Success ? "Registered!" : response.Detail);
		}

        public static RegisterFragment NewInstance()
        {
            var fragment = new RegisterFragment();
            return fragment;
        }
    }
}