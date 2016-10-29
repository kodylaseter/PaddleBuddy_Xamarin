using System;
using Android.App;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using PaddleBuddy.Core.Services;
using PaddleBuddy.Droid.Fragments;
using ActionBarDrawerToggle = Android.Support.V7.App.ActionBarDrawerToggle;

namespace PaddleBuddy.Droid.Activities
{
    [Activity(Label = "MainActivity", Theme = "@style/AppTheme.NoActionBar")]
    public class MainActivity : AppCompatActivity, NavigationView.IOnNavigationItemSelectedListener
    {
        private Toolbar _toolbar;
        private DrawerLayout _drawer;
        private NavigationView _navigationView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            _toolbar = (Toolbar) FindViewById(Resource.Id.toolbar);
            SetSupportActionBar(_toolbar);

            _drawer = (DrawerLayout) FindViewById(Resource.Id.drawer_layout);
            var toggle = new ActionBarDrawerToggle(this, _drawer, _toolbar, Resource.String.navigation_drawer_open,
                Resource.String.navigation_drawer_close);
            _drawer.SetDrawerListener(toggle);
            toggle.SyncState();

            _navigationView = (NavigationView) FindViewById(Resource.Id.nav_view);
            _navigationView.SetNavigationItemSelectedListener(this);
            OnNavigationItemSelected();
        }

        public bool OnNavigationItemSelected(IMenuItem menuItem = null)
        {
            int id;
            if (menuItem == null)
            {
                id = Resource.Id.nav_map;
                _navigationView.Menu.GetItem(0).SetChecked(true);
            }
            else
            {
                id = menuItem.ItemId;
            }
            try
            {
                BaseFragment fragment;
                switch (id)
                {
                    case Resource.Id.nav_map:
                        fragment = MapFragment.NewInstance();
                        break;
                    case Resource.Id.nav_plan:
                        fragment = PlanFragment.NewInstance();
                        break;
                    default:
                        fragment = MapFragment.NewInstance();
                        break;
                }
                if (fragment != null)
                {
                    SupportFragmentManager.BeginTransaction().Replace(Resource.Id.content_f, fragment).Commit();
                }

            }
            catch (Exception e)
            {
                LogService.Log(e);
            }
            _drawer.CloseDrawer(GravityCompat.Start);
            return true;
        }

        public override void OnBackPressed()
        {
            if (_drawer.IsDrawerOpen(GravityCompat.Start))
            {
                _drawer.CloseDrawer(GravityCompat.Start);
            }
            else
            {
                base.OnBackPressed();
            }
        }
    }
}