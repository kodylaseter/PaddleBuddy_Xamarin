using System;
using System.Collections.Generic;
using System.Linq;
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
using Path = PaddleBuddy.Core.Models.Map.Path;
using Point = PaddleBuddy.Core.Models.Map.Point;

namespace PaddleBuddy.Droid.Fragments
{
    public class MapFragment : BaseFragment, IOnMapReadyCallback, GoogleMap.IOnMarkerClickListener, GoogleMap.IOnMapClickListener, GoogleMap.IInfoWindowAdapter
    {
        private GoogleMap _myMap;
        private MapView _mapView;
        private MapModes _mapMode;
        private Point _selectedMarkerPoint;
        private Button _planTripButton;
        private LinearLayout _mapBarLayout;
        private TextView _mapBarTextView1;
        private MarkerOptions _currentMarkerOptions;
        private Marker _currentMarker;
        private Marker _currentDestinationMarker;
        private Polyline _currentTripPolyline;


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

        public bool IsLoading { get; set; }
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
                    LogService.Log("Problem initializing map");
                    LogService.Log(e.Message);
                }
                _mapView.GetMapAsync(this);
            }
            view.FindViewById<Button>(Resource.Id.plan_trip_button).Click += OnPlanTripButtonClicked;
            view.FindViewById<Button>(Resource.Id.test_simulate_button).Click += OnSimulateButtonClicked;
            view.FindViewById<Button>(Resource.Id.cancel_trip_button).Click += OnCancelTripClicked;
            _speedTextView = view.FindViewById<TextView>(Resource.Id.speed_textview);
            _mapBarLayout = view.FindViewById<LinearLayout>(Resource.Id.mapbar_layout);
            _mapBarTextView1 = view.FindViewById<TextView>(Resource.Id.mapbar_text1);
            MapMode = MapModes.Browse;
            _speeds = new List<Tuple<double, double>>();
            return view;
        }

        public void OnMapReady(GoogleMap googleMap)
        {
            MyMap = googleMap;
            MessengerService.Messenger.Register<LocationUpdatedMessage>(this, LocationUpdatedReceived);
            MyMap.SetOnMarkerClickListener(this);
            MyMap.SetOnMapClickListener(this);
            MyMap.SetInfoWindowAdapter(this);
            Setup();
        }


        private MapModes MapMode
        {
            get { return _mapMode; }
            set
            {
                _mapMode = value;
                switch (_mapMode)
                {
                    case MapModes.Browse:
                        HideMapBar();
                        break;
                    case MapModes.Navigate:
                        UpdateMapBar("");
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        private void LocationUpdatedReceived(LocationUpdatedMessage obj)
        {
            switch (MapMode)
            {
                case MapModes.Browse:
                    DrawCurrent();
                    break;
                case MapModes.Navigate:
                    DrawCurrent();
                    NavigationUpdate();
                    break;
                default: 
                    throw new NotImplementedException();
            }
        }

        private void NavigationUpdate()
        {
            if (TripManager == null || TripManager.Points == null || TripManager.Points.Count < 1)
            {
                LogService.Log("No navigation update. tripData not configured correctly");
                return;
            }
            DrawCurrentTrip();
            if (TripManager.HasStarted)
            {
                if (TripManager.IsOnTrack(TripManager.PreviousPoint, TripManager.NextPoint, CurrentLocation))
                {
                    DrawCurrentDestination(TripManager.NextPoint);
                    NavigateCamera();
                    UpdateMapBar(TripManager.NextPoint.Id.ToString());
                    if (TripManager.CloseToNext(CurrentLocation))
                    {
                        if (TripManager.HasNext)
                        {
                            TripManager.Increment();
                        }
                        else
                        {
                            LogService.Log("Finished trip!");
                            MapMode = MapModes.Browse;
                        }
                    }
                    UpdateSpeed();
                }
                else
                {
                    HideSpeed();
                    var newDestination = DatabaseService.GetInstance().PickNextDestination(CurrentLocation, TripManager);
                    AnimateCameraBounds(new[] {CurrentLocation, newDestination});
                    TripManager.UpdateForNewDestination(newDestination);
                    double distance;
                    if (TripManager.HasPrevious)
                    {
                        distance =
                            PBUtilities.DistanceInMetersFromPointToLineSegment(TripManager.PreviousPoint,
                                TripManager.NextPoint,
                                CurrentLocation);
                    }
                    else
                    {
                        distance = PBUtilities.DistanceInMeters(CurrentLocation, TripManager.NextPoint);
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
                        LogService.Log("Finished trip!");
                        MapMode = MapModes.Browse;
                    }
                }
                else
                {
                    HideSpeed();
                    DrawCurrentDestination(TripManager.NextPoint);
                    AnimateCameraBounds(new[] { CurrentLocation, TripManager.NextPoint });
                    UpdateMapBar("Navigate to river");
                }
            }
            
        }

        private void Setup()
        {
            IsLoading = true;
            DrawCurrent();
            _currentMarker = null;
            _selectedMarkerPoint = null;
            switch (MapMode)
            {
                case MapModes.Browse:
                    SetupBrowse();
                    break;
                case MapModes.InitFromPlan:
                    SetupInitFromPlan();
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
            IsLoading = false;
        }

        private void SetupBrowse()
        {
            HideSpeed();
            ClearTripData();
            try
            {
                MoveCameraZoom(animate: true);
                var path = DatabaseService.GetInstance().GetClosestRiver();
                if (path.Points != null && path.Points.Count > 1)
                {
                    DrawLine(path);
                    var launchSites = from p in DatabaseService.GetInstance().Points
                        where p.RiverId == DatabaseService.GetInstance().ClosestRiverId && p.IsLaunchSite
                        select p;
                    foreach (var site in launchSites)
                    {
                        DrawMarker(site);
                    }
                }
            }
            catch (Exception e)
            {
                LogService.Log("Error in browse setup");
                LogService.Log(e.Message);
            }
            MapMode = MapModes.Browse;
        }

        private void SetupInitFromPlan()
        {
            
        }

        private void SetupTripData(List<Point> points)
        {
            TripManager = new TripManager {Points = points};
        }

        private void ClearTripData()
        {
            TripManager = null;
            _currentTripPolyline = null;
            _currentDestinationMarker = null;
            
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
                    SetupTripData(p);
                    SimulatorService.StartSimulating(TripManager.Points);
                    break;
                case 1: //chat test 7-42
                    p = DatabaseService.GetInstance().GetPath(2).Points;
                    SetupTripData(p);
                    SimulatorService.StartSimulating(TripManager.Points);
                    break;
                default:
                    break;
            }
            MapMode = MapModes.Navigate;
        }


        private void UpdateMapBar(string text1)
        {
            ShowMapBar();
            _mapBarTextView1.Text = text1;
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
                _planTripButton.Visibility = _selectedMarkerPoint != null ? ViewStates.Visible : ViewStates.Gone;
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

        private void OnPlanTripButtonClicked(object sender, EventArgs e)
        {
            ((MainActivity)Activity).HandleNavigation(null, PlanFragment.NewInstance(_selectedMarkerPoint.Id));
        }

        private void OnCancelTripClicked(object sender, EventArgs e)
        {
            SetupBrowse();
        }

        private void OnSimulateButtonClicked(object sender, EventArgs e)
        {
            StartSimulating(1);
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
                LogService.Log("Issue in map fragment marker click");
                LogService.Log(e.Message);
            }
            return false;
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

        private void DrawMarker(Point p)
        {
            if (MapIsNull) return;
            var marker = new MarkerOptions().SetPosition(AndroidUtils.PointToLatLng(p));
            if (p.IsLaunchSite) marker.SetTitle(p.Label).SetSnippet(p.Id.ToString());
            MyMap.AddMarker(marker);
        }

        private void DrawCurrentTrip()
        {
            if (MapIsNull) return;
            if (_currentTripPolyline == null) return;
            _currentTripPolyline = DrawLine(TripManager.Points);
        }

        private Polyline DrawLine(List<Point> points)
        {
            if (MapIsNull) return null;
            var polyOpts = new PolylineOptions()
                .InvokeColor(Resource.Color.black)
                .InvokeWidth(9)
                .InvokeZIndex(1);
            polyOpts.Add(AndroidUtils.PointsToLatLngs(points));
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
                var position = AndroidUtils.PointToLatLng(CurrentLocation);
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
                LogService.Log(e.Message);
            }
        }

        private void DrawCurrentDestination(Point p)
        {
            if (MapIsNull) return;
            var position = AndroidUtils.PointToLatLng(p);
            if (_currentDestinationMarker != null)
            {
                _currentDestinationMarker.Position = position;
            }
            else
            {
                _currentDestinationMarker = MyMap.AddMarker(new MarkerOptions().SetPosition(position));
            }
        }

        private void SetTilt()
        {
            var camPos = new CameraPosition.Builder(MyMap.CameraPosition).Tilt(NAV_TILT).Build();
            MyMap.MoveCamera(CameraUpdateFactory.NewCameraPosition(camPos));
        }

        private void RemoveTilt()
        {
            var camPos = new CameraPosition.Builder(MyMap.CameraPosition).Tilt(BROWSE_TILT).Build();
            MyMap.MoveCamera(CameraUpdateFactory.NewCameraPosition(camPos));
        }

        public void NavigateCamera()
        {
            if (MapIsNull) return;
            //trying to avoid setting bearing when points are too close to accurately calulate it
            var bearing = float.NaN;
            if (PBUtilities.DistanceInMeters(CurrentLocation, TripManager.NextPoint) > SysPrefs.TripPointsCloseThreshold)
            {
               bearing = PBUtilities.BearingBetweenPoints(CurrentLocation, TripManager.NextPoint);
            }
            var camPos = CameraUpdateBuilder(CurrentLocation, NAV_TILT, NAV_ZOOM, bearing);
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

        private void AnimateCameraBounds(Point[] points)
        {
            if (MapIsNull) return;
            var builder = new LatLngBounds.Builder();
            foreach (var p in points)
            {
                builder.Include(AndroidUtils.PointToLatLng(p));
            }
            var bounds = builder.Build();
            var cameraUpdate = CameraUpdateFactory.NewLatLngBounds(bounds, 80);
            MyMap.AnimateCamera(cameraUpdate);
        }

        private CameraUpdate CameraUpdateBuilder(Point p, int tilt = int.MaxValue, int zoom = 0, float bearing = float.NaN)
        {
            var camPos = new CameraPosition.Builder(MyMap.CameraPosition).Target(new LatLng(p.Lat, p.Lng));
            if (tilt != int.MaxValue) camPos.Tilt(tilt);
            if (zoom != 0) camPos.Zoom(zoom);
            if (!bearing.Equals(float.NaN)) camPos.Bearing(bearing);
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