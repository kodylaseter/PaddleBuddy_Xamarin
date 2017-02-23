using System.Diagnostics;

namespace PaddleBuddy.Core.Services
{
    public class LogService
    {
        public static void Log(string msg)
        {
            Debug.WriteLine($"PB-G: {msg}");
        }

        public static void TagAndLog(string tag, string msg)
        {
            Debug.WriteLine($"PB-{tag} {msg}");
        }
    }
}
