using UnitsNet;
using UnitsNet.Extensions.NumberToLength;
using UnitsNet.Extensions.NumberToSpeed;

namespace PaddleBuddy.Core.Utilities
{
    public class PBPrefs
    {
        private static readonly string LocalWeb = "http://10.82.142.129:4040/";
        private static readonly string LiveWEb = "http://paddlebuddy-pbdb.rhcloud.com/";
        public static bool UseLocalWeb => false;
        public static string WebBase => UseLocalWeb ? LocalWeb : LiveWEb;

        //permission stuff
        public const int PERMISSION_LOCATION = 0;

        //serializable stuff
        public const string SERIALIZABLE_TRIPSUMMARY = "TRIP_SUMMARY";

        //sharedpreferences stuff
        public const string KEY_USER_ID = "USER_ID";

        public const string KEY_JWT_TOKEN = "JWT_TOKEN";

        public static readonly bool TestOffline = false;
        public static readonly bool TestLoggedOut = true;
        
        public static readonly int RiverIdToSimulate = 20;
        public static readonly Length TripPointsCloseThreshold = 30.Meters();
        public static readonly Length BearingTooCloseThreshold = 10.Meters();
        public static readonly Length IsOnTrackCloseThreshold = 80.Meters();
        public static readonly Length PickNextDestinationThreshold = 1000.Meters();
        public static readonly Length DistanceAheadToAim = 500.Meters(); //used in camera updates to make current location appear closer to bottom of screen
        public static readonly Speed DefaultSpeed = 1.5.MetersPerSecond();

        public static readonly int MinPasswordLength = 6;

        public static Devices Device { get; set; }
        public enum Devices
        {
            Android,
            iOS
        }
    }
}
