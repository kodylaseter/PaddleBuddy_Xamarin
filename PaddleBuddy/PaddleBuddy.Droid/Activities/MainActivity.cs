using System;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using PaddleBuddy.Core;
using PaddleBuddy.Core.Services;
using PaddleBuddy.Droid.Adapters;
using PaddleBuddy.Droid.Fragments;
using ActionBarDrawerToggle = Android.Support.V7.App.ActionBarDrawerToggle;
using SearchView = Android.Widget.SearchView;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace PaddleBuddy.Droid.Activities
{
    [Activity(Label = "PaddleBuddy", Theme = "@style/AppTheme")]
    public class MainActivity : AppCompatActivity, NavigationView.IOnNavigationItemSelectedListener, MenuItemCompat.IOnActionExpandListener
    {
        private Toolbar _toolbar;
        private DrawerLayout _drawer;
        private NavigationView _navigationView;
        private RecyclerView SearchRecyclerView { get; set; }
        private ActionBarDrawerToggle _toggle;
        private LinearLayout _searchLayout;
        private SearchView _searchView;
        private IMenuItem _searchItem;
        private int DEFAULT_FRAGMENT_ID;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            DEFAULT_FRAGMENT_ID = Resource.Id.nav_map;
            LogService.Log("main activity started");
            SetContentView(Resource.Layout.activity_main);

            _toolbar = (Toolbar) FindViewById(Resource.Id.toolbar);
            SetSupportActionBar(_toolbar);

            _drawer = (DrawerLayout) FindViewById(Resource.Id.drawer_layout);
            _toggle = new ActionBarDrawerToggle(this, _drawer, _toolbar, Resource.String.navigation_drawer_open,
                Resource.String.navigation_drawer_close);
            _drawer.AddDrawerListener(_toggle);
            _toggle.SyncState();

            _navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            _navigationView.SetNavigationItemSelectedListener(this);
            //TestTripSummary();
            //TestTripHistory();

            OnNavigationItemSelected();
            SearchRecyclerView = FindViewById<RecyclerView>(Resource.Id.search_recyclerview);
            _searchLayout = FindViewById<LinearLayout>(Resource.Id.search_results_layout);

            Window.SetSoftInputMode(SoftInput.AdjustNothing);
        }

        //private void SetSearchBarHeight()
        //{
        //    var resourceId = Resources.GetIdentifier("status_bar_height", "dimen", "android");
        //    var typedValue = new TypedValue();
        //    if (resourceId > 0 && Theme.ResolveAttribute(Android.Resource.Attribute.ActionBarSize, typedValue, true))
        //    {
        //        var height = Resources.GetDimensionPixelSize(resourceId) + TypedValue.ComplexToDimensionPixelSize(typedValue.Data, Resources.DisplayMetrics);
        //        FindViewById(Resource.Id.searchbar).SetMinimumHeight(height);
        //    }
        //    else
        //    {
        //        throw new Exception("unable to set searchbar height");
        //    }
        //}

        public override bool OnCreateOptionsMenu(IMenu menu)
        {

            var mainActivitySearchAdapter = new MainActivitySearchAdapter(this);
            SearchRecyclerView.SetAdapter(mainActivitySearchAdapter);
            SearchRecyclerView.SetLayoutManager(new LinearLayoutManager(Application.Context));
            _searchLayout.Clickable = true;
            _searchLayout.Click += (s, e) => { CloseSearch(true); };

            MenuInflater.Inflate(Resource.Menu.main_menu, menu);
            _searchItem = menu.FindItem(Resource.Id.action_search);
            _searchView = (SearchView) _searchItem.ActionView;
            MenuItemCompat.SetOnActionExpandListener(_searchItem, this);
            _searchView.QueryTextChange += (sender, args) =>
            {
                mainActivitySearchAdapter.Filter.InvokeFilter(args.NewText);
                args.Handled = true;
            };
            _searchView.QueryTextSubmit += (sender, args) =>
            {
                LogService.Log("query text submitted: " + args.Query);
                args.Handled = true;
            };
            _searchView.Focusable = true;
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_search)
            {
                OpenSearch();
            }
            return base.OnOptionsItemSelected(item);
        }

        public bool OnNavigationItemSelected(IMenuItem menuItem = null)
        {
            HandleNavigation(menuItem);
            
            _drawer.CloseDrawer(GravityCompat.Start);
            return true;
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
                case Resource.Id.nav_history:
                    id = (int)NavDraweritems.History;
                    fragment = TripHistoryFragment.NewInstance();
                    break;
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
            bundle.PutString(SysPrefs.SERIALIZABLE_TRIPSUMMARY, json);
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
            }
            catch (Exception e)
            {
                LogService.ExceptionLog(e.Message);
            }
        }

        private void TestTripSummary()
        {
            var fragment = TripSummaryFragment.NewInstance();
            DatabaseService.GetInstance().SeedTripSummary();
            HandleNavigationWithData(fragment, SysPrefs.SERIALIZABLE_TRIPSUMMARY, JsonConvert.SerializeObject(DatabaseService.GetInstance().TripSummaries.FirstOrDefault()));
        }

        private void TestTripHistory()
        {
            DatabaseService.GetInstance().SeedTripSummary();
            DEFAULT_FRAGMENT_ID = Resource.Id.nav_history;
        }

        private void OpenSearch()
        {
            _searchLayout.Visibility = ViewStates.Visible;
            _searchView.Iconified = false;
        }

        private void CloseSearch(bool shouldCollapse)
        {
            _searchLayout.Visibility = ViewStates.Gone;
            _searchView.Iconified = true;
            if (shouldCollapse)
            {
                _searchItem.CollapseActionView();
            }
        }

        //todo: figure out why this doesnt call when the search view is open
        public override void OnBackPressed()
        {
            if (_drawer.IsDrawerOpen(GravityCompat.Start))
            {
                _drawer.CloseDrawer(GravityCompat.Start);
            }
            else if (_searchItem.IsActionViewExpanded)
            {
                CloseSearch(true);
            }
            else
            {
                base.OnBackPressed();
            }
        }

        public bool OnMenuItemActionCollapse(IMenuItem item)
        {
            var id = item.ItemId;
            if (id == Resource.Id.action_search)
            {
                CloseSearch(false);
            }
            return true;
        }

        public bool OnMenuItemActionExpand(IMenuItem item)
        {
            return true;
        }

        private enum NavDraweritems
        {
            Map = 0,
            History = 1
        }
    }
}