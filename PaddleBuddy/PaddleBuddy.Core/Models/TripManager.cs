using System;
using System.Collections.Generic;
using System.Linq;
using PaddleBuddy.Core.Models.Map;
using PaddleBuddy.Core.Services;
using PaddleBuddy.Core.Utilities;

namespace PaddleBuddy.Core.Models
{ 
    public class TripManager
    {

        private int _index;
        private List<Point> _points;
        private List<Point> _pointsHistory; 
        private const double IS_CLOSE_THRESHOLD = 100; //meters
        private TimeSpan _timeStart;
        private TimeSpan _timeEnd;

        public TripManager()
        {
            Index = 0;
            _timeStart = DateTime.Now.TimeOfDay;
            _pointsHistory = new List<Point>();
        }

        public TripSummary ExportTripSummary()
        {
            return new TripSummary
            {
                StartTime = _timeStart,
                EndTime = DateTime.Now.TimeOfDay,
                PointsHistory = _pointsHistory
            };
        }


        public int Index
        {
            get { return _index; }
            set
            {
                _index = value;
                if (HasCurrentPoint)
                {
                    LogService.Log($"Nextpoint changed to {Points[_index].Id}");
                }
            }
        }

        public List<Point> Points
        {
            get { return _points; }
            set
            {
                _points = value; 
                
            }
        }

        public void AddToPointHistory(Point p)
        {
            _pointsHistory.Add(p);
        }

        public Point StartPoint
        {
            get { return Points.First(); }
        }

        public Point CurrentPoint
        {
            get { return PointAt(Index); }
        }

        public Point PreviousPoint
        {
            get { return PointAt(Index - 1);}
        }

        public bool HasStarted
        {
            get { return Index > 0; }
        }

        public bool HasPrevious
        {
            get { return Index > 0 && Points.Count > 0 && Points.ElementAt(Index - 1) != null; }
        }

        public bool HasCurrentPoint
        {
            get { return Points != null && Points.Count > 0 && Points.ElementAt(Index) != null; }
        }

        public List<Point> RemainingPoints
        {
            get
            {
                var points = Points.ToList();
                points.RemoveRange(0, Index);
                return points;
            }
        } 

        public void Increment()
        {
            Index++;
        }

        public void UpdateForNewDestination(Point newDestination)
        {
            Index = Points.IndexOf(Points.Single(p => p.Id == newDestination.Id));
        }

        /// <summary>
        /// Improve with pixel detection and rectangle bounding around line?
        /// </summary>
        /// <param name="lineStart"></param>
        /// <param name="lineEnd"></param>
        /// <param name="current"></param>
        /// <returns></returns>
        public bool IsOnTrack(Point lineStart, Point lineEnd, Point current)
        {
            var dist = PBMath.DistanceInMetersFromPointToLineSegment(lineStart, lineEnd, current);
            return Math.Abs(dist) < IS_CLOSE_THRESHOLD;
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
            return PBMath.DistanceInMeters(current, CurrentPoint);
        }

        private Point PointAt(int index)
        {
            return (Points.Count >= index + 1) ? Points.ElementAt(index) : null;
        }

        public bool HasNext => PointAt(Index + 1) != null;
    }
}