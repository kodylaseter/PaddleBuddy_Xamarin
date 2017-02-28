﻿using System;
using System.Collections.Generic;
using System.Linq;
using PaddleBuddy.Core.Models;
using PaddleBuddy.Core.Models.LinqModels;
using PaddleBuddy.Core.Models.Map;
using PaddleBuddy.Core.Models.Messages;
using PaddleBuddy.Core.Utilities;

namespace PaddleBuddy.Core.Services
{
    public class DatabaseService : ApiService
    {
        private static DatabaseService _databaseService;
        private List<River> _rivers;
        private List<Point> _points;
        private List<Link> _links;
        private bool _isReady;

        public static DatabaseService GetInstance()
        {
            return _databaseService ?? (_databaseService = new DatabaseService());
        }

        public DatabaseService()
        {
            _isReady = false;
        }

        public void UpdateIsReady()
        {
            IsReady = (_rivers != null && _rivers.Count > 0) &&
                    (_points != null && _points.Count > 0) &&
                    (_links != null && _links.Count > 0);
        }

        public bool IsReady
        {
            get { return _isReady; }
            set
            {
                _isReady = value;
                if (_isReady)
                {
                    MessengerService.Messenger.Send(new DbReadyMessage());
                }
            }
        }

        public List<Point> Points
        {
            get { return _points; }
            set { _points = value; }
        }

        public List<River> Rivers
        {
            get { return _rivers; }
            set { _rivers = value; }
        }

        public List<Link> Links
        {
            get { return _links; }
            set { _links = value; }
        }

        public Point PickNextDestination(Point current, TripManager tripManager)
        {
            //list of points and their distances to the currentlocation
            var tripPoints = tripManager.Points.ToList();
            var pointsToCheck = new List<Tuple<Point, double>>();
            //tripPoints.RemoveAll(p => PBUtilities.DistanceInMeters(current, p) > POINT_TOO_FAR_AWAY);
            for (var i = tripPoints.Count - 1; i >= 0; i--)
            {
                var dist = PBMath.DistanceInMeters(current, tripPoints[i]);
                if (dist > SysPrefs.PickNextDestinationThreshold)
                {
                    tripPoints.RemoveAt(i);
                }
                else
                {
                    pointsToCheck.Add(new Tuple<Point, double>(tripPoints[i], dist));
                }
            }
            pointsToCheck.Sort((x, y) => x.Item2.CompareTo(y.Item2));
            var closestNextPoint = new Tuple<Point, double>(null, double.MaxValue);
            foreach (var tup in pointsToCheck)
            {
                var point = tup.Item1;
                var next = GetInstance().GetNextPoint(point);
                var dist = PBMath.DistanceInMetersFromPointToLineSegment(point, next, current);
                if (dist < closestNextPoint.Item2)
                {
                    closestNextPoint = new Tuple<Point, double>(next, dist);
                }
            }
            return closestNextPoint.Item1;
        }

        public Link GetLinkForPoinds(Point a, Point b)
        {
            return GetLinkForPointIds(a.Id, b.Id);
        }

        /// <summary>
        /// Gets the link from a to b
        /// Maybe check for the opposite? 
        /// </summary>
        /// <param name="id1"></param>
        /// <param name="id2"></param>
        /// <returns></returns>
        public Link GetLinkForPointIds(int id1, int id2)
        {
            var link = Links.SingleOrDefault(l => l.Begin == id1 && l.End == id2);
            return link;
        }

        public string GetRiverName(int id)
        {
            return GetRiver(id).Name;
        }

        public River GetRiver(int id)
        {
            return (from river in Rivers where river.Id == id select river).Single();
        }

        public int GetClosestRiverId(Point point = null)
        {
            if (point == null)
            {
                point = LocationService.GetInstance().CurrentLocation;
            }
            if (point == null)
            {
                throw new NotImplementedException();
            };
            return (from p in Points let dist = PBMath.DistanceInMiles(point, p) orderby dist ascending select p).First().RiverId;
        }

        public Path GetClosestRiverPath(Point point = null)
        {
            return GetPath(GetClosestRiverId(point));
        }

        public Point GetPoint(int id)
        {
            return (from point in Points where point.Id == id select point).Single();
        }

        public Point GetNextPoint(Point point)
        {
            var link = (from p in Links where p.Begin == point.Id select p).Single();
            return GetPoint(link.End);
        }

        public Path GetPath(int riverId)
        {
            var points = (from p in Points where p.RiverId == riverId select p).ToList();
            return new Path
            {
                RiverId = riverId,
                Points = points
            };
        }

        public Path GetPath(Point start, Point end)
        {
            //todo: clean this up
            var path = new Path();
            path.Points = new List<Point>();
            if (start.RiverId != end.RiverId)
            {
                LogService.Log("invalid points for path");
            }
            path.RiverId = start.RiverId;
            var tempList = (from p in Points where p.RiverId == start.RiverId join lnk in Links on p.Id equals lnk.Begin select new PointWithNext(p, lnk.End)).ToList();
            if (tempList.Count > 0)
            {
                var temp = (from p in tempList where p.Point.Id == start.Id select p).Single();
                if (temp == null)
                {
                    LogService.Log("Failed to get path");
                }
                else
                {
                    path.Points.Add(temp.Point);
                } 
                while (temp != null && temp.Point.Id != end.Id)
                {
                    temp = (from p in tempList where p.Point.Id == temp.Next select p).First();
                    if (temp != null) path.Points.Add(temp.Point);
                    else
                    {
                        LogService.Log("Failed to get path");
                        break;
                    }
                }
                
            }
            return path;
        }
    }
}
