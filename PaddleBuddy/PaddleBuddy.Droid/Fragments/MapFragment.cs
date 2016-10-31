using System;
using Android.App;
using Android.Gms.Maps;
using Android.OS;
using Android.Views;
using PaddleBuddy.Core.Services;

namespace PaddleBuddy.Droid.Fragments
{
    public class MapFragment : BaseFragment, IOnMapReadyCallback
    {
        private GoogleMap _googleMap;
        private MapView _mapView;
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.fragment_map, container, false);
            var rootView = view.RootView;
            _mapView = (MapView) rootView.FindViewById(Resource.Id.map_view);
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

        public static MapFragment NewInstance()
        {
            return new MapFragment();
        }

        public void OnMapReady(GoogleMap googleMap)
        {
            _googleMap = googleMap;
        }
    }
}