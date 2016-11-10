namespace PaddleBuddy.Core
{
    public class SysPrefs
    {
        private const string _localDB = "http://10.0.3.3:4000/api/mobile/";
        private const string _liveDB = "http://paddlebuddy-pbdb.rhcloud.com/api/mobile/";

        public static bool UseLocalDB => false;

        public static string ApiBase => UseLocalDB ? _localDB : _liveDB;

        public static string Website => "http://paddlebuddy-pbdb.rhcloud.com";

        public static Devices Device { get; set; }

        public static bool TestOffline = false;
        public static bool DisableMap = false;
        public static bool Simulate = false;

        public enum Devices
        {
            Android,
            iOS
        }

        public static double TripPointsCloseThreshold = 20;



    }
}
