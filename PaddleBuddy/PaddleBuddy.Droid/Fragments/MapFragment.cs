using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Graphics;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.Content;
using Android.Support.V4.Content.Res;
using Android.Text;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using PaddleBuddy.Core.Models;
using PaddleBuddy.Core.Models.Map;
using PaddleBuddy.Core.Models.Messages;
using PaddleBuddy.Core.Services;
using PaddleBuddy.Core.Utilities;
using PaddleBuddy.Droid.Activities;
using PaddleBuddy.Droid.Adapters;
using PaddleBuddy.Droid.Controls;
using PaddleBuddy.Droid.Services;
using PaddleBuddy.Droid.Utilities;
using UnitsNet;
using UnitsNet.Extensions.NumberToDuration;
using Path = PaddleBuddy.Core.Models.Map.Path;
using Point = PaddleBuddy.Core.Models.Map.Point;

namespace PaddleBuddy.Droid.Fragments
{
    public class MapFragment : BaseFragment, IOnMapReadyCallback, GoogleMap.IOnMarkerClickListener, GoogleMap.IOnMapClickListener, GoogleMap.IInfoWindowAdapter, GoogleMap.IOnCameraMoveStartedListener
    {
        #region member variables
        private bool _isLoading;
        private bool _isBrowsing;

        private bool MapIsNull => MyMap == null;

        private GoogleMap MyMap { get; set; }
        private MapView MapView { get; set; }
        private MapModes MapMode { get; set; }
        private Point SelectedMarkerPoint { get; set; }
        private ProgressBarOverlay ProgressBarOverlay { get; set; }
        private TextView SpeedTextView { get; set; }
        private LinearLayout NavBarLayout { get; set; }
        private View DetailsBarLayout { get; set; }
        private View SearchDetailsBarLayout { get; set; }
        private TextView SearchDetailsTitle { get; set; }
        /// <summary>
        /// speed, time of day in totalseconds
        /// </summary>
        private List<Tuple<Speed, Duration>> Speeds { get; set; }
        private TripManager TripManager { get; set; }
        private MapImageButton StopBrowsingButton { get; set; }
        private FloatingActionButton NavFab { get; set; }
        private MarkerOptions _unselectedMapMarkerOptions;
        private MarkerOptions _currentMarkerOptions;

        //markers
        private Marker CurrentDestinationMarker { get; set; }
        private Marker CurrentLoationMarker { get; set; }
        private List<Marker> LaunchSiteMarkers { get; set; }
        private Polyline CurrentPolyline { get; set; }


        private TextView DetailsBarTextView1;
        private TextView DetailsBarTextView2;
        private TextView DetailsBarTextView3;


        private static readonly int NAV_ZOOM = 16;
        private static readonly int BROWSE_ZOOM = 8;
        private static readonly int NAV_TILT = 70;
        private static readonly int BROWSE_TILT = 0;
        private static readonly Duration SPEED_CUTOFF_TIME_IN_SECONDS = 10.Seconds();

        private const int CURRENT_MARKER_COLORID = Resource.Color.colorPrimaryDark;
        private const int MAP_MARKER_COLORID = Resource.Color.colorPrimary;
        private const int MAP_MARKER_COLORID_SELECTED = Resource.Color.colorAccent;

        private ListView SearchListView { get; set; }
        private LinearLayout OverallSearchLayout { get; set; }
        private MainActivitySearchAdapter SearchAdapter { get; set; }
        private ClearEditText SearchClearEditText { get; set; }
        private View SearchResultsCardView { get; set; }
        private ImageButton CloseSearchImageButton { get; set; }
        private SearchItem _selectedSearchItem;


        #endregion

