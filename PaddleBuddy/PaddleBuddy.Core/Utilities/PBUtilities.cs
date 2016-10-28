using PaddleBuddy.Core.Models;
using PaddleBuddy.Core.Models.LinqModels;
using PaddleBuddy.Core.Models.Map;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using PaddleBuddy.Core.Services;

namespace PaddleBuddy.Core.Utilities
{
    public class PBUtilities
    {
        public static TripEstimate LinksToEstimate(List<LinkPoint> list)
        {
            double totalTime = 0;
            double totalDistance = 0;
            foreach (var lp in list)
            {
                var dist = Distance(lp.BeginLat, lp.BeginLng, lp.EndLat, lp.EndLng);
                totalDistance += dist;
                totalTime += dist / lp.Speed * 3600000;

            }
            return new TripEstimate()
            {
                Time = TimeSpan.FromMilliseconds(totalTime),
                Distance = totalDistance,
                DistanceUnit = "miles"
            };
        }

        //returns in miles
        public static double Distance(Point begin, Point end)
        {
            if (begin != null || end != null) return Distance(begin.Lat, begin.Lng, end.Lat, end.Lng) * 0.000621371;
            Debug.WriteLine("Distance calculation failed");
            return double.MaxValue;
        }

        public static double DistanceInMeters(Point begin, Point end)
        {
            if (begin != null || end != null) return Distance(begin.Lat, begin.Lng, end.Lat, end.Lng);
            Debug.WriteLine("Distance calculation failed");
            return double.MaxValue;
        }

        //METERS
        public static double Distance(double lat1, double lon1, double lat2, double lon2)
        {
            var R = 6371000;
            var l1 = deg2rad(lat1);
            var l2 = deg2rad(lat2);
            var deltaLat = deg2rad(lat2 - lat1);
            var deltaLong = deg2rad(lon2 - lon1);

            var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) + Math.Cos(l1) * Math.Cos(l2) * Math.Sin(deltaLong / 2) * Math.Sin(deltaLong / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var d = R * c;
            return d;
            //convert to miles
            //return d * 0.000621371;
        }

        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        //::  This function converts decimal degrees to radians             :::
        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        public static double deg2rad(double deg)
        {
            return (deg * Math.PI / 180.0);
        }

        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        //::  This function converts radians to decimal degrees             :::
        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        public static double rad2deg(double rad)
        {
            return (rad / Math.PI * 180.0);
        }
    }
}

