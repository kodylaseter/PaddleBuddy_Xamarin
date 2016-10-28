using Plugin.Connectivity;

namespace PaddleBuddy.Core.Services
{
    public class NetworkService
    {
        public static bool IsOnline
        {
            get { return CrossConnectivity.Current.IsConnected; }
        }

        public static bool IsServerAvailable
        {
            get
            {
                //TODO: implement server checking
                return IsOnline;
            }
        }
    }
}
