namespace PaddleBuddy.Core.Models.Map
{
    public class Link
    {
        public int Id { get; set; }
        public int Begin { get; set; }
        public int End { get; set; }
        public int RiverId { get; set; }
        /// <summary>
        /// in meters per second
        /// </summary>
        public float Speed { get; set; } 
    }
}
