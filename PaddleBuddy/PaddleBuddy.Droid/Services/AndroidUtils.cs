using System.Collections.Generic;
using System.Linq;
using Android.Gms.Maps.Model;
using PaddleBuddy.Core.Models.Map;

namespace PaddleBuddy.Droid.Services
{
    public class AndroidUtils
    {
        public static LatLng PointToLatLng(Point p)
        {
            return new LatLng(p.Lat, p.Lng);
        }

        public static LatLng[] PointsToLatLngs(List<Point> points)
        {
            return points.Select(PointToLatLng).ToArray();
        } 
    }
}