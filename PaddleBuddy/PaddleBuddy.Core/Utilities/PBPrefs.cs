namespace PaddleBuddy.Core.Utilities
{
    public class PBPrefs
    {
        private static string LocalDb = "http://10.0.3.3:4000/api/mobile/";
        private static string LiveDb = "http://paddlebuddy-pbdb.rhcloud.com/api/mobile/";
        public static string Website = "http://paddlebuddy-pbdb.rhcloud.com";
        public static bool UseLocalDb => false;
        public static string ApiBase => UseLocalDb ? LocalDb : LiveDb;

        //permission stuff
        public const int PERMISSION_LOCATION = 0;

        //serializable stuff
        public const string SERIALIZABLE_TRIPSUMMARY = "TRIP_SUMMARY";

        //sharedpreferences stuff
        public const string KEY_USER_ID = "USER_ID";

        public static bool TestOffline = false;
        public static bool TestLoggedOut = false;
        
        public static int RiverIdToSimulate = 8;
        public static double TripPointsCloseThreshold = 30;
        public static double BearingTooCloseThreshold = 10;
        public static double IsOnTrackCloseThreshold = 80;
        public static double PickNextDestinationThreshold = 1000;
        public static int MetersAheadToAim = 500; //used in camera updates to make current location appear closer to bottom of screen
        public static float DefaultSpeed = 1.5f; //meters per second

        public static Devices Device { get; set; }
        public enum Devices
        {
            Android,
            iOS
        }
    }
}
