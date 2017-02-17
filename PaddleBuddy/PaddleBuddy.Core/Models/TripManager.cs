using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PaddleBuddy.Core.Models.Map;
using PaddleBuddy.Core.Services;
using PaddleBuddy.Core.Utilities;

namespace PaddleBuddy.Core.Models
{


    public class TripManager
    {
        private List<Point> _points;
        private int _index;

        /// <summary>
        /// Used for IsOnTrack calculation for filtering out points
        /// </summary>
        private const double POINT_TOO_FAR_AWAY = 5500;

        public TripManager()
        {
            _index = 0;
        }

        public List<Point> Points
        {
            get { return _points; }
            set { _points = value; }
        }

        public Point StartPoint
        {
            get { return _points.First(); }
        }

        public Point NextPoint
        {
            get { return PointAt(_index); }
        }

        public bool HasStarted
        {
            get { return _index > 0; }
        }


        public void Increment()
        {
            _index++;
        }

        public bool IsOnTrack()
        {
            var current = LocationService.GetInstance().CurrentLocation;
            var tempPoints = _points;
            //list of points and their distances to the currentlocation
            List<Tuple<Point, double>> pointsToCheck = new List<Tuple<Point, double>>();
            //pointsToCheck.RemoveAll(p => PBUtilities.DistanceInMeters(current, p) > POINT_TOO_FAR_AWAY);
            for (int i = _points.Count - 1; i >= 0; i--)
            {
                var dist = PBUtilities.DistanceInMeters(current, _points[i]);
                if (dist > POINT_TOO_FAR_AWAY)
                {
                    tempPoints.RemoveAt(i);
                }
                else
                {
                    pointsToCheck.Add(new Tuple<Point, double>(tempPoints[i], dist));
                }
            }
            pointsToCheck.Sort((x, y) => x.Item2.CompareTo(y.Item2));
            foreach (var point in pointsToCheck)
            {
                var p = point.Item2;
                var next = 
            }
            return true;

        }

        public bool CloseToStart(Point current)
        {
            if (HasStarted)
            {
                LogService.Log("Close to start called after starting. Not good");
            }
            var distance = DistanceToNext(current);
            return distance < SysPrefs.TripPointsCloseThreshold;
        }

        public bool CloseToNext(Point current)
        {
            return DistanceToNext(current) < SysPrefs.TripPointsCloseThreshold;
        }

        public double DistanceToNext(Point current)
        {
            return PBUtilities.DistanceInMeters(current, NextPoint);
        }

        private Point PointAt(int index)
        {
            return (_points.Count >= index + 1) ? _points.ElementAt(index) : null;
        }

        public bool HasNext => PointAt(_index + 1) != null;
    }
}