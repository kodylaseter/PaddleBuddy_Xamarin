using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using PaddleBuddy.Core.Models.Map;
using PaddleBuddy.Core.Models.Messages;
using PaddleBuddy.Core.Utilities;
using Plugin.Connectivity;
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
    }
}