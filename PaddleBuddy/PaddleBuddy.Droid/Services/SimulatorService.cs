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

        private const double SIM_INC = 10; //how many increments per map section
        private const int TIME_DELAY = 1000; //time to delay between sim jumps

        public static void StartSimulating(List<Point> points)
        {
            LogService.Log("Started simulating");
            LS.StopListening();
            Task.Run(() => Simulate(points));
        }

        public static async void Simulate(List<Point> points)
        {
            LS.StopListening();
            SetCurrent(points.First());
            await Task.Delay(TIME_DELAY);
            foreach (var point in points)
            {
                var inc = PBUtilities.DistanceInMeters(LS.CurrentLocation, point)/SIM_INC;
                while (PBUtilities.DistanceInMeters(LS.CurrentLocation, point) > SysPrefs.TripPointsCloseThreshold)
                {
                    //var newPoint = PBUtilities.PointBetween(LS.CurrentLocation, point, 0.7);
                    var bearing = PBUtilities.BearingBetweenPoints(LS.CurrentLocation, point);
                    var newPoint = PBUtilities.PointAtDistanceAlongBearing(LS.CurrentLocation, inc, bearing);
                    SetCurrent(newPoint);
                    await Task.Delay(TIME_DELAY);
                }
                SetCurrent(point);
                await Task.Delay(TIME_DELAY);
            }
            LogService.Log("Finished simulating");
            LS.StartListening();
        }

        private static void SetCurrent(Point p)
        {
            CrossCurrentActivity.Current.Activity.RunOnUiThread(() => LS.CurrentLocation = p);
        }

        private static LocationService LS => LocationService.GetInstance();
    }
}