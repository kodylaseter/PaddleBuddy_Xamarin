using System;
using System.Collections.Generic;
using System.Linq;
using Android.Gms.Maps.Model;
using Android.Support.V4.App;
using Android.Views;
using PaddleBuddy.Core.Models.Map;

namespace PaddleBuddy.Droid.Utilities
{
    public static class AndroidObjectExtensions
    {
        public static bool IsVisible(this View view)
        {
            return view.Visibility == ViewStates.Visible;
        }

        public static bool IsTypeOf(this object obj, Type type)
        {
            return obj.GetType() == type;
        }


        #region point and latlng
        public static LatLng ToLatLng(this Point value)
        {
            return new LatLng(value.Lat, value.Lng);
        }

        public static Point ToPoint(this LatLng value)
        {
            return new Point
            {
                Lat = value.Latitude,
                Lng = value.Longitude
            };
        }

        public static LatLng[] ToLatLngs(this List<Point> points)
        {
            return points.Select(a => a.ToLatLng()).ToArray();
        }

        public static Point[] ToPoints(this List<LatLng> latLngs)
        {
            return latLngs.Select(a => a.ToPoint()).ToArray();
        }
        #endregion  

        #region cheesebaron's object conversions

        /// <summary>
        /// credit to cheesebaron
        /// https://gist.github.com/Cheesebaron/9876783
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TObject ToNetObject<TObject>(this Java.Lang.Object value)
        {
            if (value == null)
                return default(TObject);

            if (!(value is JavaHolder))
                throw new InvalidOperationException("Unable to convert to .NET object. Only Java.Lang.Object created with .ToJavaObject() can be converted.");

            TObject returnVal;
            try { returnVal = (TObject)((JavaHolder)value).Instance; }
            finally { value.Dispose(); }
            return returnVal;
        }

        /// <summary>
        /// credit to cheesebaron
        /// https://gist.github.com/Cheesebaron/9876783
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Java.Lang.Object ToJavaObject<TObject>(this TObject value)
        {
            if (Equals(value, default(TObject)) && !typeof(TObject).IsValueType)
                return null;

            var holder = new JavaHolder(value);

            return holder;
        }

        #endregion

    }
}