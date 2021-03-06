﻿using System;
using System.Collections.Generic;
using System.Linq;
using PaddleBuddy.Core.Models;
using PaddleBuddy.Core.Models.Map;

namespace PaddleBuddy.Core.Services
{
    public class SearchService
    {
        public List<SearchItem> OriginalData { get; set; }
        public List<SearchItem> Items { get; set; } 

        public SearchService()
        {
            OriginalData = new List<SearchItem>();
            Items = new List<SearchItem>();
        }

        public List<SearchItem> Filter(string searchText)
        {
            searchText = searchText.Trim().ToLower();
            var filteredList = new List<SearchItem>();
            if (searchText == null)
            {
                throw new NotImplementedException();
            }
            try
            {
                filteredList = new List<SearchItem>(OriginalData.ToList().Where(w => w.SearchString.ToLower().Contains(searchText)));
            }
            catch (Exception e)
            {
                LogService.ExceptionLog(e.Message);
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
                        OriginalData.Add(river.ToSearchItem());
                    }
                }
                else if (item.GetType() == typeof (Point))
                {
                    var point = item as Point;
                    if (point != null)
                    {
                        OriginalData.Add(point.ToSearchItem());
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            OriginalData.Sort();
            Items = OriginalData.ToList();
        }

        public void Clear()
        {
            Items.Clear();
            OriginalData.Clear();
        }

        public SearchItem GetItem(int index)
        {
            return Items.Count > index ? Items[index] : null;
        }
    }
}
