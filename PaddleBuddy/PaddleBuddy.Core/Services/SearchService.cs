using System;
using System.Collections.Generic;
using System.Linq;
using PaddleBuddy.Core.Models;
using PaddleBuddy.Core.Models.Map;

namespace PaddleBuddy.Core.Services
{
    public class SearchService
    {
        public List<SearchItem> Data { get; set; }

        public SearchService()
        {
            Data = new List<SearchItem>();
        }

        public List<SearchItem> Filter(string searchText)
        {
            searchText = searchText.ToLower();
            var filteredList = new List<SearchItem>();
            if (string.IsNullOrWhiteSpace(searchText)) return filteredList;
            try
            {
                filteredList = new List<SearchItem>(Data.ToList().Where(w => w.SearchString.ToLower().Contains(searchText)));
            }
            catch (Exception e)
            {
                LogService.Log(e.Message);
            }
            return filteredList;
        } 

        public void AddData(object[] arr)
        {
            foreach (var item in arr)
            {
                if (item.GetType() == typeof(River))
                {
                    var river = item as River;
                    if (river != null)
                    {
                        Data.Add(new SearchItem
                        {
                            SearchString = river.Name,
                            Item = river
                        });
                    }
                }
                else if (item.GetType() == typeof (Point))
                {
                    var point = item as Point;
                    if (point != null)
                    {
                        Data.Add(new SearchItem
                        {
                            SearchString = point.Label ?? point.Id.ToString(),
                            Item = point
                        });
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            Data.Sort();
        }
    }
}
