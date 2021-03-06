﻿using PaddleBuddy.Core.Utilities;
using UnitsNet;

namespace PaddleBuddy.Core.Models.Map
{
    public class Point
    {
        public int Id { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public int RiverId { get; set; }
        public string Label { get; set; }
        public bool IsLaunchSite { get; set; }

        /// <summary>
        /// In seconds
        /// Current time of day converted to seconds
        /// </summary>
        public Duration Time { get; set; }

        /// <summary>
        /// In meters per second
        /// </summary>
        public Speed Speed { get; set; }

        public override string ToString()
        {
            return "id: " + Id + ", lat: " + Lat;
        }

        public SearchItem ToSearchItem()
        {
            return new SearchItem
            {
                Id = PBUtilities.GetNextId(),
                Item = this,
                SearchString = Label,
                Title = Label
            };
        }
    }
}
