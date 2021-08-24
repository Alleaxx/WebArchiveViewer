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
    public class Snapshot : NotifyObj
    {
        public Snapshot()
        {

        }
    }
    //Сайт со ссылками из архива на указанную дату
    public class SiteSnapshot : Snapshot
    {
        //Сведения о файлах
        public string FolderHtmlSavePath
        {
            get => folderHtmlSavePath ?? $"{Directory.GetCurrentDirectory()}\\Ссылки";
            set
            {
                folderHtmlSavePath = value;
                OnPropertyChanged();
            }
        }
        private string folderHtmlSavePath;
        public string FilePath { get; set; }


        [JsonIgnore]
        public SnapshotInfo Info { get; private set; }


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
            Info = new SnapshotInfo(this);
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


        //Сохранение
        [JsonIgnore]
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
            Save(FilePath, Links);
        }
        public void Save(string path)
        {
            Save(path, Links);
        }
        public void Save(SaveMode mode,string path)
        {
            var links = ViewOptions.GetFilteredLinks(mode);
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


        //Категории, их обновление
        private void UpdateLinkCategories()
        {
            foreach (IArchLink link in Links)
            {
                string cateName = RulesControl.CheckLink(link);
                link.Category = cateName;
            }
        } 
        public CategoriesInfo GetCategories()
        {
            CreateRulesIfNull();
            UpdateLinkCategories();

            var cates = RulesControl.MainRules.Select(r => new Category(r)).ToList();
            var cateDictionary = Category.GetDictionary(cates);

            CountElementsInCates(cates, cateDictionary);
            cates.ForEach(c => c.RemoveNullInnerCates());

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

    public class SnapshotInfo
    {
        public override string ToString() => $"Информация, {Status}";

        public FileInfo File => new FileInfo(Snapshot.FilePath);


        public string Status => string.IsNullOrEmpty(Snapshot.FilePath) ? "Снапшот из архива, не сохранен" : "Снапшот сохранен";

        public SiteSnapshot Snapshot { get; private set; }
        public SnapshotInfo(SiteSnapshot snapshot)
        {
            Snapshot = snapshot;
        }
    }
}
