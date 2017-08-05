using System;
using System.Linq;
using Android.App;
using Android.InputMethodServices;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Views;
using Newtonsoft.Json;
using PaddleBuddy.Core.Services;
using PaddleBuddy.Core.Utilities;
using PaddleBuddy.Droid.Fragments;
using PaddleBuddy.Droid.Utilities;
using ActionBarDrawerToggle = Android.Support.V7.App.ActionBarDrawerToggle;
using Fragment = Android.Support.V4.App.Fragment;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace PaddleBuddy.Droid.Activities
{
    [Activity(Label = "PaddleBuddy", Theme = "@style/AppTheme")]
    public class MainActivity : AppCompatActivity, NavigationView.IOnNavigationItemSelectedListener
    {
        private Toolbar Toolbar { get; set; }
        private DrawerLayout _drawer;
        private NavigationView _navigationView;
        private ActionBarDrawerToggle _toggle;

        private int DEFAULT_FRAGMENT_ID;

        #region init

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            DEFAULT_FRAGMENT_ID = Resource.Id.nav_map;
            LogService.Log("main activity started");
            SetContentView(Resource.Layout.activity_main);
            Window.SetSoftInputMode(SoftInput.AdjustResize);

            Toolbar = (Toolbar) FindViewById(Resource.Id.toolbar);
            SetSupportActionBar(Toolbar);

            _drawer = (DrawerLayout) FindViewById(Resource.Id.drawer_layout);
            _toggle = new ActionBarDrawerToggle(this, _drawer, Toolbar, Resource.String.navigation_drawer_open,
                Resource.String.navigation_drawer_close);
            _drawer.AddDrawerListener(_toggle);
            _toggle.SyncState();


            _navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            _navigationView.SetNavigationItemSelectedListener(this);
            //TestTripSummary();
            //TestTripHistory();

            OnNavigationItemSelected();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            if (GetCurrentContentFragment.IsTypeOf(typeof (MapFragment)))
            {
                MenuInflater.Inflate(Resource.Menu.main_menu, menu);
            }
            return base.OnCreateOptionsMenu(menu);
        }
        #endregion

        

        #region navigation
        public bool OnNavigationItemSelected(IMenuItem menuItem = null)
        {
            HandleNavigation(menuItem);

            _drawer.CloseDrawer(GravityCompat.Start);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_search)
            {
                if (GetCurrentContentFragment.IsTypeOf(typeof(MapFragment)))
                {
                    ((MapFragment)GetCurrentContentFragment).ToggleSearch();
                }
            }
            return base.OnOptionsItemSelected(item);
        }

        //todo: figure out why this doesnt call when the search view is open
        public override void OnBackPressed()
        {
            if (_drawer.IsDrawerOpen(GravityCompat.Start))
            {
                _drawer.CloseDrawer(GravityCompat.Start);
            }
            else if (GetCurrentContentFragment.IsTypeOf(typeof (MapFragment)))
            {
                ((MapFragment) GetCurrentContentFragment).CloseSearch();
            }
            else
            {
                base.OnBackPressed();
            }
        }

        public void HandleNavigation(IMenuItem menuItem)
        {
            var id = menuItem?.ItemId ?? DEFAULT_FRAGMENT_ID;
            //converts id to a drawerindex
            BaseFragment fragment;
            switch (id)
            {
                case Resource.Id.nav_map:
                    id = (int)NavDraweritems.Map;
                    fragment = MapFragment.NewInstance();
                    break;
                case Resource.Id.nav_plan:
                    id = (int)NavDraweritems.Plan;
                    fragment = PlanFragment.NewInstance();
                    break;
                case Resource.Id.nav_history:
                    id = (int)NavDraweritems.History;
                    fragment = TripHistoryFragment.NewInstance();
                    break;
                case Resource.Id.nav_settings:
                    id = (int) NavDraweritems.Settings;
                    fragment = MapFragment.NewInstance(); //TODO: fix this
                    break;
                case Resource.Id.nav_logout:
                    Core.Services.UserService.GetInstance().ClearUserPrefs();
                    Finish();
                    return;
                default:
                    id = (int)NavDraweritems.Map;
                    fragment = MapFragment.NewInstance();
                    break;
            }
            if (fragment != null)
            {
                _navigationView.Menu.GetItem(id).SetChecked(true);
                HandleNavigation(fragment);
            }
        }

        public void HandleNavigationWithData(BaseFragment fragment, string key, string json)
        {
            if (fragment == null || string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(json)) return;
            for (int i = 0; i < _navigationView.Menu.Size(); i++)
            {
                _navigationView.Menu.GetItem(i).SetChecked(false);
            }
            var bundle = new Bundle();
            bundle.PutString(PBPrefs.SERIALIZABLE_TRIPSUMMARY, json);
            fragment.Arguments = bundle;
            HandleNavigation(fragment);
        }

        public void HandleNavigation(BaseFragment fragment)
        {
            if (fragment == null) return;
            LogService.TagAndLog("NAV", $"Navigating to {fragment.GetType()}");
            try
            {
                SupportFragmentManager.BeginTransaction().Replace(Resource.Id.content_f, fragment).Commit();
                SupportFragmentManager.ExecutePendingTransactions();
                InvalidateOptionsMenu();
            }
            catch (Exception e)
            {
                LogService.ExceptionLog(e.Message);
            }
        }

        public Fragment GetCurrentContentFragment
        {
            get { return SupportFragmentManager.FindFragmentById(Resource.Id.content_f); }
        }
        #endregion

        #region tests
        private void TestTripSummary()
        {
            var fragment = TripSummaryFragment.NewInstance();
            DatabaseService.GetInstance().SeedTripSummary();
            HandleNavigationWithData(fragment, PBPrefs.SERIALIZABLE_TRIPSUMMARY, JsonConvert.SerializeObject(DatabaseService.GetInstance().TripSummaries.FirstOrDefault()));
        }

        private void TestTripHistory()
        {
            DatabaseService.GetInstance().SeedTripSummary();
            DEFAULT_FRAGMENT_ID = Resource.Id.nav_history;
        }
        #endregion

        private enum NavDraweritems
        {
            Map = 0,
            History = 1,
            Plan = 2,
            Settings = 3,
            LogOut = 4
        }
    }
}