using System.Diagnostics;

namespace PaddleBuddy.Core.Services
{
    public class LogService
    {
        public static void Log(string msg)
        {
            Debug.WriteLine(msg);
        }

        public static void Log(object o)
        {
            Debug.WriteLine(o);
        }
    }
}