        #region init

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.fragment_map, container, false);
            MapView = (MapView)view.RootView.FindViewById(Resource.Id.map_view);
            MapView.OnCreate(savedInstanceState);
            SpeedTextView = view.FindViewById<TextView>(Resource.Id.speed_textview);
            DetailsBarLayout = view.FindViewById(Resource.Id.details_layout);
            NavBarLayout = view.FindViewById<LinearLayout>(Resource.Id.navbar_details_layout);
            SearchDetailsBarLayout = view.FindViewById(Resource.Id.search_details_layout);
            SearchDetailsTitle = view.FindViewById<TextView>(Resource.Id.search_details_title);
            DetailsBarTextView1 = view.FindViewById<TextView>(Resource.Id.mapbar_text1);
            DetailsBarTextView2 = view.FindViewById<TextView>(Resource.Id.mapbar_text2);
            DetailsBarTextView3 = view.FindViewById<TextView>(Resource.Id.mapbar_text3);
            ProgressBarOverlay = view.FindViewById<ProgressBarOverlay>(Resource.Id.map_isloading_overlay);
            SearchListView = view.FindViewById<ListView>(Resource.Id.search_listview);
            OverallSearchLayout = view.FindViewById<LinearLayout>(Resource.Id.overall_search_layout);
            SearchClearEditText = view.FindViewById<ClearEditText>(Resource.Id.search_clearedittext);
            CloseSearchImageButton = view.FindViewById<ImageButton>(Resource.Id.search_backarrow);
            SearchResultsCardView = view.FindViewById(Resource.Id.search_results_cardview);
            StopBrowsingButton = view.FindViewById<MapImageButton>(Resource.Id.stop_browsing_button);
            NavFab = view.FindViewById<FloatingActionButton>(Resource.Id.nav_fab);

            CloseSearchImageButton.SetColorFilter(
                new Color(ContextCompat.GetColor(Context, Resource.Color.gray)));
            CloseSearchImageButton.Click += (s, e) => OnCloseSearchClicked();
            SearchClearEditText.EditText.TextChanged += EditTextOnTextChanged;
            SearchClearEditText.FocusChange += OnSearchFocused;
            view.FindViewById<Button>(Resource.Id.test_simulate_button).Click += OnSimulateButtonClicked;
            view.FindViewById<ImageButton>(Resource.Id.cancel_trip_button).Click += OnCancelTripClicked;
            SearchClearEditText.TextCleared += OnTextCleared;
            SearchAdapter = new MainActivitySearchAdapter(Activity);
            SearchListView.Adapter = SearchAdapter;
            StopBrowsingButton.Click += OnStopBrowsingButtonClicked;
            NavFab.Click += OnNavFabClicked;

            OverallSearchLayout.Clickable = true;
            OverallSearchLayout.Click += (s, e) => { CloseSearch(); };
            SearchListView.ItemClick += OnSearchItemSelected;
            ShowLoading("waiting on gps");
            LaunchSiteMarkers = new List<Marker>();

            Speeds = new List<Tuple<Speed, Duration>>();
            CurrentLoationMarker = null;
            SelectedMarkerPoint = null;
            LaunchSiteMarkers = new List<Marker>();
            SetupBrowse();

            SearchClearEditText.SetHint("Search here...");
            SearchAdapter.UpdateData();

