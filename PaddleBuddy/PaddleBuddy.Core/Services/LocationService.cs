using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using PaddleBuddy.Core.Models.Map;
using PaddleBuddy.Core.Models.Messages;
using PaddleBuddy.Core.Utilities;
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

        public static void SetupLocation()
        {
            LogService.Log("Setting up location service");
            GetInstance().StartListening();
            GetInstance().GetLocationAsync();
        }

        public void StartSimulating(List<Point> points)
        {
            StopListening();
            Task.Run(() => Simulate(points));
        }

        public async void Simulate(List<Point> points)
        {
            foreach (var point in points)
            {
                while (PBUtilities.DistanceInMeters(_currentLocation, point) > 20)
                {
                    CurrentLocation = PBUtilities.PointBetween(_currentLocation, point, 0.1);
                    await Task.Delay(500);
                }
                CurrentLocation = point;
            }
        }

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