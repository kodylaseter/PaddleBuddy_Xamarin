using System.Collections.Generic;
using System.Threading.Tasks;
using PaddleBuddy.Core;
using PaddleBuddy.Core.Models.Map;
using PaddleBuddy.Core.Services;
using PaddleBuddy.Core.Utilities;
using Plugin.CurrentActivity;

namespace PaddleBuddy.Droid.Services
{
    public class SimulatorService
    {
        public static void StartSimulating(List<Point> points)
        {
            LogService.Log("Started simulating");
            LS.StopListening();
            Task.Run(() => Simulate(points));
        }

        public static async void Simulate(List<Point> points)
        {
            foreach (var point in points)
            {
                while (PBUtilities.DistanceInMeters(LS.CurrentLocation, point) > SysPrefs.TripPointsCloseThreshold)
                {
                    var newPoint = PBUtilities.PointBetween(LS.CurrentLocation, point, 0.7);
                    SetCurrent(newPoint);
                    await Task.Delay(1000);
                }
                SetCurrent(point);
            }
            LogService.Log("Finished simulating");
        }

        private static void SetCurrent(Point p)
        {
            CrossCurrentActivity.Current.Activity.RunOnUiThread(() => LS.CurrentLocation = p);
        }

        private static LocationService LS => LocationService.GetInstance();
    }
}