            if (!PBPrefs.TestOffline)
            {
                MapView.OnResume();
                try
                {
                    MapsInitializer.Initialize(Application.Context);
                }
                catch (Exception e)
                {
                    LogService.ExceptionLog("Problem initializing map");
                    LogService.ExceptionLog(e.Message);
                }
                MapView.GetMapAsync(this);
            }
            IsBrowsing = false;
            return view;
        }

        public void OnMapReady(GoogleMap googleMap)
        {
            MyMap = googleMap;
            MyMap.SetOnMarkerClickListener(this);
            MyMap.SetOnMapClickListener(this);
            MyMap.SetInfoWindowAdapter(this);
            MyMap.SetOnCameraMoveStartedListener(this);
            if (CurrentLocation != null)
            {
                LocationUpdatedReceived(null);
            }
        }

        public override void OnStop()
        {
            base.OnStop();
            PrepareForClose();
        }

        public override void OnStart()
        {
            base.OnStart();
            PrepareForStart();
        }

        private void PrepareForStart()
        {
            LocationService.GetInstance().StartListening();
            MessengerService.Messenger.Register<LocationUpdatedMessage>(this, LocationUpdatedReceived);
        }

        private void PrepareForClose()
        {
            LocationService.GetInstance().StopListening();
            MessengerService.Messenger.Unregister<LocationUpdatedMessage>(this);
        }

        public static MapFragment NewInstance()
        {
            return new MapFragment();
        }

        private void ShowLoading(string str = "")
        {
            ProgressBarOverlay.Visibility = ViewStates.Visible;
            ProgressBarOverlay.SetText(str);
            IsLoading = true;
        }

        private void HideLoading()
        {
            ProgressBarOverlay.Visibility = ViewStates.Gone;
            IsLoading = false;
        }
        
        #endregion

        #region search

        private SearchItem SelectedSearchItem
        {
            get { return _selectedSearchItem; }
            set
            {
                _selectedSearchItem = value;
                if (_selectedSearchItem == null)
                {
                    ShowSearchResults();
                    HideNavFab();
                }
                else
                {
                    if (_selectedSearchItem.Item.IsTypeOf(typeof (Point)))
                    {
                        SearchClearEditText.EditText.Text = _selectedSearchItem.Title;
                        OpenSearch();
                        UpdateSearchDetails(_selectedSearchItem);
                        ShowNavFab();

                    }
                    else if (_selectedSearchItem.Item.IsTypeOf(typeof(River)))
                    {

                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                    HideKeyboard();
                    HideSearchResults();
                    IsBrowsing = true;
                }
            }
        }

        private void OnTextCleared()
        {
            SelectedSearchItem = null;
            ClearSelectedMarkers();
        }

        private void OnSearchItemSelected(object sender, AdapterView.ItemClickEventArgs e)
        {
            SelectedSearchItem = SearchAdapter.GetSearchItem(e.Position);
        }

        private void OnSearchFocused(object sender, View.FocusChangeEventArgs e)
        {
            ShowSearchResults();
            HideDetailsBar();
        }

        private void EditTextOnTextChanged(object sender, TextChangedEventArgs textChangedEventArgs)
        {
            ShowSearchResults();
            SearchAdapter.Filter.InvokeFilter(textChangedEventArgs.Text.ToString());
        }

        public void ToggleSearch()
        {
            if (OverallSearchLayout.Visibility == ViewStates.Gone)
            {
                OpenSearch();
                ShowSearchResults();
            }
            else
            {
                CloseSearch();
            }
        }

        private void OpenSearch(bool requestFocus = false)
        {
            if (IsSearchOpen) return; 
            ((MainActivity) Activity).SupportActionBar.Hide();
            SearchClearEditText.Text = "";
            OverallSearchLayout.Visibility = ViewStates.Visible;
            if (!requestFocus) return;
            SearchClearEditText.EditText.RequestFocusFromTouch();
            InputMethodManager.ShowSoftInput(SearchClearEditText.EditText, ShowFlags.Implicit);
        }

        public void CloseSearch()
        {
            ClearSearch();
            HideDetailsBar();
            OverallSearchLayout.Visibility = ViewStates.Gone;
            HideKeyboard();
            HideNavFab();
            ((MainActivity)Activity).SupportActionBar.Show();
        }

        private void ClearSearch()
        {
            SearchClearEditText.Text = "";
            SelectedSearchItem = null;
        }

        private void ShowSearchResults()
        {
            SearchResultsCardView.Visibility = ViewStates.Visible;
            HideDetailsBar();
        }

        private void HideSearchResults()
        {
            SearchResultsCardView.Visibility = ViewStates.Gone;
        }

        private void ShowNavFab()
        {
            NavFab.Visibility = ViewStates.Visible;
        }

        private void HideNavFab()
        {
            NavFab.Visibility = ViewStates.Gone;
        }

        private bool IsSearchOpen => OverallSearchLayout.IsVisible();

        #endregion

        #region update

        private void LocationUpdatedReceived(LocationUpdatedMessage obj)
        {
            if (MapIsNull) return;
            Activity.RunOnUiThread(() =>
            {
                HideLoading();
                if (CurrentLocation != null)
                {
                    DrawCurrent();
                }
                switch (MapMode)
                {
                    case MapModes.Browse:
                        BrowseUpdate();
                        break;
                    case MapModes.Navigate:
                        NavigationUpdate();
                        break;
                    default:
                        throw new NotImplementedException();
                }
            });
            
        }

        private void NavigationUpdate()
        {
            if (IsLoading || MapIsNull) return;
            if (TripManager == null || TripManager.Points == null || TripManager.Points.Count < 1)
            {
                LogService.Log("No navigation update. tripData not configured correctly");
                return;
            }
            DrawCurrentTrip();
            TripManager.AddToPointHistory(CurrentLocation);
            if (TripManager.HasStarted)
            {
                if (TripManager.IsOnTrack(TripManager.PreviousCheckPoint, TripManager.CurrentCheckPoint, CurrentLocation))
                {
                    NavigateCamera();
                    UpdateNavBar();
                    if (TripManager.CloseToNext(CurrentLocation))
                    {
                        if (TripManager.HasNext)
                        {
                            TripManager.Increment();
                        }
                        else
                        {
                            FinishTrip();
                            return;
                        }
                    }
                    UpdateSpeed();
                }
                else
                {
                    HideSpeed();
                    var newDestination = DatabaseService.GetInstance().PickNextDestination(CurrentLocation, TripManager);
                    AnimateCameraBounds(new List<Point> {CurrentLocation, newDestination});
                    TripManager.UpdateForNewDestination(newDestination);
                    Length distance;
                    if (TripManager.HasPrevious)
                    {
                        distance =
                            PBMath.DistanceFromPointToLineSegment(TripManager.PreviousCheckPoint,
                                TripManager.CurrentCheckPoint,
                                CurrentLocation);
                    }
                    else
                    {
                        distance = PBMath.Distance(CurrentLocation, TripManager.CurrentCheckPoint);
                    }
                    UpdateNavBar($"Navigate to river - {PBUtilities.FormatDistanceToMilesOrMeters(distance)}");
                }
            }
            else
            {
                if (TripManager.CloseToNext(CurrentLocation))
                {
                    if (TripManager.HasNext)
                    {
                        TripManager.Increment();
                    }
                    else
                    {
                        FinishTrip();
                    }
                }
                else
                {
                    HideSpeed();
                    if (!IsBrowsing)
                    {
                        AnimateNavigationUpdate();
                    }
                    UpdateNavBar("Navigate to river");
                }
            }
        }

        private void BrowseUpdate()
        {
            if (IsLoading || MapIsNull || IsBrowsing ) return;
            try
            {
                DrawCurrentBrowsePathAndSites();
                var points = new List<LatLng> { CurrentLocation.ToLatLng()};
                points.AddRange(CurrentPolyline.Points);
                AnimateCameraBounds(points.ToArray());
            }
            catch (Exception e)
            {
                LogService.ExceptionLog(e.Message);
                throw e;
            }
        }

        private void SetupBrowse()
        {
            MapMode = MapModes.Browse;
            ClearCurrentLineAndMarkers();
            HideSpeed();
            HideDetailsBar();
            SimulatorService.GetInstance().StopSimulating();
            ClearTripData();
        }

        private void SetupNavigate(int destinationPointId)
        {
            var path = DatabaseService.GetInstance().GetPathWithDestination(destinationPointId);
            SetupNavigate(path?.Points);
        }

        private void SetupNavigate(List<Point> points)
        {
            if (points == null || points.Count < 1)
            {
                LogService.Log("no points to navigate to");
                return;
            };
            MapMode = MapModes.Navigate;
            TripManager = new TripManager {Points = points};
            ClearCurrentLineAndMarkers();
            NavigationUpdate();
        }

        private void StartSimulating()
        {
            List<Point> points = new List<Point>();
            if (TripManager != null && TripManager.Points != null && TripManager.Points.Count > 1)
            {
                points = TripManager.Points;
            }
            else
            {
                points = DatabaseService.GetInstance().GetPath(PBPrefs.RiverIdToSimulate).Points;
                if (points.Count <= 1)
                {
                    LogService.Log("No points in startsimulating");
                    return;
                }
            }
            SetupNavigate(points);
            SimulatorService.GetInstance().StartSimulating(points);
        }

        private Point CurrentLocation
        {
            get { return LocationService.GetInstance().CurrentLocation; }
        }

        private bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                _isLoading = MapIsNull || value;
            }
        }

        private bool IsBrowsing
        {
            get { return _isBrowsing; }
            set
            {
                _isBrowsing = value;
                StopBrowsingButton.Visibility = _isBrowsing ? ViewStates.Visible : ViewStates.Gone;
            }
        }

        #endregion

        #region speed

        private void UpdateSpeed()
        {
            var newTime = CurrentLocation.Time;
            if (CurrentLocation.Speed.MetersPerSecond > 0)
            {
                Speeds.Add(new Tuple<Speed, Duration>(CurrentLocation.Speed, CurrentLocation.Time));
            }
            for (int i = Speeds.Count - 1; i >= 0; i--)
            {
                if (newTime - Speeds[i].Item2 > SPEED_CUTOFF_TIME_IN_SECONDS)
                {
                    Speeds.RemoveAt(i);
                }
            }
            if (Speeds.Count > 0)
            {
                var avg = Speeds.Average(item => item.Item1.MetersPerSecond);
                if (avg > 0)
                {
                    SpeedTextView.Text = avg.ToString("0.0");
                    SpeedTextView.Visibility = ViewStates.Visible;
                }
            }
        }

        private void HideSpeed()
        {
            SpeedTextView.Visibility = ViewStates.Gone;
        }
        #endregion

        #region details bar

        private void UpdateNavBar(string text1 = null)
        {
            ShowNavBar();
            if (string.IsNullOrWhiteSpace(text1))
            {
                var points = TripManager.RemainingPoints;
                points.Insert(0, CurrentLocation);
                var tripEstimate = PBMath.PointsToEstimate(points);
                DetailsBarTextView1.Text = tripEstimate.TimeRemaining;
                DetailsBarTextView2.Text = PBUtilities.FormatDistanceToMilesOrMeters(tripEstimate.Distance);
                DetailsBarTextView3.Text = DateTime.Now.Add(tripEstimate.Time).ToStringHrsMinsAmPm();
            }
            else
            {
                DetailsBarTextView1.Text = text1;
                DetailsBarTextView2.Text = "";
                DetailsBarTextView2.Text = "";
            }
        }

        private void UpdateSearchDetails(SearchItem item)
        {
            ShowSearchDetailsBar();
            if (item.Item.IsTypeOf(typeof (River)))
            {
                var river = (River) item.Item;
                SearchDetailsTitle.Text = river.Name;
                AnimateCameraAndDrawRiver(river);
            } else if (item.Item.IsTypeOf(typeof (Point)))
            {
                var point = (Point) item.Item;
                SearchDetailsTitle.Text = point.Label;
                MoveCameraZoom(point, 15);
            }
        }

        private void ShowSearchDetailsBar()
        {
            ShowDetailsBar();
            SearchDetailsBarLayout.Visibility = ViewStates.Visible;
            HideNavBar();
        }

        private void ShowNavBar()
        {
            ShowDetailsBar();
            NavBarLayout.Visibility = ViewStates.Visible;
            HideSearchDetailsBar();
        }

        private void ShowDetailsBar()
        {
            DetailsBarLayout.Visibility = ViewStates.Visible;
        }

        private void HideSearchDetailsBar()
        {
            SearchDetailsBarLayout.Visibility = ViewStates.Gone;
        }

        private void HideNavBar()
        {
            NavBarLayout.Visibility = ViewStates.Gone;
        }

        private void HideDetailsBar()
        {
            DetailsBarLayout.Visibility = ViewStates.Gone;
        }
        #endregion

        #region trip

        private void ClearTripData()
        {
            TripManager = null;
            CurrentPolyline = null;
            CurrentDestinationMarker = null;
        }

        private void FinishTrip()
        {
            LogService.Log("finished trip");
            PrepareForClose();
            NavigateTo(TripSummaryFragment.NewInstance(), PBPrefs.SERIALIZABLE_TRIPSUMMARY, TripManager.ExportTripSummary());
        }
        #endregion

        #region clicks

        private void OnCloseSearchClicked()
        {
            CloseSearch();
            ClearSelectedMarkers();
        }

        private void OnNavFabClicked(object sender, EventArgs e)
        {
            if (SelectedSearchItem.Item.IsTypeOf(typeof(Point)))
            {
                SetupNavigate(((Point)SelectedSearchItem.Item).Id);
                NavigationUpdate();
                CloseSearch();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private void OnStopBrowsingButtonClicked(object sender, EventArgs e)
        {
            IsBrowsing = false;
            if (MapMode == MapModes.Browse)
            {
                BrowseUpdate();
            }
        }

        public void OnCameraMoveStarted(int reason)
        {
            if (reason == GoogleMap.OnCameraMoveStartedListener.ReasonGesture)
            {
                IsBrowsing = true;
                HideSearchResults();
            }
        }

        private void OnCancelTripClicked(object sender, EventArgs e)
        {
            SetupBrowse();
            BrowseUpdate();
        }

        private void OnSimulateButtonClicked(object sender, EventArgs e)
        {
            StartSimulating();
        }

        public bool OnMarkerClick(Marker marker)
        {
            if (MapMode.Equals(MapModes.Browse))
            {
                try
                {
                    var id = int.Parse(marker.Snippet);
                    SelectedMarkerPoint = DatabaseService.GetInstance().GetPoint(id);
                }
                catch (Exception e)
                {
                    LogService.ExceptionLog("Issue in map fragment marker click");
                    LogService.ExceptionLog(e.Message);
                    throw e;
                }
                if (SelectedMarkerPoint != null)
                {
                    SelectedSearchItem = SelectedMarkerPoint.ToSearchItem();
                    ChangeMapMarkerToSelected(marker);
                }
            }
            return true;
        }

        public void OnMapClick(LatLng point)
        {
            SelectedMarkerPoint = null;
            HideSearchResults();
        }
        #endregion

        #region draw

        private void ClearCurrentLineAndMarkers()
        {
            CurrentPolyline?.Remove();
            CurrentPolyline = null;
            if (LaunchSiteMarkers != null)
            {
                foreach (var marker in LaunchSiteMarkers)
                {
                    marker.Remove();
                }
            }
            LaunchSiteMarkers = null;
            CurrentDestinationMarker?.Remove();
            CurrentDestinationMarker = null;
        }

        private void DrawCurrentTrip(bool reDraw = false)
        {
            if (MapIsNull) return;
            if (reDraw)
            {
                CurrentPolyline.Remove();
                CurrentPolyline = null;
            }
            if (CurrentPolyline != null) return;
            CurrentPolyline = DrawLine(TripManager.Points);
            DrawCurrentDestination();
        }

        private void DrawCurrentBrowsePathAndSites()
        {
            DrawRiverAndLaunchSites(DatabaseService.GetInstance().GetClosestRiverId());
        }

        private Polyline DrawLine(List<Point> points)
        {
            if (MapIsNull) return null;
            var polyOpts = new PolylineOptions()
                .InvokeColor(Resource.Color.black)
                .InvokeWidth(9)
                .InvokeZIndex(1);
            polyOpts.Add(points.ToLatLngs());
            return MyMap.AddPolyline(polyOpts);
        }

        private Polyline DrawLine(Path path)
        {
            if (MapIsNull || path == null || path.Points.Count < 2) return null;
            return DrawLine(path.Points);
        }

        private Polyline DrawLine(Point start, Point end)
        {
            var path = DatabaseService.GetInstance().GetPath(start, end);
            if (path == null) return null;
            return DrawLine(path);
        }

        private void DrawRiverAndLaunchSites(int id)
        {
            if (MapIsNull) return;
            CurrentPolyline?.Remove();
            if (LaunchSiteMarkers != null)
            {
                foreach (var siteMarker in LaunchSiteMarkers)
                {
                    siteMarker.Remove();
                }
            }
            LaunchSiteMarkers = new List<Marker>();
            var sites = DatabaseService.GetInstance().Points.Where(a => a.IsLaunchSite && a.RiverId == id).ToList();
            foreach (var site in sites)
            {
                LaunchSiteMarkers.Add(DrawMarker(site));
            }
            CurrentPolyline = DrawLine(DatabaseService.GetInstance().GetPath(id));
        }

        private void DrawCurrent()
        {
            try
            {
                if (MapIsNull) return;
                var position = CurrentLocation.ToLatLng();
                if (CurrentLoationMarker != null)
                {
                    CurrentLoationMarker.Position = position;
                }
                else
                {
                    CurrentLoationMarker = MyMap.AddMarker(CurrentMarkerOptions.SetPosition(position));
                }
            }
            catch (Exception e)
            {
                LogService.ExceptionLog(e.Message);
                throw e;
            }
        }

        private void DrawCurrentDestination(Point p = null)
        {
            if (MapIsNull) return;
            if (p == null) p = TripManager.FinalCheckPoint;
            var position = p.ToLatLng();
            if (CurrentDestinationMarker != null)
            {
                CurrentDestinationMarker.Position = position;
            }
            else
            {
                CurrentDestinationMarker = MyMap.AddMarker(new MarkerOptions().SetPosition(position));
            }
        }

        public View GetInfoContents(Marker marker)
        {
            var view = GetLayoutInflater(null).Inflate(Resource.Layout.infowindow_custom_marker, null);
            ((TextView)view.FindViewById(Resource.Id.markerTitle)).Text = SelectedMarkerPoint.Label;
            return view;
        }

        public View GetInfoWindow(Marker marker)
        {
            return null;
        }

        private MarkerOptions CurrentMarkerOptions
        {
            get {
                return _currentMarkerOptions ??
                       (_currentMarkerOptions = CreateCurrentMarkerOptions());
            }
        }

        private MarkerOptions UnselectedMapMarkerOptions
        {
            get {
                return _unselectedMapMarkerOptions ??
                       (_unselectedMapMarkerOptions = CreateUnselectedMapMarkerOptions(CreateMapMarkerIcon()));
            }
        }

        private MarkerOptions CreateCurrentMarkerOptions()
        {
            var opts = new MarkerOptions();
            opts.SetIcon(CreateMarkerIcon(50, 50, Resource.Drawable.current_circle, Resource.Color.red));
            opts.Anchor(0.5f, 0.5f);
            return opts;
        }

        private MarkerOptions CreateUnselectedMapMarkerOptions(BitmapDescriptor descriptor)
        {
            var opts = new MarkerOptions();
            opts.SetIcon(descriptor);
            opts.Anchor(0.5f, 0.9f);
            return opts;
        }

        private BitmapDescriptor CreateMapMarkerIcon(int colorId = MAP_MARKER_COLORID)
        {
            return CreateMarkerIcon(90, 90, Resource.Drawable.ic_room_white_24dp, colorId);
        }

        private BitmapDescriptor CreateMarkerIcon(int width, int height, int drawableResource, int colorId = int.MinValue)
        {
            var bitmap = Bitmap.CreateBitmap(width, height, Bitmap.Config.Argb8888);
            var canvas = new Canvas(bitmap);
            var shape = ResourcesCompat.GetDrawable(Resources, drawableResource, null);
            shape.SetColorFilter(new Color(ContextCompat.GetColor(Context, colorId != int.MinValue ? colorId : Resource.Color.black)), PorterDuff.Mode.SrcAtop);
            shape.SetBounds(0, 0, bitmap.Width, bitmap.Height);
            shape.Draw(canvas);

            return BitmapDescriptorFactory.FromBitmap(bitmap);
        }

        private Marker DrawMarker(Point p)
        {
            if (MapIsNull) return null;
            var marker = UnselectedMapMarkerOptions;
            if (p.IsLaunchSite) marker.SetTitle(p.Label).SetSnippet(p.Id.ToString());
            marker.SetPosition(p.ToLatLng());
            return MyMap.AddMarker(marker);
        }

        private void ChangeMapMarkerToSelected(Marker marker)
        {
            ClearSelectedMarkers();
            if (marker != null)
            {
                ChangeMapMarkerToColor(marker, MAP_MARKER_COLORID_SELECTED);
            }
        }

        private void ClearSelectedMarkers()
        {
            foreach (var m in LaunchSiteMarkers)
            {
                ChangeMapMarkerToColor(m, MAP_MARKER_COLORID);
            }
        }

        private void ChangeMapMarkerToColor(Marker marker, int colorId)
        {
            marker.SetIcon(CreateMapMarkerIcon(colorId));
        }

        #endregion

        #region camera

        public void NavigateCamera()
        {
            if (MapIsNull) return;
            //trying to avoid setting bearing when points are too close to accurately calulate it
            var dist = PBMath.Distance(CurrentLocation, TripManager.CurrentCheckPoint);
            if (dist < PBPrefs.BearingTooCloseThreshold)
                return;
            var bearing = PBMath.BearingBetweenPoints(CurrentLocation, TripManager.CurrentCheckPoint);
            var camTarget = PBMath.PointAtDistanceAlongBearing(CurrentLocation, PBPrefs.DistanceAheadToAim, bearing);
            var camPos = CameraUpdateBuilder(camTarget, NAV_TILT, NAV_ZOOM, bearing);
            MyMap.AnimateCamera(camPos);
        }
        
        private void MoveCameraZoom(Point p = null, int zoom = 0)
        {
            if (MapIsNull) return;
            if (p == null) p = CurrentLocation;

            var camPos = CameraUpdateBuilder(p, BROWSE_TILT, zoom);
            MyMap.AnimateCamera(camPos);
        }

        private void AnimateCameraAndDrawRiver(River river)
        {
            var path = DatabaseService.GetInstance().GetPath(river.Id);
            if (path?.Points != null && path.Points.Count > 0)
            {
                AnimateCameraBounds(path.Points);
                DrawRiverAndLaunchSites(river.Id);
            }
        }

        private void AnimateCameraBounds(List<Point> points)
        {
            AnimateCameraBounds(points.ToLatLngs());
        }

        private void AnimateCameraBounds(LatLng[] points)
        {
            if (MapIsNull) return;
            var builder = new LatLngBounds.Builder();
            foreach (var p in points)
            {
                builder.Include(p);
            }
            var bounds = builder.Build();
            var cameraUpdate = CameraUpdateFactory.NewLatLngBounds(bounds, View.Width, View.Height, 80);
            MyMap.AnimateCamera(cameraUpdate);
        }

        private void AnimateNavigationUpdate()
        {
            AnimateCameraBounds(new List<Point>{CurrentLocation, TripManager.CurrentCheckPoint});
        }

        private CameraUpdate CameraUpdateBuilder(Point p, int tilt = int.MaxValue, int zoom = 0, double bearing = double.NaN)
        {
            var camPos = new CameraPosition.Builder(MyMap.CameraPosition).Target(new LatLng(p.Lat, p.Lng));
            if (tilt != int.MaxValue) camPos.Tilt(tilt);
            if (zoom != 0) camPos.Zoom(zoom);
            if (!bearing.Equals(double.NaN)) camPos.Bearing((float)bearing);
            return CameraUpdateFactory.NewCameraPosition(camPos.Build());
        }
        #endregion

        public enum MapModes
        {
            InitFromPlan,
            Browse,
            Navigate
        }
    }
}