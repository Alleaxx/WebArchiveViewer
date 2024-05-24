using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WebArchive.Data
{
    public class FileHelper
    {
        private JsonSerializerSettings SerializerSettings { get; set; }
        public FileHelper()
        {
            SerializerSettings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
            };
        }

        public string OpenReadText(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                byte[] array = new byte[fs.Length];
                fs.Read(array, 0, array.Length);
                string textFromFile = Encoding.UTF8.GetString(array);
                return textFromFile;
            }
        }

        public T OpenReadJson<T>(string path)
        {
            string text = OpenReadText(path);
            return JsonConvert.DeserializeObject<T>(text, SerializerSettings);
        }

        public bool SaveFile<T>(string path, T obj)
        {
            string json = JsonConvert.SerializeObject(obj, SerializerSettings);
            File.WriteAllText(path, json, new UTF8Encoding(false));
            return true;
        }
    }
}
