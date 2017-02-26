using System;
using System.Collections.Generic;
using PaddleBuddy.Core.Models.Map;

namespace PaddleBuddy.Core.Models
{
    public class TripSummary
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public TimeSpan StartNavTime { get; set; }
        public List<Point> PointsHistory { get; set; } 
    }
}
