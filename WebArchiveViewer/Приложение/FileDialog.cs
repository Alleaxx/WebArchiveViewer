﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchiveViewer
{
    public interface IFileDialog
    {
        DirectoryInfo SelectFolder();

        FileInfo Save();
        FileInfo Save(string name);
        bool SaveFile<T>(string path, T obj);


        FileInfo Open();
        string OpenReadText(string path);
        T OpenReadJson<T>(string path);
    }

    public class FileDialog : IFileDialog
    {
        public override string ToString()
        {
            return $"Файловый диалог: {Extension ?? "без расширения"}";
        }

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

        public DirectoryInfo SelectFolder()
        {
            Ookii.Dialogs.Wpf.VistaFolderBrowserDialog dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();

            if (dialog.ShowDialog() == true)
            {
                return new DirectoryInfo(dialog.SelectedPath);
            }
            return null;
        }

        public FileInfo Save()
        {
            return Save("Имя файла");
        }
        public FileInfo Save(string name)
        {
            Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = name,

                DefaultExt = Extension
            };

            if (!string.IsNullOrEmpty(DefaultDirectory))
            {
                dialog.InitialDirectory = DefaultDirectory;
            }

            dialog.Filter = Filter;

            if (dialog.ShowDialog() == true)
            {
                return new FileInfo(dialog.FileName);
            }
            return null;
        }

        public FileInfo Open()
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = Extension
            };

            if (!string.IsNullOrEmpty(DefaultDirectory))
                dialog.InitialDirectory = DefaultDirectory;

            dialog.Filter = Filter;

            if (dialog.ShowDialog() == true)
            {
                return new FileInfo(dialog.FileName);
            }
            return null;
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
            return JsonConvert.DeserializeObject<T>(text, new JsonSerializerSettings());
        }

        public bool SaveFile<T>(string path, T obj)
        {
            string json = JsonConvert.SerializeObject(obj, new JsonSerializerSettings());
            File.WriteAllText(path, json, new UTF8Encoding(false));
            return true;
        }
    }
}
