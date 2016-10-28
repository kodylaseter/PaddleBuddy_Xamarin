using System;

namespace PaddleBuddy.Core.Models
{
    public class TripEstimate
    {
        public TimeSpan Time { get; set; }
        //public double Time { get; set; }
        public string TimeUnit { get; set; }
        //todo: implement distanceunit of some sort?
        public double Distance { get; set; }
        public string DistanceUnit { get; set; }

        public int StartId { get; set; }
        public int EndId { get; set; }
        public int RiverId { get; set; }

        public string TimeString
        {
            get
            {
                return Time.ToString();
            }
        }

        public override string ToString()
        {
            return Time.ToString(@"hh\:mm\:ss") + " for " + Distance.ToString("#.##") + " " + DistanceUnit;
        }
    }
}
