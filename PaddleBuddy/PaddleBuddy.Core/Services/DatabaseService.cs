using System;
using System.Collections.Generic;
using System.Linq;
using PaddleBuddy.Core.Models;
using PaddleBuddy.Core.Models.LinqModels;
using PaddleBuddy.Core.Models.Map;
using PaddleBuddy.Core.Models.Messages;
using PaddleBuddy.Core.Utilities;
using UnitsNet;

namespace PaddleBuddy.Core.Services
{
    public class DatabaseService : ApiService
    {
        private static DatabaseService _databaseService;
        public List<TripSummary> TripSummaries { get; set; }
        public List<Point> Points { get; set; }
        public List<River> Rivers { get; set; }
        public List<Link> Links { get; set; }
        private bool _isReady;

        #region data stuff
        public List<TripSummary> GetSortedSummaries()
        {
            var sums = TripSummaries.ToList();
            sums.Sort();
            return sums;
        }

        public Point PickNextDestination(Point current, TripManager tripManager)
        {
            //list of points and their distances to the currentlocation
            var tripPoints = tripManager.Points.ToList();
            var pointsToCheck = new List<Tuple<Point, Length>>();
            //tripPoints.RemoveAll(p => PBUtilities.DistanceInMeters(current, p) > POINT_TOO_FAR_AWAY);
            for (var i = tripPoints.Count - 1; i >= 0; i--)
            {
                var dist = PBMath.Distance(current, tripPoints[i]);
                if (dist > PBPrefs.PickNextDestinationThreshold)
                {
                    tripPoints.RemoveAt(i);
                }
                else
                {
                    pointsToCheck.Add(new Tuple<Point, Length>(tripPoints[i], dist));
                }
            }
            pointsToCheck.Sort((x, y) => x.Item2.CompareTo(y.Item2));
            var closestNextPoint = new Tuple<Point, Length>(null, Length.MaxValue);
            foreach (var tup in pointsToCheck)
            {
                var point = tup.Item1;
                var next = GetInstance().GetNextPoint(point);
                var dist = PBMath.DistanceFromPointToLineSegment(point, next, current);
                if (dist < closestNextPoint.Item2)
                {
                    closestNextPoint = new Tuple<Point, Length>(next, dist);
                }
            }
            return closestNextPoint.Item1;
        }

        public Link GetLinkForPoints(Point a, Point b)
        {
            return GetLinkForPointIds(a.Id, b.Id);
        }

        public List<Point> GetLaunchSites()
        {
            return (from p in Points where p.IsLaunchSite select p).ToList();
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
            return GetRiver(id)?.Name;
        }

        public River GetRiver(int riverId)
        {
            return (from river in Rivers where river.Id == riverId select river).SingleOrDefault();
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
            return (from p in Points let dist = PBMath.Distance(point, p) orderby dist select p).First().RiverId;
        }

        public Path GetClosestRiverPath(Point point = null)
        {
            return GetPath(GetClosestRiverId(point));
        }

        public Point GetPoint(int pointId)
        {
            return (from point in Points where point.Id == pointId select point).SingleOrDefault();
        }

        public Point GetNextPoint(Point point)
        {
            var link = (from p in Links where p.Begin == point.Id select p).SingleOrDefault();
            return GetPoint(link.End);
        }

        public Path GetPath(int riverId)
        {
            var points = (from p in Points where p.RiverId == riverId select p)?.ToList();
            return new Path
            {
                RiverId = riverId,
                Points = points
            };
        }

        public Path GetPathWithDestination(int destinationPointId)
        {
            var point = GetInstance().GetPoint(destinationPointId);
            var path = GetInstance().GetPath(point.RiverId);
            if (path?.Points != null && path.Points.Count > 0 && path.Points.Contains(point))
            {
                try
                {
                    var index = path.Points.IndexOf(point);
                    path.Points.RemoveRange(index + 1, path.Points.Count - index - 1);
                }
                catch (Exception e)
                {
                    LogService.ExceptionLog(e.Message);
                }
            }
            else
            {
                LogService.Log("Unable to trim path in GetPathWithDestination");
            }
            return path;
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
        #endregion

        #region Update stuff
        public static DatabaseService GetInstance()
        {
            return _databaseService ?? (_databaseService = new DatabaseService());
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

        public DatabaseService()
        {
            IsReady = false;
            TripSummaries = new List<TripSummary>();
            Points = new List<Point>();
            Rivers = new List<River>();
            Links = new List<Link>();
        }

        public void UpdateIsReady()
        {
            IsReady = (Rivers != null && Rivers.Count > 0) &&
                    (Points != null && Points.Count > 0) &&
                    (Links != null && Links.Count > 0);
        }
        #endregion

        #region seeding

        public void SeedData()
        {
            SeedTripSummary();
            SeedPoints();
        }

        public void SeedTripSummary(int count = 3)
        {
            for (var i = 0; i < count; i++)
            {
                var tripSummary = new TripSummary
                {
                    StartDateTime = DateTime.Now,
                    RiverId = PBPrefs.RiverIdToSimulate
                };
                var endTIme = DateTime.Now;
                endTIme = endTIme.Add(new TimeSpan(0, 2, 20, 0));
                tripSummary.EndTime = endTIme;
                var points = GetPath(tripSummary.RiverId)?.Points;
                if (points != null && points.Count > 0)
                {
                    tripSummary.PointsHistory = points;
                }
                AddTripSummary(tripSummary);
            }
        }

        public void AddTripSummary(TripSummary tripSummary)
        {
            TripSummaries.Add(tripSummary);
        }

        public void SeedPoints(int count = 10)
        {
            var random = new Random();
            var strings = new[]
            {"asdf", "blue", "green", "red", "orange", "black", "purple", "georgia", "florida", "hawaii", "new york"};
            for (var i = 0; i < count; i++)
            {
                var randInt = random.Next(0, strings.Length);
                var point = new Point
                {
                    Id = PBUtilities.GetNextId(),
                    Label = strings[randInt]
                };
                AddPoint(point);
            }
        }

        private void AddPoint(Point p)
        {
            Points.Add(p);
        }
        #endregion

    }
}
