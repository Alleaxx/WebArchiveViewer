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
    public interface ISnapshot
    {
        FileInfo File { get; }

        string SourceURI { get; }
        DateTime ReceivingDate { get; }
        ArchiveLink[] Links { get; }
        RulesControl RulesControl { get; }


        void Save();
        void Save(string path);
        void Save(string path, IEnumerable<ArchiveLink> links);
        void Save(SaveMode mode,string path);


        IEnumerable<ICategory> GetCategories();
        ViewOptions ViewOptions { get; }
    }

    //Сайт со ссылками из архива на указанную дату
    [Serializable]
    public class SiteSnapshot : NotifyObj, ISnapshot
    {
        //Сведения о файлах
        public string FolderHtmlSavePath
        {
            get => folderHtmlSavePath ?? $"{Directory.GetCurrentDirectory()}\\Ссылки";
            set
            {
                folderHtmlSavePath = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FolderHtmlSaveName));
            }
        }
        private string folderHtmlSavePath;
        [JsonIgnore]
        public string FolderHtmlSaveName => new DirectoryInfo(FolderHtmlSavePath).Name;

        [JsonIgnore]
        public string FilePath { get; set; }
        [JsonIgnore]
        public FileInfo File => string.IsNullOrEmpty(FilePath) ? null : new FileInfo(FilePath);
        [JsonIgnore]
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


        [JsonIgnore]
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

        [JsonIgnore]
        public ICommand SaveHTMLCommand { get; private set; }
        private void SaveHTML(object obj)
        {
            SaveHTMLView saveHTMLView = new SaveHTMLView(this);
            SaveHTMLWindow w = new SaveHTMLWindow(saveHTMLView);
            w.Show();
        }




        //Сведения о запросе
        public string Request { get; set; }
        public string SourceURI { get; set; }
        public DateTime ReceivingDate { get; set; }


        public ViewOptions ViewOptions { get; set; }
        public RulesControl RulesControl { get; set; }
        public ArchiveLink[] Links { get; set; }




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

            SaveHTMLCommand = new RelayCommand(SaveHTML);
            UpdateCategoriesCommand = new RelayCommand(UpdateCategories);
            LoadNamesCommand = new RelayCommand(LoadNames);
            ClearProgressCommand = new RelayCommand(ClearProgress);
        }


        [JsonIgnore]
        public ICommand OpenLinkCommand { get; private set; }
        private bool IsCorrectLink(object obj) => obj is string link && !string.IsNullOrEmpty(link);
        private void OpenLink(object obj)
        {
            if (obj is string link)
            {
                System.Diagnostics.Process.Start(link);
            }
        }


        [JsonIgnore]
        public ICommand ClearProgressCommand { get; private set; }
        private void ClearProgress(object obj)
        {
            foreach (var link in Links)
            {
                link.HtmlFilePath = null;
            }
        }

        [JsonIgnore]
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


        [JsonIgnore]
        public ICommand UpdateCategoriesCommand { get; private set; }
        private void UpdateCategories(object obj)
        {
            ViewOptions.LoadCategories(this);
        }


        public void Save()
        {
            Save(FilePath, Links);
        }
        public void Save(string path)
        {
            Save(path, Links);
        }
        public void Save(SaveMode mode,string path)
        {
            var links = ViewOptions.GetFilteredLinks(this, mode);
            Save(path, links);
        }
        public void Save(string path, IEnumerable<ArchiveLink> links)
        {
            FilePath = path;
            LastSaveDate = DateTime.Now;

            var copy = MemberwiseClone() as SiteSnapshot;
            copy.Links = links.ToArray();

            IFileDialog fileDialog = new FileDialog();
            fileDialog.SaveFile(path, copy);
        }


        private void UpdateLinkCategories()
        {
            foreach (IArchLink link in Links)
            {
                string cateName = RulesControl.CheckLink(link);
                link.Category = cateName;
            }
        } 
        public IEnumerable<ICategory> GetCategories()
        {
            CreateRulesIfNull();

            var categoriesFound = new List<ICategory>();
            UpdateLinkCategories();

            foreach (IArchLink link in Links)
            {
                string cateName = link.Category;
                ICategory cate = categoriesFound.Find(o => o.Name == cateName);

                if (cate == null)
                    categoriesFound.Add(new Category(cateName));
                else if (cate != null)
                    cate.ItemsAmount++;
            }
            return categoriesFound;
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

    public class SaveData
    {

    }
}
