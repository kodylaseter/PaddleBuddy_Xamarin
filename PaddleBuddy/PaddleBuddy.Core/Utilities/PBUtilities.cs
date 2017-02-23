using PaddleBuddy.Core.Models;
using PaddleBuddy.Core.Models.LinqModels;
using PaddleBuddy.Core.Models.Map;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace PaddleBuddy.Core.Utilities
{
    public class PBUtilities
    {
        private static double EARTH_RADIUS_IN_METERS = 6371000;
        private static double EARTH_RADIUS_IN_KM = 6371;

        /// <summary>
        /// returns degrees
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
            return (float) Radians2Degrees(b);
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
            var distanceKm = distance / 1000; //converting to km
            var bearingRad = Degrese2Radians(bearing);
            var distRatio = distanceKm / EARTH_RADIUS_IN_KM;
            var distRatioSine = Math.Sin(distRatio);
            var distRatioCosine = Math.Cos(distRatio);

            var startLatRad = Degrese2Radians(start.Lat);
            var startLonRad = Degrese2Radians(start.Lng);

            var startLatCos = Math.Cos(startLatRad);
            var startLatSin = Math.Sin(startLatRad);

            var endLatRads = Math.Asin((startLatSin * distRatioCosine) + (startLatCos * distRatioSine * Math.Cos(bearingRad)));

            var endLonRads = startLonRad
                + Math.Atan2(
                    Math.Sin(bearingRad) * distRatioSine * startLatCos,
                    distRatioCosine - startLatSin * Math.Sin(endLatRads));
            var endLatDeg = Radians2Degrees(endLatRads);
            var endLonDeg = Radians2Degrees(endLonRads);
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
                var dist = DistanceInMeters(lp.BeginLat, lp.BeginLng, lp.EndLat, lp.EndLng);
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

        /// <summary>
        /// http://www.movable-type.co.uk/scripts/latlong.html
        /// 
        /// </summary>
        /// <param name="lineStart"></param>
        /// <param name="lineEnd"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static double DistanceInMetersFromPointToLineSegment(Point lineStart, Point lineEnd, Point point)
        {
            var angleFromPointToStartToEnd = AngleBetweenPoints(point, lineStart, lineEnd);
            var angleFromPointToEndToStart = AngleBetweenPoints(point, lineEnd, lineStart);
            //need to determine distance from 
            if (angleFromPointToStartToEnd > 90 && angleFromPointToEndToStart > 90)
            {
                throw new Exception("Something wrong in DistanceInMetersFromPointToLineSegment");
            }
            if (angleFromPointToStartToEnd > 90) //return distance from point to start
            {
                return DistanceInMeters(point, lineStart);
            }
            if (angleFromPointToEndToStart > 90) //return distance from point to end
            {
                return DistanceInMeters(point, lineEnd);
            }
            //return crosstrack
            return CrossTrackError(lineStart, lineEnd, point);
        }

        /// <summary>
        /// Angle in degrees between 3 points
        /// From A-B-C
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        private static double AngleBetweenPoints(Point a, Point b, Point c)
        {
            var baBearing = BearingBetweenPoints(b, a);
            var bcBearing = BearingBetweenPoints(b, c);
            return AngleBetweenBearings(baBearing, bcBearing);
        }

        /// <summary>
        /// Angle in degrees between 2 bearings
        /// </summary>
        /// <param name="b1"></param>
        /// <param name="b2"></param>
        /// <returns></returns>
        private static double AngleBetweenBearings(double b1, double b2)
        {
            var angle = Math.Abs(b1 - b2);
            if (angle > 180)
            {
                angle = 360 - angle;
            }
            return angle;
        }

        /// <summary>
        /// Calculates the distance from a point to a line around a sphere
        /// line is defined by start and end points, but continues forever
        /// http://www.movable-type.co.uk/scripts/latlong.html
        /// </summary>
        /// <param name="lineStart"></param>
        /// <param name="lineEnd"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        private static double CrossTrackError(Point lineStart, Point lineEnd, Point point)
        {
            var distanceInMeters = DistanceInMeters(lineStart, point);
            var startToPointBearing = Degrese2Radians(BearingBetweenPoints(lineStart, point));
            var lineBearing = Degrese2Radians(BearingBetweenPoints(lineStart, lineEnd));
            var dXt =
                Math.Asin(Math.Sin(distanceInMeters / EARTH_RADIUS_IN_METERS) * Math.Sin(startToPointBearing - lineBearing)) *
                EARTH_RADIUS_IN_METERS;
            return dXt;
        }

        //returns in miles
        public static double DistanceInMiles(Point begin, Point end)
        {
            if (begin != null && end != null) return DistanceInMeters(begin.Lat, begin.Lng, end.Lat, end.Lng) * 0.000621371;
            Debug.WriteLine("Distance calculation failed");
            return double.MaxValue;
        }

        public static double DistanceInMeters(Point begin, Point end)
        {
            if (begin != null && end != null) return DistanceInMeters(begin.Lat, begin.Lng, end.Lat, end.Lng);
            Debug.WriteLine("Distance calculation failed");
            return double.MaxValue;
        }

        public static double DistanceInRadians(Point p1, Point p2)
        {
            return DistanceInRadians(p1.Lat, p1.Lng, p2.Lat, p2.Lng);
        }

        public static double DistanceInRadians(double lat1, double lon1, double lat2, double lon2)
        {
            var l1 = Degrese2Radians(lat1);
            var l2 = Degrese2Radians(lat2);
            var deltaLat = Degrese2Radians(lat2 - lat1);
            var deltaLong = Degrese2Radians(lon2 - lon1);

            var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) + Math.Cos(l1) * Math.Cos(l2) * Math.Sin(deltaLong / 2) * Math.Sin(deltaLong / 2);
            return 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        }

        //METERS
        private static double DistanceInMeters(double lat1, double lon1, double lat2, double lon2)
        {
            var c = DistanceInRadians(lat1, lon1, lat2, lon2);
            return EARTH_RADIUS_IN_METERS * c;
        }

        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        //::  This function converts decimal degrees to radians             :::
        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        public static double Degrese2Radians(double deg)
        {
            return (deg * Math.PI / 180.0);
        }

        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        //::  This function converts radians to decimal degrees             :::
        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        public static double Radians2Degrees(double rad)
        {
            return (rad / Math.PI * 180.0);
        }

        /// <summary>
        /// Takes a distance in meters and returns string representation in meters or miles plus unit text
        /// </summary>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static string FormatDistanceToMilesOrMeters(double distance)
        {
            var miles = distance/1609.34;
            if (miles > 0.1)
            {
                return miles.ToString("0.00") + "mi";
            }
            return distance.ToString("0.0") + "m";
        }
    }
}

