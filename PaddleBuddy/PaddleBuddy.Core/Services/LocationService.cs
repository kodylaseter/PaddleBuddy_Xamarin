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
                var tempLocation = value;
                var time = DateTime.Now.TimeOfDay.TotalSeconds;
                tempLocation.Time = time;
                if (_currentLocation != null && _currentLocation.Time > 0 && time > 0)
                {
                    var speed = PBMath.DistanceInMeters(tempLocation, CurrentLocation) / (tempLocation.Time - CurrentLocation.Time);
                    if (speed > 0) //todo: filter out bad speeds
                    {
                        tempLocation.Speed = speed;
                    }
                }
                _currentLocation = tempLocation;
                MessengerService.Messenger.Send(new LocationUpdatedMessage());
            }
        }

        public void StartListening()
        {
            if (Geolocator.IsListening) return;
            Log("start listening");
            //parameters are milliseconds, meters
            Geolocator.StartListeningAsync(2000, 5, true);
            Geolocator.PositionChanged += OnPositionChanged;
        }

        public void StopListening()
        {
            Log("stop listening");
            Geolocator.StopListeningAsync();
        }

        private void OnPositionChanged(object sender, object eventArgs)
        {
            Log("location updated");
            if (eventArgs.GetType() == typeof(PositionEventArgs))
            {
                var args = (PositionEventArgs)eventArgs;
                CurrentLocation = new Point
                {
                    Lat = args.Position.Latitude,
                    Lng = args.Position.Longitude
                };
            }
            else if (eventArgs.GetType() == typeof(Point))
            {
                //point = (Point)eventArgs;
                throw new NotImplementedException();
            }
            else
            {
                Log("OnPositionChanged error");
            }
        }

        public void SetupLocation()
        {
            Log("Setting up location service");
            GetInstance().StartListening();
            //GetInstance().GetLocationAsync();
        }

        private void Log(string msg)
        {
            LogService.TagAndLog("LOCSERV", msg);
        }
    }
}