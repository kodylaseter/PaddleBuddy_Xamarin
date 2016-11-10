using System.Collections.Generic;
using System.Linq;
using PaddleBuddy.Core.Models.Map;

namespace PaddleBuddy.Core.Models
{
    public class TripData
    {
        private List<Point> _points;

        public List<Point> Points
        {
            get { return _points; }
            set { _points = value; }
        }

        public Point NextPoint
        {
            get { return _points.First(); }
        }
    }
}