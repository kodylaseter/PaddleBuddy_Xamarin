using System;
using System.Collections.Generic;
using System.Linq;
using PaddleBuddy.Core.Models.Map;
using PaddleBuddy.Core.Services;
using PaddleBuddy.Core.Utilities;

namespace PaddleBuddy.Core.Models
{
    public class TripData
    {
        private List<Point> _points;
        private int _index;

        public TripData()
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

        public void Increment()
        {
            _index++;
        }

        public bool HasStarted
        {
            get { return _index > 0; }
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
            Point point;
            try
            {
                point = _points.ElementAt(index);
            }
            catch (Exception e)
            {
                LogService.Log("Issue in pointAt");
                LogService.Log(e.Message);
                return null;
            }
            return point;
        }

        public bool HasNext
        {
            get { return PointAt(_index + 1) != null; }
        }
    }
}