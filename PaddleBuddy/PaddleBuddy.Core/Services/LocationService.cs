using System;
using System.Threading.Tasks;
using PaddleBuddy.Core.Models.Map;
using PaddleBuddy.Core.Models.Messages;
using PaddleBuddy.Core.Utilities;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using UnitsNet.Extensions.NumberToDuration;

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
                var time = DateTime.Now.TimeOfDay.TotalSeconds.Seconds();
                tempLocation.Time = time;
                if (_currentLocation != null && _currentLocation.Time.Seconds > 0 && time.Seconds > 0)
                {
                    var speed = PBMath.Distance(tempLocation, CurrentLocation) / (tempLocation.Time - CurrentLocation.Time);
                    if (speed.MetersPerSecond > 0) //todo: filter out bad speeds
                    {
                        tempLocation.Speed = speed;
                    }
                }
                _currentLocation = tempLocation;

                MessengerService.Messenger.Send(new LocationUpdatedMessage());
                Log("Location Updated");
            }
        }

        public void StartListening()
        {
            if (Geolocator.IsListening) return;
            Log("start listening");
            //parameters are milliseconds, meters
            Geolocator.StartListeningAsync(2000, 5, true);
        }

        public void StopListening()
        {
            Log("stop listening");
            Geolocator.StopListeningAsync();
        }

        private void OnPositionChanged(object sender, object eventArgs)
        {
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

        private void OnPositionError(object sender, PositionErrorEventArgs e)
        {
            Log(e.ToString());
        }

        public void SetupLocation()
        {
            Log("Setting up location service");
            Geolocator.PositionError += OnPositionError;
            Geolocator.PositionChanged += OnPositionChanged;
            GetInstance().StartListening();
            GetInstance().GetLocationAsync();
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

        private void Log(string msg)
        {
            LogService.TagAndLog("LOCSERV", msg);
        }
    }
}