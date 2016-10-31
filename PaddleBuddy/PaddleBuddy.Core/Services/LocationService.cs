using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using PaddleBuddy.Core.Models.Map;
using PaddleBuddy.Core.Models.Messages;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;

namespace PaddleBuddy.Core.Services
{
    public class LocationService
    {
        private static LocationService _locationService;
        private Point _currentLocation;
        public IGeolocator Geolocator => CrossGeolocator.Current;

        public LocationService()
        {
            Geolocator.DesiredAccuracy = 5;
        }


        public static LocationService GetInstance()
        {
            return _locationService ?? (_locationService = new LocationService());
        }

        public Point CurrentLocation
        {
            get { return _currentLocation; }
            set
            {
                _currentLocation = value;
                MessengerService.Messenger.Send(new LocationUpdatedMessage());
            }
        }

        public async Task<Point> GetLocationAsync()
        {
            var position = await Geolocator.GetPositionAsync();
            var point = new Point
            {
                Lat = position.Latitude,
                Lng = position.Longitude
            };
            CurrentLocation = point;
            return point;
        }

        public void StartListening()
        {
            if (Geolocator.IsListening) return;
            Geolocator.StartListeningAsync(5, 5, true);
            Geolocator.PositionChanged += OnPositionChanged;
        }

        public void StopListening()
        {
            Geolocator.StopListeningAsync();
        }

        private void OnPositionChanged(object sender, object eventArgs)
        {
            LogService.Log("location updated");
            var point = new Point();
            if (eventArgs.GetType() == typeof(PositionEventArgs))
            {
                var args = (PositionEventArgs)eventArgs;
                point = new Point
                {
                    Lat = args.Position.Latitude,
                    Lng = args.Position.Longitude
                };
                CurrentLocation = point;
            }
            else if (eventArgs.GetType() == typeof(Point))
            {
                //point = (Point)eventArgs;
                throw new NotImplementedException();
            }
            else
            {
                LogService.Log("OnPositionChanged error");
            }
        }

        public static async void SetupLocation()
        {
            LogService.Log("Setting up location service");
            GetInstance().StartListening();
            GetInstance().GetLocationAsync();
        }

        public void StartSimulating(List<Point> points)
        {
            //this
        }

        //public async void Simulate()
        //{

        //    var curr = new Point();
        //    while (true)
        //    {
        //        await Task.Delay(500);
        //        if (ViewModel.StartPoint != null)
        //        {
        //            curr.Lat = ViewModel.CurrentLocation.Lat;
        //            curr.Lng = ViewModel.CurrentLocation.Lng + (-84.1180229 - ViewModel.CurrentLocation.Lng) / 2;
        //            //Need to set current location
        //            //Need to refactor this
        //        }
        //    }
        //}

        //using this simulate, plan from tretret to qwertyuiop
        //if (CurrentLocation == null)
        //{
        //    CurrentLocation = new Point
        //    {
        //        Lat = 34.065676,
        //        Lng = -84.272612
        //    };
        //}
    }
}