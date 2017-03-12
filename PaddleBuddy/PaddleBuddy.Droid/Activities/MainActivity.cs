using System;
using System.Linq;
using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.Content;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Text;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Newtonsoft.Json;
using PaddleBuddy.Core.Models;
using PaddleBuddy.Core.Services;
using PaddleBuddy.Core.Utilities;
using PaddleBuddy.Droid.Adapters;
using PaddleBuddy.Droid.Controls;
using PaddleBuddy.Droid.Fragments;
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
        private ListView SearchListView1 { get; set; }
        private ListView SearchListView2 { get; set; }
        private LinearLayout OverallSearchLayout { get; set; }
        private MainActivitySearchAdapter SearchAdapter1 { get; set; }
        private MainActivitySearchAdapter SearchAdapter2 { get; set; }
        private ClearEditText ClearEditText1 { get; set; }
        private ClearEditText ClearEditText2 { get; set; }
        private View SearchLayout2 { get; set; }
        private ImageButton CloseSearchImageButton { get; set; }
        private SearchItem _selectedSearchItem1;
        private SearchItem _selectedSearchItem2;

        private int DEFAULT_FRAGMENT_ID;
        private InputMethodManager InputMethodManager => (InputMethodManager) GetSystemService(InputMethodService);

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
            SearchListView1 = FindViewById<ListView>(Resource.Id.search_listview1);
            SearchListView2 = FindViewById<ListView>(Resource.Id.search_listview2);
            OverallSearchLayout = FindViewById<LinearLayout>(Resource.Id.overall_search_layout);
            ClearEditText1 = FindViewById<ClearEditText>(Resource.Id.search1_clearedittext);
            ClearEditText2 = FindViewById<ClearEditText>(Resource.Id.search2_clearedittext);
            CloseSearchImageButton = FindViewById<ImageButton>(Resource.Id.search_backarrow);
            SearchLayout2 = FindViewById<View>(Resource.Id.search2_layout);
            ClearEditText1.SetHint("Search here...");
            ClearEditText2.SetHint("Search here...");
            CloseSearchImageButton.SetColorFilter(
                new Color(ContextCompat.GetColor(ApplicationContext, Resource.Color.gray)));
            CloseSearchImageButton.Click += (s, e) => CloseSearch();
            ClearEditText1.EditText.TextChanged += EditText1OnTextChanged;
            ClearEditText2.EditText.TextChanged += EditText2OnTextChanged;
            ClearEditText1.TextCleared += Text1Cleared;
            ClearEditText2.TextCleared += Text2Cleared;
            SearchAdapter1 = new MainActivitySearchAdapter(this);
            SearchAdapter1.UpdateData();
            SearchAdapter2 = new MainActivitySearchAdapter(this);
            SearchListView1.Adapter = SearchAdapter1;
            SearchListView2.Adapter = SearchAdapter2;

            OverallSearchLayout.Clickable = true;
            OverallSearchLayout.Click += (s, e) => { CloseSearch(); };
            SearchListView1.ItemClick += OnSearchItem1Selected;
            SearchListView2.ItemClick += OnSearchItem2Selected;
            HideClearEditText2();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.main_menu, menu);
            return base.OnCreateOptionsMenu(menu);
        }
        #endregion

        #region search

        private SearchItem SelectedSearchItem1
        {
            get { return _selectedSearchItem1; }
            set
            {
                var tempValue = value;
                if (tempValue == null)
                {
                    _selectedSearchItem1 = null;
                    HideClearEditText2();
                }
                else
                {
                    ClearEditText1.EditText.Text = tempValue.Title;
                    _selectedSearchItem1 = tempValue;
                    ShowClearEditText2();
                }
            }
        }

        private SearchItem SelectedSearchItem2
        {
            get { return _selectedSearchItem2; }
            set
            {
                _selectedSearchItem2 = value;
                if (_selectedSearchItem2 != null)
                {
                    ClearEditText2.Text = _selectedSearchItem2.Title;
                }
            }
        }

        private void Text1Cleared()
        {
            SelectedSearchItem1 = null;
        }

        private void Text2Cleared()
        {
            SelectedSearchItem2 = null;
        }

        private void OnSearchItem1Selected(object sender, AdapterView.ItemClickEventArgs e)
        {
            SelectedSearchItem1 = SearchAdapter1.GetSearchItem(e.Position);
        }

        private void OnSearchItem2Selected(object sender, AdapterView.ItemClickEventArgs e)
        {
            SelectedSearchItem2 = SearchAdapter2.GetSearchItem(e.Position);
        }

        private void EditText1OnTextChanged(object sender, TextChangedEventArgs textChangedEventArgs)
        {
            SearchAdapter1.Filter.InvokeFilter(textChangedEventArgs.Text.ToString());
        }

        private void EditText2OnTextChanged(object sender, TextChangedEventArgs e)
        {
            SearchAdapter2.Filter.InvokeFilter(e.Text.ToString());
        }

        private void ToggleSearch()
        {
            if (OverallSearchLayout.Visibility == ViewStates.Gone)
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
            ClearEditText1.Text = "";
            OverallSearchLayout.Visibility = ViewStates.Visible;
            ClearEditText1.EditText.RequestFocusFromTouch();
            InputMethodManager.ShowSoftInput(ClearEditText1.EditText, ShowFlags.Implicit);
            Toolbar.Visibility = ViewStates.Gone;
        }

        private void CloseSearch()
        {
            ClearSearch();
            OverallSearchLayout.Visibility = ViewStates.Gone;
            Toolbar.Visibility = ViewStates.Visible;
            var view = CurrentFocus;
            if (view != null)
            {
                InputMethodManager.HideSoftInputFromWindow(view.WindowToken,
                    HideSoftInputFlags.None);
            }
        }

        private void ClearSearch()
        {
            ClearEditText1.Text = "";
            ClearEditText2.Text = "";
            SelectedSearchItem1 = null;
            SelectedSearchItem2 = null;
        }

        private void ShowClearEditText2()
        {
            SearchLayout2.Visibility = ViewStates.Visible;
            SearchListView1.Visibility = ViewStates.Gone;
            SearchListView2.Visibility = ViewStates.Visible;
            ClearEditText2.Text = "";
            SearchAdapter2.UpdateData(_selectedSearchItem1.Item);
            SelectedSearchItem2 = null;
        }

        private void HideClearEditText2()
        {
            SearchLayout2.Visibility = ViewStates.Gone;
            SearchListView1.Visibility = ViewStates.Visible;
            SearchListView2.Visibility = ViewStates.Gone;
            ClearEditText2.Text = "";
            SelectedSearchItem2 = null;
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
            History = 1
        }
    }
}