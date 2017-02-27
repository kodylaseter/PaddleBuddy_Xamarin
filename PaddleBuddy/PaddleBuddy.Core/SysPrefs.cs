namespace PaddleBuddy.Core
{
    public class SysPrefs
    {
        private const string LocalDb = "http://10.0.3.3:4000/api/mobile/";
        private const string LiveDb = "http://paddlebuddy-pbdb.rhcloud.com/api/mobile/";
        public const string Website = "http://paddlebuddy-pbdb.rhcloud.com";

        //permission stuff
        public const int PERMISSION_LOCATION = 0;

        //serializable stuff
        public const string SERIALIZABLE_TRIPSUMMARY = "TRIP_SUMMARY";

        public static bool UseLocalDb => false;
        public static string ApiBase => UseLocalDb ? LocalDb : LiveDb;
        public static Devices Device { get; set; }

        public static bool TestOffline = false;
        public static bool DisableMap = false;

        public static bool Simulate = false;
        public static int SimulateRiver = 8;


        public static double TripPointsCloseThreshold = 30;
        public static double BearingTooCloseThreshold = 10;
        //used in camera updates to make current location appear closer to bottom of screen
        public static int MetersAheadToAim = 500;
        public static float DefaultSpeed = 1.5f; //meters per second

        public enum Devices
        {
            Android,
            iOS
        }
    }
}
