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

        public static bool HasData(string[] names)
        {
            return names.All(a => File.Exists(GetPath(a)));
        }
    }
}