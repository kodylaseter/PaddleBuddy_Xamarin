using System;

namespace PaddleBuddy.Core.Utilities
{
    public static class ObjectExtensions
    {
        public static string ToStringHrsMinsAmPm(this DateTime time)
        {
            var str = time.ToString("hh:mm tt");
            if (str.Substring(0, 1) == "0")
            {
                str = str.Remove(0, 1);
            }
            return str;
        }

        public static string ToStringHrsMins(this TimeSpan time)
        {
            var str = time.ToString("hh:mm");
            if (str.Substring(0, 1) == "0")
            {
                str = str.Remove(0, 1);
            }
            return str;
        }

        public static string ToStringHrsOrMins(this TimeSpan time)
        {
            var str = "";
            if (time.TotalHours > 0)
            {
                str += time.Hours;
                str += " hr";
            }
            if (time.Minutes <= 0) return str;
            str += " " + time.Minutes;
            str += " min";
            return str;
        }
    }
}
