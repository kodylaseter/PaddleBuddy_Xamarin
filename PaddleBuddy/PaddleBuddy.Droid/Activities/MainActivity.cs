using System;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Text;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Newtonsoft.Json;
using PaddleBuddy.Core;
using PaddleBuddy.Core.Services;
using PaddleBuddy.Droid.Adapters;
using PaddleBuddy.Droid.Fragments;
using PaddleBuddy.Droid.Utilities;
using ActionBarDrawerToggle = Android.Support.V7.App.ActionBarDrawerToggle;
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
        private ListView SearchListView { get; set; }
        private LinearLayout SearchLayout { get; set; }
        private EditText SearchEditText { get; set; }
        private MainActivitySearchAdapter SearchAdapter { get; set; }
        private ImageButton SearchCloseImageButton { get; set; }
        private int DEFAULT_FRAGMENT_ID;
        private InputMethodManager InputMethodManager => (InputMethodManager) GetSystemService(InputMethodService);

        #region init
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            DEFAULT_FRAGMENT_ID = Resource.Id.nav_map;
            LogService.Log("main activity started");
            SetContentView(Resource.Layout.activity_main);

            Toolbar = (Toolbar) FindViewById(Resource.Id.toolbar);
            SetSupportActionBar(Toolbar);
            SupportActionBar.Title = null;

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
            SearchListView = FindViewById<ListView>(Resource.Id.search_listview);
            SearchLayout = FindViewById<LinearLayout>(Resource.Id.search_layout);
            //SearchEditText = FindViewById<EditText>(Resource.Id.search_edittext_1);
            //SearchEditText.TextChanged += SearchEditTextOnTextChanged;
            SearchAdapter = new MainActivitySearchAdapter(this);
            SearchListView.Adapter = SearchAdapter;
            SearchLayout.Clickable = true;
            SearchLayout.Click += (s, e) => { CloseSearch(); };
            SearchListView.ItemClick += OnSearchItemSelected;
            Window.SetSoftInputMode(SoftInput.AdjustResize);

        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.main_menu, menu);
            return base.OnCreateOptionsMenu(menu);
        }
        #endregion

        #region search
        private void SearchEditTextOnTextChanged(object sender, TextChangedEventArgs args)
        {
            var query = args.Text.ToString();
            SearchAdapter.Filter.InvokeFilter(args.Text.ToString());
            var isNotNull = !string.IsNullOrWhiteSpace(query);
            UpdateSearchCloseOrClear(isNotNull);
        }

        private void SearchCloseImageButtonClicked(object sender, EventArgs e)
        {
            if (!SearchLayout.IsVisible()) return;
            if (string.IsNullOrWhiteSpace(SearchEditText.Text))
            {
                CloseSearch();
            }
            else
            {
                SearchEditText.Text = "";
            }
        }

        private void ToggleSearch()
        {
            if (SearchLayout.Visibility == ViewStates.Gone)
            {
                OpenSearch();
            }
            else
            {
                CloseSearch();
            }
        }

        private void OpenSearch()
        {
            SearchLayout.Visibility = ViewStates.Visible;
            //SearchEditText.RequestFocus();
            InputMethodManager.ShowSoftInput(SearchEditText, ShowFlags.Implicit);
            Toolbar.Visibility = ViewStates.Gone;
            UpdateSearchCloseOrClear(false);
        }

        private void CloseSearch()
        {
            SearchLayout.Visibility = ViewStates.Gone;
            Toolbar.Visibility = ViewStates.Visible;
            var view = CurrentFocus;
            if (view != null)
            {
                InputMethodManager.HideSoftInputFromWindow(view.WindowToken,
                    HideSoftInputFlags.None);
            }
        }

        private void UpdateSearchCloseOrClear(bool setToClear)
        {
            SearchCloseImageButton.SetImageResource(setToClear
                ? Resource.Drawable.ic_clear_white_24dp
                : Resource.Drawable.ic_arrow_back_white_24dp);
        }

        private void OnSearchItemSelected(object sender, AdapterView.ItemClickEventArgs e)
        {
            var a = 5;
            //var item = SearchListView.Adapter.GetItem(e.Position) as Search
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
                ToggleSearch();
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
            //else if (_searchItem.IsActionViewExpanded)
            //{
            //    CloseSearch(true);
            //}
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
        #endregion

        #region tests
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
        #endregion

        private enum NavDraweritems
        {
            Map = 0,
            History = 1
        }
    }
}