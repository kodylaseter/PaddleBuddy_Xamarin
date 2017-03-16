using PaddleBuddy.Core.Utilities;

namespace PaddleBuddy.Core.Models.Map
{
    public class River
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public SearchItem ToSearchItem()
        {
            return new SearchItem
            {
                Id = PBUtilities.GetNextId(),
                Item = this,
                SearchString = Name,
                Title = Name
            };
        }
    }
}
