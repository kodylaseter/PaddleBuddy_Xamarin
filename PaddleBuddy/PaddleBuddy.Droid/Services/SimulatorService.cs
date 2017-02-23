using System;
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

        private const double SIM_INC = 8; //how many increments per map section
        /// <summary>
        /// 
        /// </summary>
        private const int TIME_DELAY = 1100; //
        private const double FRACTION_OFF_TRACK = 0.3;

        public static bool StopSimulating { get; set; }
        private static Random Random { get; set; }

        public static void StartSimulating(List<Point> points)
        {
            LogService.Log("Started simulating");
            LS.StopListening();
            StopSimulating = false;
            Random = new Random();
            Task.Run(() => Simulate(points, FRACTION_OFF_TRACK));
        }

        public static async void Simulate(List<Point> points, double fractionTestOffTrack)
        {
            if (StopSimulating) return;
            LS.StopListening();
            SetCurrent(points.First());
            int rand;
            var tempPoints = points;
            await Task.Delay(TIME_DELAY);
            foreach (var point in tempPoints)
            {
                if (StopSimulating) return;
                var inc = PBUtilities.DistanceInMeters(LS.CurrentLocation, point)/SIM_INC;
                while (PBUtilities.DistanceInMeters(LS.CurrentLocation, point) > SysPrefs.TripPointsCloseThreshold)
                {
                    if (StopSimulating) return;
                    var bearing = PBUtilities.BearingBetweenPoints(LS.CurrentLocation, point);
                    var newPoint = PBUtilities.PointAtDistanceAlongBearing(LS.CurrentLocation, inc, bearing);
                    SetCurrent(newPoint);
                    await Task.Delay(TIME_DELAY);
                }
                SetCurrent(point);
                await Task.Delay(TIME_DELAY);
                rand = Random.Next(1, 11);
                if (rand <= (int)(fractionTestOffTrack * 10))
                {
                    await Task.Run(TestOffTrack);
                }
            }
            LogService.Log("Finished simulating");
            LS.StartListening();
        }

        private static async Task TestOffTrack()
        {
            var numberOfOffTrackIncrements = Random.Next(1, 10);
            var offTrackIncs = 0;
            var bearing = Random.Next(0, 360);
            while (!StopSimulating)
            {
                SetCurrent(PBUtilities.PointAtDistanceAlongBearing(LS.CurrentLocation, 60, bearing));
                await Task.Delay(TIME_DELAY);
                offTrackIncs++;
                if (offTrackIncs >= numberOfOffTrackIncrements) return;
            }

        }

        private static void SetCurrent(Point p)
        {
            if (p != null)
            {
                CrossCurrentActivity.Current.Activity.RunOnUiThread(() => LS.CurrentLocation = p);
            }
        }

        private static LocationService LS => LocationService.GetInstance();
    }
}