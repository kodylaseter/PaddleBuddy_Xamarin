using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
using Path = PaddleBuddy.Core.Models.Map.Path;
using Point = PaddleBuddy.Core.Models.Map.Point;

namespace PaddleBuddy.Droid.Fragments
{
    public class MapFragment : BaseFragment, IOnMapReadyCallback, GoogleMap.IOnMarkerClickListener, GoogleMap.IOnMapClickListener, GoogleMap.IInfoWindowAdapter, GoogleMap.IOnCameraMoveStartedListener
    {
        #region member variables
        private bool _isLoading;
        private bool _isBrowsing;

        private GoogleMap MyMap { get; set; }
        private MapView MapView { get; set; }
        private MapModes MapMode { get; set; }
        private Point SelectedMarkerPoint { get; set; }
        private ProgressBarOverlay ProgressBarOverlay { get; set; }
        private MarkerOptions _currentMarkerOptions;
        private Marker CurrentLoationMarker { get; set; }
        private Marker SelectedMarker { get; set; }
        private Marker CurrentDestinationMarker { get; set; }
        private List<Marker> LaunchSiteMarkers { get; set; }
        private Polyline CurrentPolyline { get; set; }
        private TextView SpeedTextView { get; set; }
        private LinearLayout NavBarLayout { get; set; }
        private View DetailsBarLayout { get; set; }
        private View SearchDetailsBarLayout { get; set; }
        private TextView SearchDetailsTitle { get; set; }
        /// <summary>
        /// speed, time of day in totalseconds
        /// </summary>
        private List<Tuple<double, double>> Speeds { get; set; }
        private TripManager TripManager { get; set; }
        private MapImageButton StopBrowsingButton { get; set; }
        private bool MapIsNull => MyMap == null;
        private FloatingActionButton NavFab { get; set; }

        private TextView DetailsBarTextView1;
        private TextView DetailsBarTextView2;
        private TextView DetailsBarTextView3;


        private const int NAV_ZOOM = 16;
        private const int BROWSE_ZOOM = 8;
        private const int NAV_TILT = 70;
        private const int BROWSE_TILT = 0;
        private const int SPEED_CUTOFF_TIME_IN_SECONDS = 10;

        #region search
        private ListView SearchListView { get; set; }
        private LinearLayout OverallSearchLayout { get; set; }
        private MainActivitySearchAdapter SearchAdapter { get; set; }
        private ClearEditText SearchClearEditText { get; set; }
        private View SearchResultsCardView { get; set; }
        private ImageButton CloseSearchImageButton { get; set; }
        private SearchItem _selectedSearchItem;

        #endregion

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
            CloseSearchImageButton.Click += (s, e) => CloseSearch();
            SearchClearEditText.EditText.TextChanged += EditTextOnTextChanged;
            SearchClearEditText.FocusChange += OnSearchFocused;
            view.FindViewById<Button>(Resource.Id.test_simulate_button).Click += OnSimulateButtonClicked;
            view.FindViewById<ImageButton>(Resource.Id.cancel_trip_button).Click += OnCancelTripClicked;
            SearchClearEditText.TextCleared += TextCleared;
            SearchAdapter = new MainActivitySearchAdapter(Activity);
            SearchListView.Adapter = SearchAdapter;
            StopBrowsingButton.Click += OnStopBrowsingButtonClicked;
            NavFab.Click += OnNavFabClicked;

            OverallSearchLayout.Clickable = true;
            OverallSearchLayout.Click += (s, e) => { CloseSearch(); };
            SearchListView.ItemClick += OnSearchItemSelected;
            ShowLoading("waiting on gps");
            LaunchSiteMarkers = new List<Marker>();

