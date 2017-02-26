using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Content.Res;
using Android.Views;
using Android.Widget;
using PaddleBuddy.Core;
using PaddleBuddy.Core.Models;
using PaddleBuddy.Core.Models.Messages;
using PaddleBuddy.Core.Services;
using PaddleBuddy.Core.Utilities;
using PaddleBuddy.Droid.Activities;
using PaddleBuddy.Droid.Services;
using PaddleBuddy.Droid.Utilities;
using Path = PaddleBuddy.Core.Models.Map.Path;
using Point = PaddleBuddy.Core.Models.Map.Point;

namespace PaddleBuddy.Droid.Fragments
{
    public class MapFragment : BaseFragment, IOnMapReadyCallback, GoogleMap.IOnMarkerClickListener, GoogleMap.IOnMapClickListener, GoogleMap.IInfoWindowAdapter
    {
        private GoogleMap _myMap;
        private bool _isLoading;
        private MapView _mapView;
        private MapModes _mapMode;
        private Point _selectedMarkerPoint;
        private Button _planTripButton;
        private LinearLayout _mapBarLayout;
        private RelativeLayout _progressBarLayout;
        private TextView _mapBarTextView1;
        private TextView _mapBarTextView2;
        private TextView _mapBarTextView3;
        private MarkerOptions _currentMarkerOptions;
        private Marker _currentMarker;
        private Marker _currentDestinationMarker;
        private List<Marker> _launchSiteMarkers;
        private Polyline _currentTripPolyline;
        private Polyline _browsePolyline;


        private const int NAV_ZOOM = 16;
        private const int BROWSE_ZOOM = 8;
        private const int NAV_TILT = 70;
        private const int BROWSE_TILT = 0;

        //speed variables
        private const int SPEED_CUTOFF_TIME_IN_SECONDS = 10;
        private TextView _speedTextView;
        /// <summary>
        /// speed, time of day in totalseconds
        /// </summary>
        private List<Tuple<double, double>> _speeds;
        public TripManager TripManager { get; set; }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.fragment_map, container, false);

