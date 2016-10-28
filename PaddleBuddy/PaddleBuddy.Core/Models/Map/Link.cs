namespace PaddleBuddy.Core.Models.Map
{
    public class Link
    {
        public int Id { get; set; }
        public int Begin { get; set; }
        public int End { get; set; }
        public int RiverId { get; set; }
        public float Speed { get; set; }
    }
}
