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

        private bool _stop;
        private Random Random { get; set; }

        private static SimulatorService _simulatorService;

        public static SimulatorService GetInstance()
        {
            return _simulatorService ?? (_simulatorService = new SimulatorService());
        }

        private SimulatorService()
        {
            Random = new Random();
            _stop = false;
        }

        public void StartSimulating(List<Point> points)
        {
            Log("Started simulating");
            LS.StopListening();
            _stop = false;
            Task.Run(() => Simulate(points, FRACTION_OFF_TRACK));
        }

        public void StopSimulating()
        {
            _stop = true;
            Log("stop simulating");
            LS.StartListening();
        }

        private async Task Simulate(List<Point> points, double fractionTestOffTrack)
        {
            try
            {
                if (_stop)
                {
                    StopSimulating();
                    return;
                }
                LS.StopListening();
                SetCurrent(points.First());
                int rand;
                await Task.Delay(TIME_DELAY);
                foreach (var point in points.ToList())
                {
                    Log($"Simulating to {point.Id}");
                    if (_stop)
                    {
                        StopSimulating();
                        return;
                    }
                    var inc = PBUtilities.DistanceInMeters(LS.CurrentLocation, point)/SIM_INC;
                    while (PBUtilities.DistanceInMeters(LS.CurrentLocation, point) > SysPrefs.TripPointsCloseThreshold)
                    {
                        if (_stop)
                        {
                            StopSimulating();
                            return;
                        }
                        var bearing = PBUtilities.BearingBetweenPoints(LS.CurrentLocation, point);
                        var newPoint = PBUtilities.PointAtDistanceAlongBearing(LS.CurrentLocation, inc, bearing);
                        SetCurrent(newPoint);
                        await Task.Delay(TIME_DELAY);
                    }
                    SetCurrent(point);
                    await Task.Delay(TIME_DELAY);
                    rand = Random.Next(1, 11);
                    if (rand <= (int) (fractionTestOffTrack*10))
                    {
                        var b = await TestOffTrack();
                    }
                }
                Log("Finished simulating");
                StopSimulating();
            }
            catch (Exception e)
            {
                Log(e.Message);
                StopSimulating();
            }
        }

        private async Task<bool> TestOffTrack()
        {
            Log("Sim off track");
            var numberOfOffTrackIncrements = Random.Next(1, 10);
            var offTrackIncs = 0;
            var bearing = Random.Next(0, 360);
            while (!_stop)
            {
                SetCurrent(PBUtilities.PointAtDistanceAlongBearing(LS.CurrentLocation, 60, bearing));
                await Task.Delay(TIME_DELAY);
                offTrackIncs++;
                if (offTrackIncs >= numberOfOffTrackIncrements)
                {
                    Log("Sim back on track");
                    return true;
                }
            }
            Log("Sim off track");
            return true;
        }

        private static void SetCurrent(Point p)
        {
            if (p != null)
            {
                CrossCurrentActivity.Current.Activity.RunOnUiThread(() => LS.CurrentLocation = p);
            }
        }

        private static LocationService LS => LocationService.GetInstance();

        private static void Log(string msg)
        {
            LogService.TagAndLog("SIM", msg);
        }
    }
}