            var rootView = view.RootView;
            if (!SysPrefs.DisableMap)
            {
                _mapView = (MapView)rootView.FindViewById(Resource.Id.map_view);
                _mapView.OnCreate(savedInstanceState);
                _mapView.OnResume();
                try
                {
                    MapsInitializer.Initialize(Application.Context);
                }
                catch (Exception e)
                {
                    LogService.ExceptionLog("Problem initializing map");
                    LogService.ExceptionLog(e.Message);
                }
                _mapView.GetMapAsync(this);
            }
            view.FindViewById<Button>(Resource.Id.plan_trip_button).Click += OnPlanTripButtonClicked;
            view.FindViewById<Button>(Resource.Id.test_simulate_button).Click += OnSimulateButtonClicked;
            view.FindViewById<ImageButton>(Resource.Id.cancel_trip_button).Click += OnCancelTripClicked;
            _speedTextView = view.FindViewById<TextView>(Resource.Id.speed_textview);
            _mapBarLayout = view.FindViewById<LinearLayout>(Resource.Id.mapbar_layout);
            _mapBarTextView1 = view.FindViewById<TextView>(Resource.Id.mapbar_text1);
            _mapBarTextView2 = view.FindViewById<TextView>(Resource.Id.mapbar_text2);
            _mapBarTextView3 = view.FindViewById<TextView>(Resource.Id.mapbar_text3);
            _progressBarLayout = view.FindViewById<RelativeLayout>(Resource.Id.map_isloading_overlay);
            IsLoading = true;
            _speeds = new List<Tuple<double, double>>();
            _currentMarker = null;
            _selectedMarkerPoint = null;
            _launchSiteMarkers = new List<Marker>();
            SetupBrowse();
            return view;
        }

        public void OnMapReady(GoogleMap googleMap)
        {
            MyMap = googleMap;
            MessengerService.Messenger.Register<LocationUpdatedMessage>(this, LocationUpdatedReceived);
            MyMap.SetOnMarkerClickListener(this);
            MyMap.SetOnMapClickListener(this);
            MyMap.SetInfoWindowAdapter(this);
            DelayedSetupBrowse();
        }

        private async void DelayedSetupBrowse()
        {
            //todo: improve or debug this
            await Task.Delay(500);
            if (CurrentLocation != null && _browsePolyline == null)
            {
                LocationUpdatedReceived(null);
            }
        }

        private MapModes MapMode
        {
            get { return _mapMode; }
            set
            {
                _mapMode = value;
            }
        }

        private void LocationUpdatedReceived(LocationUpdatedMessage obj)
        {
            if (MapIsNull) return;
            IsLoading = false;
            DrawCurrent();
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
            if (TripManager.HasStarted)
            {
                if (TripManager.IsOnTrack(TripManager.PreviousPoint, TripManager.CurrentPoint, CurrentLocation))
                {
                    NavigateCamera();
                    UpdateMapBar();
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
                    UpdateMapBar($"Navigate to river - {PBUtilities.FormatDistanceToMilesOrMeters(distance)}");
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
                        return;
                    }
                }
                else
                {
                    HideSpeed();
                    AnimateCameraBounds(new List<Point> { CurrentLocation, TripManager.CurrentPoint });
                    UpdateMapBar("Navigate to river");
                }
            }
            
        }

        private void BrowseUpdate()
        {
            if (IsLoading || MapIsNull) return;
            try
            {
                DrawCurrentBrowsePathAndSites(true);
                var points = new List<LatLng> { CurrentLocation.ToLatLng()};
                points.AddRange(_browsePolyline.Points);
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
            HideMapBar();
            SimulatorService.GetInstance().StopSimulating();
            ClearTripData();
            _launchSiteMarkers = new List<Marker>();
        }

        private void SetupNavigate(List<Point> points)
        {
            MapMode = MapModes.Navigate;
            TripManager = new TripManager {Points = points};
        }

        private void ClearTripData()
        {
            TripManager = null;
            _currentTripPolyline = null;
            _currentDestinationMarker = null;
        }

        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                _isLoading = MapIsNull || value;
                _progressBarLayout.Visibility = _isLoading ? ViewStates.Visible : ViewStates.Gone;
            }
        }

        private void UpdateMapBar(string text1 = null)
        {
            ShowMapBar();
            if (string.IsNullOrWhiteSpace(text1))
            {
                var points = TripManager.RemainingPoints;
                points.Insert(0, CurrentLocation);
                var tripEstimate = PBMath.PointsToEstimate(points);
                _mapBarTextView1.Text = tripEstimate.TimeRemaining;
                _mapBarTextView2.Text = PBUtilities.FormatDistanceToMilesOrMeters(tripEstimate.Distance);
                _mapBarTextView3.Text = DateTime.Now.Add(tripEstimate.Time).ToString("hh:mm tt");
            }
            else
            {
                _mapBarTextView1.Text = text1;
                _mapBarTextView2.Text = "";
                _mapBarTextView2.Text = "";
            }
        }

        private void UpdateSpeed()
        {
            var newTime = CurrentLocation.Time;
            if (CurrentLocation.Speed > 0)
            {
                _speeds.Add(new Tuple<double, double>(CurrentLocation.Speed, CurrentLocation.Time));
            }
            //_speeds.RemoveAll(item => newTime - item.Item2 > SPEED_CUTOFF_TIME_IN_SECONDS || newTime - item.Item2 < 0);
            for (int i = _speeds.Count - 1; i >= 0; i--)
            {
                if (newTime - _speeds[i].Item2 > SPEED_CUTOFF_TIME_IN_SECONDS)
                {
                    _speeds.RemoveAt(i);
                }
            }
            if (_speeds.Count > 0)
            {
                var avg = _speeds.Average(item => item.Item1);
                if (avg > 0)
                {
                    _speedTextView.Text = avg.ToString("0.0");
                    _speedTextView.Visibility = ViewStates.Visible;
                }
            }
        }

        private void HideSpeed()
        {
            _speedTextView.Visibility = ViewStates.Gone;
        }

        private void HideMapBar()
        {
            _mapBarLayout.Visibility = ViewStates.Gone;
        }

        private void ShowMapBar()
        {
            _mapBarLayout.Visibility = ViewStates.Visible;
        }

        private GoogleMap MyMap
        {
            get { return _myMap; }
            set { _myMap = value; }
        }

        public Point SelectedMarkerPoint
        {
            get { return _selectedMarkerPoint; }
            set
            {
                _selectedMarkerPoint = value;
                //_planTripButton?.Visibility = _selectedMarkerPoint != null ? ViewStates.Visible : ViewStates.Gone;
            }
        }

        private Point CurrentLocation
        {
            get { return LocationService.GetInstance().CurrentLocation; }
        }

        public static MapFragment NewInstance()
        {
            return new MapFragment();
        }

        private void FinishTrip()
        {
            LogService.Log("finished trip");
            LocationService.GetInstance().StopListening();
            MessengerService.Messenger.Unregister<LocationUpdatedMessage>(this);
            NavigateTo(TripSummaryFragment.NewInstance());
        }

        private void StartSimulating(int type)
        {
            var p = new List<Point>();
            switch (type)
            {
                case 0: //random bad test
                    p.Add(DatabaseService.GetInstance().GetPoint(86));
                    p.Add(DatabaseService.GetInstance().GetPoint(87));
                    p.Add(DatabaseService.GetInstance().GetPoint(88));
                    p.Add(DatabaseService.GetInstance().GetPoint(89));
                    SetupNavigate(p);
                    SimulatorService.GetInstance().StartSimulating(TripManager.Points);
                    break;
                case 1: //chat test 7-42
                    p = DatabaseService.GetInstance().GetPath(2).Points;
                    SetupNavigate(p);
                    SimulatorService.GetInstance().StartSimulating(TripManager.Points);
                    break;
                case 2:
                    p = DatabaseService.GetInstance().GetPath(19).Points;
                    SetupNavigate(p);
                    SimulatorService.GetInstance().StartSimulating(TripManager.Points);
                    break;
                case 3:
                    p = DatabaseService.GetInstance().GetPath(15).Points;
                    SetupNavigate(p);
                    SimulatorService.GetInstance().StartSimulating(TripManager.Points);
                    break;
                default:
                    break;
            }
        }

        private void OnPlanTripButtonClicked(object sender, EventArgs e)
        {
            ((MainActivity)Activity).HandleNavigation(PlanFragment.NewInstance(_selectedMarkerPoint.Id));
        }

        private void OnCancelTripClicked(object sender, EventArgs e)
        {
            SetupBrowse();
            BrowseUpdate();
        }

        private void OnSimulateButtonClicked(object sender, EventArgs e)
        {
            StartSimulating(3);
        }

        public bool OnMarkerClick(Marker marker)
        {
            try
            {
                var id = int.Parse(marker.Snippet);
                SelectedMarkerPoint = DatabaseService.GetInstance().GetPoint(id);
                marker.ShowInfoWindow();
                return true;
            }
            catch (Exception e)
            {
                LogService.ExceptionLog("Issue in map fragment marker click");
                LogService.ExceptionLog(e.Message);
                throw e;
            }
        }

        public void OnMapClick(LatLng point)
        {
            SelectedMarkerPoint = null;
        }

        public View GetInfoContents(Marker marker)
        {
            var view = GetLayoutInflater(null).Inflate(Resource.Layout.infowindow_custom_marker, null);
            ((TextView) view.FindViewById(Resource.Id.markerTitle)).Text = _selectedMarkerPoint.Label;
            return view;
        }

        public View GetInfoWindow(Marker marker)
        {
            return null;
        }

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
                _currentTripPolyline.Remove();
                _currentTripPolyline = null;
            }
            if (_currentTripPolyline != null) return;
            _currentTripPolyline = DrawLine(TripManager.Points);
        }

        private void DrawCurrentBrowsePathAndSites(bool reDraw = false)
        {
            //todo: figure out a way to check if the data is the same
            if (MapIsNull) return;
            if (reDraw)
            {
                _browsePolyline?.Remove();
                _browsePolyline = null;
                if (_launchSiteMarkers != null)
                {
                    foreach (var siteMarker in _launchSiteMarkers)
                    {
                        siteMarker.Remove();
                    }
                }
                _launchSiteMarkers = new List<Marker>();
            }
            if (_browsePolyline != null && _launchSiteMarkers != null) return;
            //todo: check if user has moved camera
            var closestRiverId = DatabaseService.GetInstance().GetClosestRiverId();
            if (closestRiverId >= 0)
            {
                _browsePolyline = DrawLine(DatabaseService.GetInstance().GetPath(closestRiverId));
                var sites = DatabaseService.GetInstance().Points.Where(a => a.IsLaunchSite && a.RiverId == closestRiverId).ToList();
                foreach (var site in sites)
                {
                    _launchSiteMarkers.Add(DrawMarker(site));
                }
            }
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

        private void DrawCurrent()
        {
            try
            {
                if (MapIsNull) return;
                var position = CurrentLocation.ToLatLng();
                if (_currentMarker != null)
                {
                    _currentMarker.Position = position;
                }
                else
                {
                    _currentMarker = MyMap.AddMarker(CurrentMarkerOptions.SetPosition(position));
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
            if (_currentDestinationMarker != null)
            {
                _currentDestinationMarker.Position = position;
            }
            else
            {
                _currentDestinationMarker = MyMap.AddMarker(new MarkerOptions().SetPosition(position));
            }
        }

        public void NavigateCamera()
        {
            if (MapIsNull) return;
            //trying to avoid setting bearing when points are too close to accurately calulate it
            var dist = PBMath.DistanceInMeters(CurrentLocation, TripManager.CurrentPoint);
            if (dist < SysPrefs.BearingTooCloseThreshold)
                return;
            var bearing = PBMath.BearingBetweenPoints(CurrentLocation, TripManager.CurrentPoint);
            var camTarget = PBMath.PointAtDistanceAlongBearing(CurrentLocation, SysPrefs.MetersAheadToAim, bearing);
            var camPos = CameraUpdateBuilder(camTarget, NAV_TILT, NAV_ZOOM, bearing);
            MyMap.AnimateCamera(camPos);
        }
        
        private void MoveCameraZoom(bool animate = false, Point p = null, int zoom = int.MaxValue)
        {
            if (MapIsNull) return;
            if (p == null) p = CurrentLocation;
            if (zoom == int.MaxValue) zoom = BROWSE_ZOOM;

            var camPos = CameraUpdateBuilder(p, BROWSE_TILT, zoom, 0);
            if (animate)
            {
                MyMap.AnimateCamera(camPos);
            }
            else
            {
                MyMap.MoveCamera(camPos);
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

        private CameraUpdate CameraUpdateBuilder(Point p, int tilt = int.MaxValue, int zoom = 0, double bearing = double.NaN)
        {
            var camPos = new CameraPosition.Builder(MyMap.CameraPosition).Target(new LatLng(p.Lat, p.Lng));
            if (tilt != int.MaxValue) camPos.Tilt(tilt);
            if (zoom != 0) camPos.Zoom(zoom);
            if (!bearing.Equals(double.NaN)) camPos.Bearing((float)bearing);
            return CameraUpdateFactory.NewCameraPosition(camPos.Build());
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

        private bool MapIsNull => MyMap == null;

        public enum MapModes
        {
            InitFromPlan,
            Browse,
            Navigate
        }
    }
}