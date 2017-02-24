namespace PaddleBuddy.Core
{
    public class SysPrefs
    {
        private const string LocalDb = "http://10.0.3.3:4000/api/mobile/";
        private const string LiveDb = "http://paddlebuddy-pbdb.rhcloud.com/api/mobile/";
        public const string Website = "http://paddlebuddy-pbdb.rhcloud.com";

        public static bool UseLocalDb => false;
        public static string ApiBase => UseLocalDb ? LocalDb : LiveDb;
        public static Devices Device { get; set; }
        public static bool TestOffline = false;
        public static bool DisableMap = false;
        public static bool Simulate = false;
        public static double TripPointsCloseThreshold = 70;
        //used in camera updates to make current location appear closer to bottom of screen
        public static int MetersAheadToAim = 500; 

        public enum Devices
        {
            Android,
            iOS
        }
    }
}
