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
        private Marker _currentMarker;
        private GoogleMap _googleMap;
        private MapView _mapView;
        private MapModes MapMode { get; set; }
        public bool IsLoading { get; set; }
        public TripData TripData { get; set; }
        private Point _selectedMarkerPoint;
        private Button _planTripButton;
        private LinearLayout _subBarLayout;
        private TextView _subBarTextView1;
        private TextView _subBarTextView2;

        private MarkerOptions _currentMarkerOptions;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            MapMode = MapModes.Browse;
            var view = inflater.Inflate(Resource.Layout.fragment_map, container, false);
            _planTripButton = (Button)view.FindViewById(Resource.Id.plan_trip_button);
            _planTripButton.Click += OnPlanTripButtonClicked;
            ((Button) view.FindViewById(Resource.Id.test_simulate_button)).Click += OnSimulateButtonClicked;
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
            _subBarLayout = (LinearLayout)Activity.FindViewById(Resource.Id.subbar_layout);
            _subBarTextView1 = (TextView)Activity.FindViewById(Resource.Id.subbar_text1);
            _subBarTextView2 = (TextView)Activity.FindViewById(Resource.Id.subbar_text2);
            return view;
        }

        public void OnMapReady(GoogleMap googleMap)
        {
            GoogleMap = googleMap;
            MessengerService.Messenger.Register<LocationUpdatedMessage>(this, LocationUpdatedReceived);
            GoogleMap.SetOnMarkerClickListener(this);
            GoogleMap.SetOnMapClickListener(this);
            GoogleMap.SetInfoWindowAdapter(this);
            Setup();
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
            if (TripData == null || TripData.Points == null || TripData.Points.Count < 1)
            {
                LogService.Log("No navigation update. tripData not configured correctly");
                return;
            }
            if (!TripData.HasStarted)
            {
                AnimateCameraBounds(new[] {CurrentLocation, TripData.NextPoint});
                if (!TripData.CloseToStart(CurrentLocation))
                {
                    UpdateSubBar(ViewStates.Visible, "Navigate to starting point",
                        "Distance remaining " + TripData.DistanceToNext(CurrentLocation) + " meters");
                }
                else
                {
                    //todo check if hasnext
                    TripData.Increment();
                    HideSubBar();
                }
            }
            else
            { 
                NavigateCamera();
                if (TripData.CloseToNext(CurrentLocation))
                {
                    if (TripData.HasNext)
                    {
                        TripData.Increment();
                    }
                    else
                    {
                        LogService.Log("Finished trip!");
                    }
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
            try
            {
                MoveCameraZoomToCurrent();
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
            TripData = new TripData {Points = points};
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
                    SimulatorService.StartSimulating(TripData.Points);
                    break;
                case 1: //chat test 7-42
                    p = DatabaseService.GetInstance().GetPath(2).Points;
                    SetupTripData(p);
                    SimulatorService.StartSimulating(TripData.Points);
                    break;
                default:
                    break;
            }
            MapMode = MapModes.Navigate;
        }


        private void UpdateSubBar(ViewStates state, string text1, string text2)
        {
            ShowSubBar();
            _subBarTextView1.Text = text1;
            _subBarTextView2.Text = text2;
        }

        private void HideSubBar()
        {
            _subBarLayout.Visibility = ViewStates.Gone;
        }

        private void ShowSubBar()
        {
            _subBarLayout.Visibility = ViewStates.Visible;
        }

        private GoogleMap GoogleMap
        {
            get { return _googleMap; }
            set { _googleMap = value; }
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
            var marker = new MarkerOptions().SetPosition(new LatLng(p.Lat, p.Lng));
            if (p.IsLaunchSite) marker.SetTitle(p.Label).SetSnippet(p.Id.ToString());
            GoogleMap.AddMarker(marker);
        }

        private void DrawLine(Point[] points)
        {
            if (MapIsNull) return;
            var polyOpts = new PolylineOptions()
                .InvokeColor(Resource.Color.black)
                .InvokeWidth(9)
                .InvokeZIndex(1);
            foreach (var p in points)
            {
                polyOpts.Add(AndroidUtils.ToLatLng(p));
            }
            GoogleMap.AddPolyline(polyOpts);
        }

        private void DrawLine(Path path)
        {
            if (MapIsNull || path == null || path.Points.Count < 2) return;
            DrawLine(path.Points.ToArray());
        }

        private void DrawLine(Point start, Point end)
        {
            var path = DatabaseService.GetInstance().GetPath(start, end);
            DrawLine(path);
        }

        private void DrawCurrent()
        {
            try
            {
                if (MapIsNull) return;
                var position = AndroidUtils.ToLatLng(CurrentLocation);
                if (_currentMarker != null)
                {
                    _currentMarker.Position = position;
                }
                else
                {
                    _currentMarker = GoogleMap.AddMarker(CurrentMarkerOptions.SetPosition(position));
                }
            }
            catch (Exception e)
            {
                LogService.Log(e.Message);
            }
        }

        public void MoveCameraZoomToCurrent()
        {
            MoveCameraZoom(CurrentLocation, 8);
        }

        public void NavigateCamera()
        {
            MoveCameraZoom(CurrentLocation, 17);
        }

        public void MoveCamera(Point p)
        {
            if (MapIsNull) return;
            GoogleMap.MoveCamera(CameraUpdateFactory.NewLatLng(new LatLng(p.Lat, p.Lng)));
        }

        //todo: combine these methods into 1 camera method
        public void MoveCameraZoom(Point p, int zoom)
        {
            if (MapIsNull) return;
            GoogleMap.MoveCamera(CameraUpdateFactory.NewLatLngZoom(new LatLng(p.Lat, p.Lng), zoom));
        }

        public void AnimateCameraBounds(Point[] points)
        {
            if (MapIsNull) return;
            var builder = new LatLngBounds.Builder();
            foreach (var p in points)
            {
                builder.Include(new LatLng(p.Lat, p.Lng));
            }
            var bounds = builder.Build();
            var cameraUpdate = CameraUpdateFactory.NewLatLngBounds(bounds, 80);
            GoogleMap.AnimateCamera(cameraUpdate);
        }

        public MarkerOptions CurrentMarkerOptions
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

        private bool MapIsNull => GoogleMap == null;

        public enum MapModes
        {
            InitFromPlan,
            Browse,
            Navigate
        }
    }
}