using System;
using Android.App;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using PaddleBuddy.Core.Services;
using PaddleBuddy.Droid.Fragments;
using ActionBarDrawerToggle = Android.Support.V7.App.ActionBarDrawerToggle;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace PaddleBuddy.Droid.Activities
{
    [Activity(Label = "PaddleBuddy", Theme = "@style/AppTheme")]
    public class MainActivity : AppCompatActivity, NavigationView.IOnNavigationItemSelectedListener
    {
        private Toolbar _toolbar;
        private DrawerLayout _drawer;
        private NavigationView _navigationView;
        private ListView _searchListView;
        private ArrayAdapter<string> _searchArrayAdapter;
        private ActionBarDrawerToggle _toggle;
        private LinearLayout _searchLayout;
        private SearchView _searchView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            LogService.Log("main activity started");
            SetContentView(Resource.Layout.activity_main);

            _toolbar = (Toolbar) FindViewById(Resource.Id.toolbar);
            SetSupportActionBar(_toolbar);

            _drawer = (DrawerLayout) FindViewById(Resource.Id.drawer_layout);
            _toggle = new ActionBarDrawerToggle(this, _drawer, _toolbar, Resource.String.navigation_drawer_open,
                Resource.String.navigation_drawer_close);
            _drawer.SetDrawerListener(_toggle);
            _toggle.SyncState();

            _navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            _navigationView.SetNavigationItemSelectedListener(this);
            OnNavigationItemSelected();

            _searchListView = FindViewById<ListView>(Resource.Id.search_list_view);
            _searchLayout = FindViewById<LinearLayout>(Resource.Id.search_results_layout);
            _searchLayout.Clickable = true;
            _searchLayout.Click += (s,e) => { SearchClosed(); };
            var _searchItems = new[] {"test1", "abc", "def", "testttt", "gerogia", "blah"};
            _searchArrayAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, _searchItems);
            _searchListView.Adapter = _searchArrayAdapter;
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
            MenuInflater.Inflate(Resource.Menu.main_menu, menu);
            var searchItem = menu.FindItem(Resource.Id.action_search);
            _searchView = (SearchView) searchItem.ActionView;

            _searchView.QueryTextChange += (sender, args) =>
            {
                LogService.Log("typing: " + args.NewText);
                _searchArrayAdapter.Filter.InvokeFilter(args.NewText);
            };
            _searchView.QueryTextSubmit += (sender, args) =>
            {
                LogService.Log("query text submitted: " + args.Query);
                args.Handled = true;
            };
            _searchView.Focusable = true;
            _searchView.Iconified = false;
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_search)
            {
                _searchView.RequestFocusFromTouch();
                SearchOpened();
            }
            return base.OnOptionsItemSelected(item);
        }

        public bool OnNavigationItemSelected(IMenuItem menuItem = null)
        {
            HandleNavigation(menuItem);
            
            _drawer.CloseDrawer(GravityCompat.Start);
            return true;
        }

        public void HandleNavigation(IMenuItem menuItem = null, BaseFragment fragment = null)
        {
            var id = menuItem?.ItemId ?? Resource.Id.nav_map;
            //converts id to a drawerindex
            switch (id)
            {
                case Resource.Id.nav_map:
                    id = (int)NavDraweritems.Map;
                    fragment = fragment ?? MapFragment.NewInstance();
                    break;
                case Resource.Id.nav_plan:
                    id = (int)NavDraweritems.Plan;
                    fragment = fragment ?? PlanFragment.NewInstance();
                    break;
                default:
                    id = (int)NavDraweritems.Map;
                    fragment = fragment ?? MapFragment.NewInstance();
                    break;
            }
            try
            {
                if (fragment != null)
                {
                    SupportFragmentManager.BeginTransaction().Replace(Resource.Id.content_f, fragment).Commit();
                }
                _navigationView.Menu.GetItem(id).SetChecked(true);
            }
            catch (Exception e)
            {
                LogService.Log(e);
            }
        }

        private void SearchOpened()
        {
            _searchLayout.Visibility = ViewStates.Visible;
        }

        private void SearchClosed()
        {
            _searchLayout.Visibility = ViewStates.Gone;
             _searchView.Iconified = true;
        }

        public override void OnBackPressed()
        {
            if (_drawer.IsDrawerOpen(GravityCompat.Start))
            {
                _drawer.CloseDrawer(GravityCompat.Start);
            } else if (!_searchView.Iconified)
            {
                _searchView.Iconified = true;
            }
            else
            {
                base.OnBackPressed();
            }
        }

        private enum NavDraweritems
        {
            Map = 0,
            Plan = 1
        }
    }
}