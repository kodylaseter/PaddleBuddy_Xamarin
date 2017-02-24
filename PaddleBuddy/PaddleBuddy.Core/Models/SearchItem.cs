using PaddleBuddy.Core.Utilities;

namespace PaddleBuddy.Core.Models
{
    public class SearchItem
    {
        public long Id { get; set; }
        public string SearchString { get; set; }
        public object Item { get; set; }

        public SearchItem()
        {
            Id = PBUtilities.GetNextId();
        }
    }
}
