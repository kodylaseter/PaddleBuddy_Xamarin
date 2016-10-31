using System;
using System.Linq;
using Android.App;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Content.Res;
using Android.Views;
using PaddleBuddy.Core.Services;
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
        private MarkerOptions _currentMarkerOptions;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            MapMode = MapModes.Init;
            var view = inflater.Inflate(Resource.Layout.fragment_map, container, false);
            var rootView = view.RootView;
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
            return view;
        }

        public void OnMapReady(GoogleMap googleMap)
        {
            GoogleMap = googleMap;
            GoogleMap.SetOnMarkerClickListener(this);
            GoogleMap.SetOnMapClickListener(this);
            GoogleMap.SetInfoWindowAdapter(this);
            Setup();
        }

        private void Setup()
        {
            IsLoading = true;
            _currentMarker = null;
            switch (MapMode)
            {
                case MapModes.Init:
                    SetupInit();
                    break;
                case MapModes.InitFromPlan:
                    SetupInitFromPlan();
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
            IsLoading = false;
        }

        private void SetupInit()
        {
            MoveCameraZoom(GetCurrentLocation, 8);
            try
            {
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
                LogService.Log("Error in setup init");
                LogService.Log(e.Message);
            }
        }

        private void SetupInitFromPlan()
        {
            
        }

        private GoogleMap GoogleMap
        {
            get { return _googleMap; }
            set { _googleMap = value; }
        }

        private Point GetCurrentLocation
        {
            get { return LocationService.GetInstance().CurrentLocation; }
        }


        public static MapFragment NewInstance()
        {
            return new MapFragment();
        }


        public bool OnMarkerClick(Marker marker)
        {
            throw new NotImplementedException();
        }

        public void OnMapClick(LatLng point)
        {
            throw new NotImplementedException();
        }

        public View GetInfoContents(Marker marker)
        {
            throw new NotImplementedException();
        }

        public View GetInfoWindow(Marker marker)
        {
            throw new NotImplementedException();
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
            if (MapIsNull) return;
            var position = AndroidUtils.ToLatLng(GetCurrentLocation);
            if (_currentMarker != null)
            {
                _currentMarker.Position = position;
            }
            else
            {
                _currentMarker = GoogleMap.AddMarker(CurrentMarkerOptions.SetPosition(position));
            }
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
            Init,
            InitFromPlan
        }
    }
}