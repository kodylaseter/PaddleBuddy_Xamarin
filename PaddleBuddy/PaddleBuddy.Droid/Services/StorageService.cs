using System;
using System.IO;
using System.Linq;

namespace PaddleBuddy.Droid.Services
{
    public class StorageService : BaseAndroidService
    {
        public static void SaveSerializedToFile(string json, string name)
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

        public static string ReadSerializedFromFile(string name)
        {
            return File.ReadAllText(GetPath(name));
        }

        public static string GetPath(string name)
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), name + ".txt");
        }
        /// <summary>
        /// Checks whether the files exist by name first and then checks to make sure they meet a minimum size
        /// </summary>
        /// <param name="names"></param>
        /// <returns></returns>
        public static bool HasData(string[] names)
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
}