            Speeds = new List<Tuple<double, double>>();
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
                        OpenSearch();
                        SearchClearEditText.EditText.Text = _selectedSearchItem.Title;
                        ShowNavFab();
                    } else if (!_selectedSearchItem.Item.IsTypeOf(typeof (River)))
                    {
                        throw new NotImplementedException();
                    }
                    HideKeyboard();
                    HideSearchResults();
                    UpdateSearchDetails(_selectedSearchItem);
                    IsBrowsing = true;
                }
            }
        }

        private void TextCleared()
        {
            SelectedSearchItem = null;
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

        private void OnNavFabClicked(object sender, EventArgs e)
        {
            CloseSearch();
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
            }
            else
            {
                CloseSearch();
            }
        }

        private void OpenSearch()
        {
            if (IsSearchOpen) return; 
            ((MainActivity) Activity).SupportActionBar.Hide();
            SearchClearEditText.Text = "";
            OverallSearchLayout.Visibility = ViewStates.Visible;
            SearchClearEditText.EditText.RequestFocusFromTouch();
            InputMethodManager.ShowSoftInput(SearchClearEditText.EditText, ShowFlags.Implicit);
            ShowSearchResults();
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
                if (TripManager.IsOnTrack(TripManager.PreviousPoint, TripManager.CurrentPoint, CurrentLocation))
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
                    double distance;
                    if (TripManager.HasPrevious)
                    {
                        distance =
                            PBMath.DistanceInMetersFromPointToLineSegment(TripManager.PreviousPoint,
                                TripManager.CurrentPoint,
                                CurrentLocation);
                    }
                    else
                    {
                        distance = PBMath.DistanceInMeters(CurrentLocation, TripManager.CurrentPoint);
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
                    AnimateCameraBounds(new List<Point> { CurrentLocation, TripManager.CurrentPoint });
                    UpdateNavBar("Navigate to river");
                }
            }
            
        }

        private void BrowseUpdate()
        {
            if (IsLoading || MapIsNull || IsBrowsing ) return;
            try
            {
                CurrentPolyline = DrawCurrentBrowsePathAndSites();
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
            HideSpeed();
            HideDetailsBar();
            SimulatorService.GetInstance().StopSimulating();
            ClearTripData();
            LaunchSiteMarkers = new List<Marker>();
            if (CurrentPolyline != null && CurrentPolyline.Points.Count > 0)
            {
                CurrentPolyline.Remove();
            }
            CurrentPolyline = null;
        }

        private void SetupNavigate(List<Point> points)
        {
            MapMode = MapModes.Navigate;
            TripManager = new TripManager {Points = points};

        }

        private void StartSimulating()
        {
            var p = DatabaseService.GetInstance().GetPath(PBPrefs.RiverIdToSimulate).Points;
            if (p.Count <= 1) return;
            SetupNavigate(p);
            SimulatorService.GetInstance().StartSimulating(p);
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
            if (CurrentLocation.Speed > 0)
            {
                Speeds.Add(new Tuple<double, double>(CurrentLocation.Speed, CurrentLocation.Time));
            }
            //Speeds.RemoveAll(item => newTime - item.Item2 > SPEED_CUTOFF_TIME_IN_SECONDS || newTime - item.Item2 < 0);
            for (int i = Speeds.Count - 1; i >= 0; i--)
            {
                if (newTime - Speeds[i].Item2 > SPEED_CUTOFF_TIME_IN_SECONDS)
                {
                    Speeds.RemoveAt(i);
                }
            }
            if (Speeds.Count > 0)
            {
                var avg = Speeds.Average(item => item.Item1);
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
                CurrentPolyline = AnimateCameraAndDrawRiver(river);
            } else if (item.Item.IsTypeOf(typeof (Point)))
            {
                var point = (Point) item.Item;
                SearchDetailsTitle.Text = point.Label;
                MoveCameraZoom(point, 15);
            }
        }

        private void ShowSearchDetailsBar()
        {
            DetailsBarLayout.Visibility = ViewStates.Visible;
            SearchDetailsBarLayout.Visibility = ViewStates.Visible;
        }

        private void ShowNavBar()
        {
            DetailsBarLayout.Visibility = ViewStates.Visible;
            NavBarLayout.Visibility = ViewStates.Visible;
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
                UpdateSearchDetails(SelectedSearchItem);
            }
            return true;
        }

        public void OnMapClick(LatLng point)
        {
            SelectedMarkerPoint = null;
        }
        #endregion

        #region draw
        private Marker DrawMarker(Point p)
        {
            if (MapIsNull) return null;
            var marker = new MarkerOptions().SetPosition(p.ToLatLng());
            if (p.IsLaunchSite) marker.SetTitle(p.Label).SetSnippet(p.Id.ToString());
            return MyMap.AddMarker(marker);
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
        }

        private Polyline DrawCurrentBrowsePathAndSites()
        {
            return CurrentLocation != null ? DrawRiverAndLaunchSites(DatabaseService.GetInstance().GetClosestRiverId()) : null;
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

        private Polyline DrawRiverAndLaunchSites(int id)
        {
            if (MapIsNull) return null;
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
            return DrawLine(DatabaseService.GetInstance().GetPath(id));
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

        private void DrawCurrentDestination(Point p)
        {
            if (MapIsNull) return;
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
            get
            {
                if (_currentMarkerOptions != null) return _currentMarkerOptions;
                var px = 50;
                var bitmap = Bitmap.CreateBitmap(px, px, Bitmap.Config.Argb8888);
                var canvas = new Canvas(bitmap);
                var shape = ResourcesCompat.GetDrawable(Resources, Resource.Drawable.current_circle, null);
                shape.SetBounds(0, 0, bitmap.Width, bitmap.Height);
                shape.Draw(canvas);
                var markerOpts = new MarkerOptions().SetIcon(BitmapDescriptorFactory.FromBitmap(bitmap))
                    .Anchor(.5f, .5f);
                _currentMarkerOptions = markerOpts;
                return _currentMarkerOptions;

            }
        }
        #endregion

        #region camera
        public void NavigateCamera()
        {
            if (MapIsNull) return;
            //trying to avoid setting bearing when points are too close to accurately calulate it
            var dist = PBMath.DistanceInMeters(CurrentLocation, TripManager.CurrentPoint);
            if (dist < PBPrefs.BearingTooCloseThreshold)
                return;
            var bearing = PBMath.BearingBetweenPoints(CurrentLocation, TripManager.CurrentPoint);
            var camTarget = PBMath.PointAtDistanceAlongBearing(CurrentLocation, PBPrefs.MetersAheadToAim, bearing);
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

        private Polyline AnimateCameraAndDrawRiver(River river)
        {
            var path = DatabaseService.GetInstance().GetPath(river.Id);
            if (path?.Points != null && path.Points.Count > 0)
            {
                AnimateCameraBounds(path.Points);
                return DrawRiverAndLaunchSites(river.Id);
            }
            return null;
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