namespace PaddleBuddy.Core.Models.LinqModels
{
    public class LinkPoint
    {
        public int Id { get; set; }
        public int Begin { get; set; }
        public int End { get; set; }
        public double Speed { get; set; }
        public int RiverId { get; set; }
        public double BeginLat { get; set; }
        public double BeginLng { get; set; }
        public double EndLat { get; set; }
        public double EndLng { get; set; }

        public LinkPoint(int id, int begin, int end, double speed, int riverId, double beginLat, double beginLng, double endLat, double endLng)
        {
            Id = id;
            Begin = begin;
            End = end;
            Speed = speed;
            RiverId = riverId;
            BeginLat = beginLat;
            BeginLng = beginLng;
            EndLat = endLat;
            EndLng = endLng;
        }
    }
}
