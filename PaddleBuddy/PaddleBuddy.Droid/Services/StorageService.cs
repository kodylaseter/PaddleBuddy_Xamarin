﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PaddleBuddy.Core.Models.Map;
using PaddleBuddy.Core.Services;
using UnitsNet;
using UnitsNet.Extensions.NumberToSpeed;
using Path = System.IO.Path;

namespace PaddleBuddy.Droid.Services
{
    public class StorageService : ApiService
    {
        private static StorageService _storageService;
        private static object locker = new object();
        private string[] names = { "points", "rivers", "links" };

        public static StorageService GetInstance()
        {
            return _storageService ?? (_storageService = new StorageService());
        }

        public async Task Setup(bool sync = false)
        {
            if (sync)
            {
                await UpdateAll();
            }
            else if (HasData(names))
            {
                LogService.Log("Device has local data");
                DatabaseService.GetInstance().Points = JsonConvert.DeserializeObject<List<Point>>(ReadSerializedFromFile("points"));
                DatabaseService.GetInstance().Rivers = JsonConvert.DeserializeObject<List<River>>(ReadSerializedFromFile("rivers"));
                DatabaseService.GetInstance().Links = JsonConvert.DeserializeObject<List<Link>>(ReadSerializedFromFile("links"));
            }
            DatabaseService.GetInstance().UpdateIsReady();
            if (!DatabaseService.GetInstance().IsReady) await UpdateAll();
        }

        private async Task UpdateAll()
        {
            await UpdateRivers();
            await UpdateLinks();
            await UpdatePoints();
            SaveData();
        }

        public void SaveData()
        {
            var points = JsonConvert.SerializeObject(DatabaseService.GetInstance().Points);
            var rivers = JsonConvert.SerializeObject(DatabaseService.GetInstance().Rivers);
            var links = JsonConvert.SerializeObject(DatabaseService.GetInstance().Links);
            SaveSerializedToFile(points, "points");
            SaveSerializedToFile(rivers, "rivers");
            SaveSerializedToFile(links, "links");
        }

        public async Task<bool> UpdatePoints()
        {
            try
            {
                var resp = await ApiGetAsync("all_points");
                if (resp.Success)
                {
                    DatabaseService.GetInstance().Points = JsonConvert.DeserializeObject<List<Point>>(resp.Data.ToString());
                }
                else
                {
                    LogService.ExceptionLog("Failed to update points");
                    return false;
                }
            }
            catch (Exception e)
            {

                LogService.ExceptionLog(e.Message);
                LogService.ExceptionLog("Failed to update points");
                return false;
            }
            return true;
        }

        public async Task<bool> UpdateRivers()
        {
            try
            {
                var resp = await ApiGetAsync("all_rivers");
                if (resp.Success)
                {
                    DatabaseService.GetInstance().Rivers = JsonConvert.DeserializeObject<List<River>>(resp.Data.ToString());
                }
                else
                {
                    LogService.ExceptionLog("Failed to update rivers");
                    return false;
                }
            }
            catch (Exception e)
            {
                LogService.ExceptionLog(e.Message);
                LogService.ExceptionLog("Failed to update rivers");
                return false;
            }
            return true;
        }

        public async Task<bool> UpdateLinks()
        {
            try
            {
                var resp = await ApiGetAsync("all_links");
                if (resp.Success)
                {
                    DatabaseService.GetInstance().Links =
                        JsonConvert.DeserializeObject<List<Link>>(resp.Data.ToString(), new LinkConverter());
                }
                else
                {
                    LogService.Log("Failed to update links");
                    return false;
                }
            }
            catch (Exception e)
            {
                LogService.ExceptionLog(e.Message);
                LogService.ExceptionLog("Failed to update links");
                return false;
            }
            return true;
        }

        public void SaveSerializedToFile(string json, string name)
        {
            lock (locker)
            {
                var path = GetPath(name);
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                using (var file = File.Open(path, FileMode.Create, FileAccess.Write))
                {
                    using (var stream = new StreamWriter(file))
                    {
                        stream.Write(json);
                    }
                }
            }
        }

        public string ReadSerializedFromFile(string name)
        {
            return File.ReadAllText(GetPath(name));
        }

        public string GetPath(string name)
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), name + ".txt");
        }
        /// <summary>
        /// Checks whether the files exist by name first and then checks to make sure they meet a minimum size
        /// </summary>
        /// <param name="names"></param>
        /// <returns></returns>
        public bool HasData(string[] names)
        {
            foreach (var name in names)
            {
                if (!File.Exists(GetPath(name))) return false;
                var fileInfo = new FileInfo(GetPath(name));
                if (fileInfo.Length <= 0) return false;
            }
            return true;
        }
    }

    internal class LinkConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
			if (value == null)
			{
				serializer.Serialize(writer, null);
				return;
			}

            // find all properties with type 'int'
            var properties = value.GetType().GetProperties();

			writer.WriteStartObject();

			foreach (var property in properties)
			{
				writer.WritePropertyName(property.Name);
                if (property.Name == "Speed") {
                    try {
                        var speed = property.GetValue(value, null);
                        speed = ((Speed)speed).MetersPerSecond;
                        serializer.Serialize(writer, speed);
                    } catch (Exception) {
                        LogService.ExceptionLog("Issue serializing link");
                    }
				}
				else
				{
					serializer.Serialize(writer, property.GetValue(value, null));
                }
			}
			writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jo = JObject.Load(reader);
            var link = new Link();
            if (jo["id"] != null && (int) jo["id"] > -1)
            {
                link.Id = (int) jo["id"];
            }
            if (jo["begin"] != null && (int) jo["begin"] > -1)
            {
                link.Begin = (int) jo["begin"];
            }
            if (jo["end"] != null && (int) jo["end"] > -1)
            {
                link.End = (int) jo["end"];
            }
            if (jo["riverId"] != null && (int) jo["riverId"] > -1)
            {
                link.RiverId = (int) jo["riverId"];
            }
            if (jo["speed"] != null && (float)jo["speed"] > -1)
            {
                link.Speed = ((float)jo["speed"]).MetersPerSecond();
            }
            return link;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Link);
        }
    }
}