namespace PaddleBuddy.Core.Models.Map
{
    public class Point
    {
        public int Id { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public int RiverId { get; set; }
        public string Label { get; set; }
        public bool IsLaunchSite { get; set; }

        public override string ToString()
        {
            return "id: " + Id + ", lat: " + Lat;
        }

        public static Point Add(Point a, Point b)
        {
            return new Point
            {
                Lat = a.Lat + b.Lat,
                Lng = a.Lng + b.Lng
            };
        }

        public static Point Subtract(Point a, Point b)
        {
            return new Point
            {
                Lat = a.Lat - b.Lat,
                Lng = a.Lng - b.Lng
            };
        }

        public static Point MultiplyByFraction(Point point, double fraction)
        {
            return new Point
            {
                Lat = point.Lat*fraction,
                Lng = point.Lng*fraction
            };
        }
    }
}
