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
        private int Index { get; set; }
        public List<Point> Points { get; set; }

        private const double IS_CLOSE_THRESHOLD = 100; //meters

        public TripManager()
        {
            Index = 0;
        }


        public Point StartPoint
        {
            get { return Points.First(); }
        }

        public Point NextPoint
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



        public void Increment()
        {
            Index++;
        }

        public void UpdateForNewDestination(Point newDestination)
        {
            Index = Points.IndexOf(newDestination);
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
            var dist = PBUtilities.DistanceInMetersFromPointToLine(lineStart, lineEnd, current);
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
            return PBUtilities.DistanceInMeters(current, NextPoint);
        }

        private Point PointAt(int index)
        {
            return (Points.Count >= index + 1) ? Points.ElementAt(index) : null;
        }

        public bool HasNext => PointAt(Index + 1) != null;
    }
}