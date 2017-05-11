using System.Diagnostics;

namespace PaddleBuddy.Core.Services
{
    public class LogService
    {
        public static void Log(string msg)
        {
            TagAndLog("G", msg);
        }

        public static void Log(object o)
        {
            TagAndLog("G", o.ToString());
        }

        public static void TagAndLog(string tag, string msg)
        {
            Debug.WriteLine($"PB-{tag}: {msg}");
        }

        public static void ExceptionLog(string msg)
        {
            TagAndLog("EX", msg);
        }
    }
}
