using PaddleBuddy.Core.Models.Map;

namespace PaddleBuddy.Core.Models.LinqModels
{
    public class PointWithNext
    {
        public Point Point { get; set; }
        public int Next { get; set; }

        public PointWithNext(Point p, int n)
        {
            Point = p;
            Next = n;
        }

        public PointWithNext() { }
    }
}
