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
        /// <summary>
        /// credit: http://www.movable-type.co.uk/scripts/latlong.html
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns> returns float in degrees </returns>
        public static float BearingBetweenPoints(Point start, Point end)
        {
            var x = Math.Cos(end.Lat) * Math.Sin(start.Lng - end.Lng);
            var y = Math.Cos(start.Lat)*Math.Sin(end.Lat) -
                    Math.Sin(start.Lat)*Math.Cos(end.Lat)*Math.Cos(start.Lng - end.Lng);
            var b = Math.Atan2(x, y);
            return (float) rad2deg(b);
        }

        /// <summary>
        /// http://stackoverflow.com/a/8243966/4867663
        /// </summary>
        /// <param name="start"></param>
        /// <param name="distance"> in meters</param>
        /// <param name="bearing"> in degrees</param>
        /// <returns></returns>
        public static Point PointAtDistanceAlongBearing(Point start, double distance, double bearing)
        {
            var earthRadiusKm = 6371.01;
            var distanceKm = distance / 1000; //converting to km
            var bearingRad = deg2rad(bearing);
            var distRatio = distanceKm / earthRadiusKm;
            var distRatioSine = Math.Sin(distRatio);
            var distRatioCosine = Math.Cos(distRatio);

            var startLatRad = deg2rad(start.Lat);
            var startLonRad = deg2rad(start.Lng);

            var startLatCos = Math.Cos(startLatRad);
            var startLatSin = Math.Sin(startLatRad);

            var endLatRads = Math.Asin((startLatSin * distRatioCosine) + (startLatCos * distRatioSine * Math.Cos(bearingRad)));

            var endLonRads = startLonRad
                + Math.Atan2(
                    Math.Sin(bearingRad) * distRatioSine * startLatCos,
                    distRatioCosine - startLatSin * Math.Sin(endLatRads));
            var endLatDeg = rad2deg(endLatRads);
            var endLonDeg = rad2deg(endLonRads);
            return new Point
            {
                Lat = endLatDeg,
                Lng = endLonDeg
            };
        }

        public static Point PointBetween(Point current, Point goal, double fraction)
        {
            var point = PointAdd(current, PointMultiplyByFraction(PointSubtract(goal, current), fraction));
            return point;
        }

        public static Point PointAdd(Point a, Point b)
        {
            return new Point
            {
                Lat = a.Lat + b.Lat,
                Lng = a.Lng + b.Lng
            };
        }

        public static Point PointSubtract(Point a, Point b)
        {
            return new Point
            {
                Lat = a.Lat - b.Lat,
                Lng = a.Lng - b.Lng
            };
        }

        public static Point PointMultiplyByFraction(Point point, double fraction)
        {
            return new Point
            {
                Lat = point.Lat * fraction,
                Lng = point.Lng * fraction
            };
        }

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
        public static double DistanceInMiles(Point begin, Point end)
        {
            if (begin != null && end != null) return Distance(begin.Lat, begin.Lng, end.Lat, end.Lng) * 0.000621371;
            Debug.WriteLine("Distance calculation failed");
            return double.MaxValue;
        }

        public static double DistanceInMeters(Point begin, Point end)
        {
            if (begin != null && end != null) return Distance(begin.Lat, begin.Lng, end.Lat, end.Lng);
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

