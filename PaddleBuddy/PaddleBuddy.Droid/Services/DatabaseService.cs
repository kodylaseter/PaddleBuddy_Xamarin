using System.Collections.Generic;
using System.Linq;
using PaddleBuddy.Core.Models.LinqModels;
using PaddleBuddy.Core.Models.Map;
using PaddleBuddy.Core.Models.Messages;
using PaddleBuddy.Core.Services;
using PaddleBuddy.Core.Utilities;

namespace PaddleBuddy.Droid.Services
{
    public class DatabaseService : ApiService
    {
        private static DatabaseService _databaseService;
        private List<River> _rivers;
        private List<Point> _points;
        private List<Link> _links;
        public int ClosestRiverId { get; set; }
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
                MessengerService.Messenger.Send(new DbReadyMessage());
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

        public River GetRiver(int id)
        {
            return (from river in Rivers where river.Id == id select river).Single();
        }

        //todo: enable this
        public Path GetClosestRiver()
        {
            var curr = LocationService.GetInstance().CurrentLocation;
            var point = (from p in Points let dist = PBUtilities.DistanceInMiles(curr, p) orderby dist ascending select p).First();
            ClosestRiverId = point.RiverId;
            return GetPath(point.RiverId);
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
