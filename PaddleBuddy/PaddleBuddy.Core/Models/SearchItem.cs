using System;
using PaddleBuddy.Core.Utilities;

namespace PaddleBuddy.Core.Models
{
    public class SearchItem : IComparable<SearchItem>
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string SearchString { get; set; }
        public object Item { get; set; }

        public SearchItem()
        {
            Id = PBUtilities.GetNextId();
        }

        public int CompareTo(SearchItem other)
        {
            return string.Compare(Title, other.Title, StringComparison.Ordinal);
        }
    }
}
