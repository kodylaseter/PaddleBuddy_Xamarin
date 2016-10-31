using Android.Gms.Maps.Model;
using PaddleBuddy.Core.Models.Map;

namespace PaddleBuddy.Droid.Services
{
    public class AndroidUtils
    {
        public static LatLng ToLatLng(Point p)
        {
            return new LatLng(p.Lat, p.Lng);
        }
    }
}