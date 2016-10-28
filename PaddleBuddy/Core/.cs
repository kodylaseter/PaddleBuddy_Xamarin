namespace Core
{
    public class SysPrefs
    {
        private const string _localDB = "http://10.0.3.3:4000/api/mobile/";
        private const string _liveDB = "http://paddlebuddy-pbdb.rhcloud.com/api/mobile/";

        public static bool UseLocalDB => false;

        public static string ApiBase => UseLocalDB ? _localDB : _liveDB;

        public static string Website => "http://paddlebuddy-pbdb.rhcloud.com";
    }
}
