using System;

namespace PaddleBuddy.Core.Models
{
    public class TripSummary
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public TimeSpan StartNavTime { get; set; }
    }
}
