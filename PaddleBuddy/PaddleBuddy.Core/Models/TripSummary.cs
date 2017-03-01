using System;
using System.Collections.Generic;
using PaddleBuddy.Core.Models.Map;
using PaddleBuddy.Core.Utilities;

namespace PaddleBuddy.Core.Models
{
    public class TripSummary : IComparable<TripSummary>
    {
        public DateTime StartDateTime { get; set; }
        public DateTime EndTime { get; set; }
        public List<Point> PointsHistory { get; set; } 
        public int RiverId { get; set; }
        public long Id { get; set; }

        public TripSummary()
        {
            Id = PBUtilities.GetNextId();
        }

        public int CompareTo(TripSummary other)
        {
            return StartDateTime.CompareTo(other.StartDateTime);
        }
    }
}
