using System;
using Android.App;
using Android.OS;
using Android.Support.V7.App;
using PaddleBuddy.Core.Services;
using PaddleBuddy.Droid.Fragments;

namespace PaddleBuddy.Droid.Activities
{
    [Activity(Label = "PaddleBuddy", Theme = "@style/AppTheme")]
    public class LoginRegisterActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.activity_loginregister);
            try
            {
                ShowLoginFragment();
            }
            catch (Exception e)
            {
                LogService.ExceptionLog(e.Message);
            }
        }

        public void ShowRegisterFragment()
        {
            NavigateToFragment(RegisterFragment.NewInstance());
        }

        public void ShowLoginFragment()
        {
            NavigateToFragment(LoginFragment.NewInstance());
        }

        private void NavigateToFragment(BaseFragment fragment)
        {
            SupportFragmentManager.BeginTransaction().Replace(Resource.Id.content, fragment).Commit();
        }
    }
}

