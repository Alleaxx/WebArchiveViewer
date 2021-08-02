using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchiveViewer
{
    interface IFileDialog
    {
        string SelectFolder();

        string Save();
        string Save(string name);
        bool SaveFile<T>(string path, T obj);


        string Open();
        string OpenReadText(string path);
        T OpenReadJson<T>(string path);
    }

    class FileDialog : IFileDialog
    {
        private string DefaultDirectory { get; set; }
        private string Extension { get; set; }
        private string Filter { get; set; }

        public FileDialog() : this(".json", "JSON-файл (*.json) |*.json")
        {

        }
        public FileDialog(string defaultExt, string filter)
        {
            Extension = defaultExt;
            Filter = filter;
        }

        public string SelectFolder()
        {
            Ookii.Dialogs.Wpf.VistaFolderBrowserDialog dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();

            if (dialog.ShowDialog() == true)
            {
                return dialog.SelectedPath;
            }
            return null;
        }

        public string Save() => Save("Имя файла");
        public string Save(string name)
        {
            Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.FileName = name;

            dialog.DefaultExt = Extension;

            if(!string.IsNullOrEmpty(DefaultDirectory))
                dialog.InitialDirectory = DefaultDirectory;

            dialog.Filter = Filter;

            if (dialog.ShowDialog() == true)
            {
                return dialog.FileName;
            }
            return null;
        }

        public string Open()
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();

            dialog.DefaultExt = Extension;

            if (!string.IsNullOrEmpty(DefaultDirectory))
                dialog.InitialDirectory = DefaultDirectory;

            dialog.Filter = Filter;

            if (dialog.ShowDialog() == true)
            {
                return dialog.FileName;
            }
            return null;
        }
        public string OpenReadText(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                byte[] array = new byte[fs.Length];
                fs.Read(array, 0, array.Length);
                string textFromFile = Encoding.Default.GetString(array);
                return textFromFile;
            }
        }

        public T OpenReadJson<T>(string path)
        {
            string text = OpenReadText(path);
            return JsonConvert.DeserializeObject<T>(text);
        }

        public bool SaveFile<T>(string path, T obj)
        {
            string json = JsonConvert.SerializeObject(obj, Formatting.Indented);
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                byte[] arr = Encoding.Default.GetBytes(json);
                fs.Write(arr, 0, arr.Length);
                return true;
            }
        }
    }
}
