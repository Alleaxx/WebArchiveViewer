using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace WebArchiveViewer
{

    //Расширенная версия снапшота 
    public class SiteSnapshot : Snapshot
    {
        public FileInfo File => string.IsNullOrEmpty(FilePath) ? null : new FileInfo(FilePath);
        public string Status => string.IsNullOrEmpty(FilePath) ? "Снапшот из архива, не сохранен" : "Снапшот сохранен";



        public DateTime LastSaveDate
        {
            get => lastSaveDate;
            set
            {
                lastSaveDate = value;
                OnPropertyChanged();
            }
        }
        private DateTime lastSaveDate;


        public ICommand SelectSaveFolderCommand { get; private set; }
        private void SelectFolderSave(object obj)
        {
            IFileDialog dialog = new FileDialog();
            var folder = dialog.SelectFolder();
            if (folder != null && folder.Exists)
            {
                FolderHtmlSavePath = folder.FullName;
            }
        }

        public ICommand SaveHTMLCommand { get; private set; }
        private void SaveHTML(object obj)
        {
            SaveHTMLView saveHTMLView = new SaveHTMLView(this);
            SaveHTMLWindow w = new SaveHTMLWindow(saveHTMLView);
            w.Show();
        }



        public SiteSnapshot()
        {

        }
        public SiteSnapshot(string request, string source, IEnumerable<ArchiveLink> links) : this()
        {
            ReceivingDate = DateTime.Now;
            Request = request;
            SourceURI = source;
            Links = links.ToArray();
            ViewOptions = new ViewOptions(this);
        }
        protected override void InitCommands()
        {
            OpenLinkCommand = new RelayCommand(OpenLink, IsCorrectLink);
            SelectSaveFolderCommand = new RelayCommand(SelectFolderSave);
            SaveSnapFileCommand = new RelayCommand(SaveSnapFile);

            SaveHTMLCommand = new RelayCommand(SaveHTML);
            UpdateCategoriesCommand = new RelayCommand(UpdateCategories);
            LoadNamesCommand = new RelayCommand(LoadNames);
            ClearProgressCommand = new RelayCommand(ClearProgress);
        }


        public ICommand OpenLinkCommand { get; private set; }
        private bool IsCorrectLink(object obj) => obj is string link && !string.IsNullOrEmpty(link);
        private void OpenLink(object obj)
        {
            if (obj is string link)
            {
                System.Diagnostics.Process.Start(link);
            }
        }


        public ICommand ClearProgressCommand { get; private set; }
        private void ClearProgress(object obj)
        {
            foreach (var link in Links)
            {
                link.HtmlFilePath = null;
            }
        }

        public ICommand LoadNamesCommand { get; private set; }
        private async void LoadNames(object obj)
        {
            var links = Links.Where(l => l.LoadNameCommand.CanExecute(null) && l.Name == ArchiveLink.DefaultName);
            Task loading = new Task(() => LoadNamesProcess(links));
            loading.Start();
            await loading;
        }
        private void LoadNamesProcess(IEnumerable<IArchLink> links)
        {
            var linksArr = links.ToArray();
            int latencyMs = 1000;

            for (int i = 0; i < linksArr.Length; i++)
            {
                var link = linksArr[i];
                System.Threading.Thread.Sleep(latencyMs);
                link.LoadNameCommand.Execute(null);
            }
        }


        public ICommand UpdateCategoriesCommand { get; private set; }
        private void UpdateCategories(object obj)
        {
            ViewOptions.LoadCategories(this);
        }


        //Сохранение
        public ICommand SaveSnapFileCommand { get; private set; }
        private void SaveSnapFile(object obj)
        {
            SaveMode saveMode = GetSaveMode(obj);
            string path = GetSavingPath(saveMode);

            if (path != null)
            {
                Save(saveMode, path);
            }

            //Из object в режим сохранения
            SaveMode GetSaveMode(object objMode)
            {
                SaveMode modeS = SaveMode.AllShowed;
                if (objMode != null && Enum.TryParse(objMode.ToString(), out SaveMode mode))
                    modeS = mode;
                return modeS;
            }
        }
        private string GetSavingPath(SaveMode mode)
        {
            if (mode == SaveMode.AllDefaultPath && !string.IsNullOrEmpty(FilePath))
            {
                return FilePath;
            }
            else
            {
                int linksCount = Links.Length;

                var file = new FileDialog().Save($"Снапшот - {linksCount}");
                if (file != null)
                    return file.FullName;
                else
                    return null;
            }
        }


        public void Save()
        {
            SaveAsync(FilePath, Links);
        }
        public void Save(string path)
        {
            SaveAsync(path, Links);
        }
        public void Save(SaveMode mode,string path)
        {
            var links = ViewOptions.GetFilteredLinks(mode);
            SaveAsync(path, links);
        }
        public async void SaveAsync(string path, IEnumerable<ArchiveLink> links)
        {
            FilePath = path;
            LastSaveDate = DateTime.Now;

            var copy = new Snapshot(this);

            IFileDialog fileDialog = new FileDialog();
            await Task.Run(() => fileDialog.SaveFile(path, copy));
        }


        //Категории, их обновление
        private void UpdateLinkCategories()
        {
            foreach (IArchLink link in Links)
            {
                string category = RulesControl.CheckLink(link);
                link.Category = category;
            }
        } 
        public CategoriesInfo GetCategories()
        {
            CreateRulesIfNull();
            UpdateLinkCategories();

            var mainRule = RulesControl.Rule;
            var cates = RulesControl.Rules.Select(r => new Category(r)).ToList();
            var cateDictionary = Category.GetDictionary(cates);

            CountElementsInCates(cates, cateDictionary);
            foreach (var cate in cates)
            {
                cate.RemoveNullInnerCates();
            }

            return new CategoriesInfo(cates, cateDictionary);
        }
        private void CountElementsInCates(List<Category> categories, Dictionary<string, ICategory> dictionary)
        {
            foreach (IArchLink link in Links)
            {
                string cateName = link.Category;

                if (!dictionary.ContainsKey(cateName))
                {
                    var newCate = new Category(cateName);
                    dictionary.Add(newCate.Name, newCate);
                    categories.Add(newCate);
                }
                dictionary[cateName].ItemsAmount++;
            }
        }


        private void CreateRulesIfNull()
        {
            if (RulesControl == null)
            {
                RulesControl = new RulesControl();
                RulesControl.AddRules(new RumineRules());
            }
        }
    }
}
