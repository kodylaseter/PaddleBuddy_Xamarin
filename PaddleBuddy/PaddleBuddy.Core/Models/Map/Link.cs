using UnitsNet;

namespace PaddleBuddy.Core.Models.Map
{
    public class Link
    {
        public int Id { get; set; }
        public int Begin { get; set; }
        public int End { get; set; }
        public int RiverId { get; set; }
        public Speed Speed { get; set; }

        public override string ToString()
        {
            return string.Format("[Link: Id={0}, Begin={1}, End={2}, RiverId={3}, Speed={4}]", Id, Begin, End, RiverId, Speed);
        }
    }
}
