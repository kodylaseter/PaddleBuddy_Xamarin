using System.Collections.Generic;
using System.Linq;
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

        private const double SIM_INC = 20; //how much to increment each sim jump (meters)
        private const int TIME_DELAY = 1000; //time to delay between sim jumps

        public static void StartSimulating(List<Point> points)
        {
            LogService.Log("Started simulating");
            LS.StopListening();
            Task.Run(() => Simulate(points));
        }

        public static async void Simulate(List<Point> points)
        {
            SetCurrent(points.First());
            foreach (var point in points)
            {
                while (PBUtilities.DistanceInMeters(LS.CurrentLocation, point) > SysPrefs.TripPointsCloseThreshold)
                {
                    var newPoint = PBUtilities.PointBetween(LS.CurrentLocation, point, 0.5);
                    SetCurrent(newPoint);
                    await Task.Delay(TIME_DELAY);
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