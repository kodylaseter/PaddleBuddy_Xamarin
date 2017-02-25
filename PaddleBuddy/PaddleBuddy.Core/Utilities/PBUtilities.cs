using System;
using System.Threading;

namespace PaddleBuddy.Core.Utilities
{
    public class PBUtilities
    {
        private static int newObjectId = Int32.MinValue;

        public static int GetNextId()
        {
            return Interlocked.Increment(ref newObjectId);
        }

        /// <summary>
        /// Takes a distance in meters and returns string representation in meters or miles plus unit text
        /// </summary>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static string FormatDistanceToMilesOrMeters(double distance)
        {
            var miles = distance / 1609.34;
            if (miles > 0.1)
            {
                return miles.ToString("0.00") + "mi";
            }
            return distance.ToString("0.0") + "m";
        }
    }
}
