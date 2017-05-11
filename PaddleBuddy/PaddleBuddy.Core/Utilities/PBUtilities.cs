using System;
using System.Threading;
using UnitsNet;
using Newtonsoft.Json.Linq;

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
        public static string FormatDistanceToMilesOrMeters(Length distance)
        {
            if (distance.Miles > 0.1)
            {
                return distance.Miles + "mi";
            }
            return distance.Meters + "m";
        }

		private static Random random = new Random();
		public static string RandomString(int length)
		{
			var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[length];

			for (int i = 0; i < stringChars.Length; i++)
			{
				stringChars[i] = chars[random.Next(chars.Length)];
			}

			return new String(stringChars);
		}
    }
}
