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

        private const double IS_CLOSE_THRESHOLD = 100; //meters

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

        public Point LastPoint
        {
            get { return PointAt(_index - 1);}
        }

        public bool HasStarted
        {
            get { return _index > 0; }
        }


        public void Increment()
        {
            _index++;
        }

        public bool IsOnTrack(Point lineStart, Point lineEnd, Point current)
        {
            var dist = PBUtilities.DistanceInMetersFromPointToLine(lineStart, lineEnd, current);
            return dist < IS_CLOSE_THRESHOLD;
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