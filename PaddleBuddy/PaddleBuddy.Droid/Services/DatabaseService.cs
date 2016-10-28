using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PaddleBuddy.Core.Models.LinqModels;
using PaddleBuddy.Core.Models.Map;
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
        private string[] names = { "points", "rivers", "links" };
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

        public async Task Setup(bool sync = false)
        {
            if (sync)
            {
                await UpdateAll();
            }
            if (StorageService.HasData(names))
            {
                Points = JsonConvert.DeserializeObject<List<Point>>(StorageService.ReadSerializedFromFile("points"));
                Rivers = JsonConvert.DeserializeObject<List<River>>(StorageService.ReadSerializedFromFile("rivers"));
                Links = JsonConvert.DeserializeObject<List<Link>>(StorageService.ReadSerializedFromFile("links"));
            }
            UpdateIsReady();
            if (!IsReady) await UpdateAll();
        }

        public async Task UpdateAll()
        {
            await UpdateRivers();
            await UpdateLinks();
            await UpdatePoints();
            SaveData();
        }

        public void SaveData()
        {
            var points = JsonConvert.SerializeObject(_points);
            var rivers = JsonConvert.SerializeObject(_rivers);
            var links = JsonConvert.SerializeObject(_links);
            StorageService.SaveSerializedToFile(points, "points");
            StorageService.SaveSerializedToFile(rivers, "rivers");
            StorageService.SaveSerializedToFile(links, "links");
            UpdateIsReady();
        }

        private void UpdateIsReady()
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
                //todo: this notification wont work
                //if (_isReady) MessengerService.Messenger.Publish(new DbReadyMessage(this));
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
        //public Path GetClosestRiver()
        //{
        //    var curr = LocationService.GetInstance().CurrentLocation;
        //    var point = (from p in Points let dist = PBUtilities.Distance(curr, p) orderby dist ascending select p).First();
        //    ClosestRiverId = point.RiverId;
        //    return GetPath(point.RiverId);
        //}

        public Point GetPoint(int id)
        {
            return (from point in Points where point.Id == id select point).Single();
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

        public async Task<bool> UpdatePoints()
        {
            Stopwatch stop = new Stopwatch();
            stop.Start();
            try
            {
                var resp = await GetAsync("all_points/");
                if (resp.Success)
                {
                    Points = JsonConvert.DeserializeObject<List<Point>>(resp.Data.ToString());
                }
                else
                {
                    LogService.Log("Failed to update points");
                    return false;
                }
            }
            catch (Exception e)
            {

                LogService.Log(e);
                LogService.Log("Failed to update points");
                return false;
            }
            stop.Stop();
            Debug.WriteLine("pbuddy points: " + stop.Elapsed);
            return true;
        }

        public async Task<bool> UpdateRivers()
        {
            Stopwatch stop = new Stopwatch();
            stop.Start();
            try
            {
                var resp = await GetAsync("all_rivers/");
                if (resp.Success)
                {
                    Rivers = JsonConvert.DeserializeObject<List<River>>(resp.Data.ToString());
                }
                else
                {
                    LogService.Log("Failed to update rivers");
                    return false;
                }
            }
            catch (Exception e)
            {
                LogService.Log(e);
                LogService.Log("Failed to update rivers");
                return false;
            }
            stop.Stop();
            Debug.WriteLine("pbuddy rivers: " + stop.Elapsed);
            return true;
        }

        public async Task<bool> UpdateLinks()
        {
            Stopwatch stop = new Stopwatch();
            stop.Start();
            try
            {
                var resp = await GetAsync("all_links/");
                if (resp.Success)
                {
                    Links = JsonConvert.DeserializeObject<List<Link>>(resp.Data.ToString());
                }
                else
                {
                    LogService.Log("Failed to update links");
                    return false;
                }
            }
            catch (Exception e)
            {
                LogService.Log(e);
                LogService.Log("Failed to update links");
                return false;
            }
            stop.Stop();
            Debug.WriteLine("pbuddy links: " + stop.Elapsed);
            return true;
        }
    }
}
