using System;
using PaddleBuddy.Core.Utilities;
using UnitsNet;

namespace PaddleBuddy.Core.Models
{
    public class TripEstimate
    {
        public TimeSpan Time { get; set; }
        public Length Distance { get; set; }

        public int StartId { get; set; }
        public int EndId { get; set; }
        public int RiverId { get; set; }

        public string TimeRemaining
        {
            get { return Time.ToStringHrsMinsText(); }
        } 

        public override string ToString()
        {
            return Time.ToString() + " for " + Distance.Miles + " " + "Miles";
        }
    }
}
