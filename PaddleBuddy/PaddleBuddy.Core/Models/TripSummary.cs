using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using PaddleBuddy.Core.Models.Map;

namespace PaddleBuddy.Core.Models
{
    [DataContract]
    public class TripSummary
    {
        public TimeSpan StartTime { get; set; }


        [DataMember]
        public TimeSpan EndTime { get; set; }

        public TimeSpan StartNavTime { get; set; }
        public List<Point> PointsHistory { get; set; } 
    }
}
