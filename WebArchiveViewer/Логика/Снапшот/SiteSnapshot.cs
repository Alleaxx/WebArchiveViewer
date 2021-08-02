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
        string SourceURI { get; }
        DateTime Date { get; }
        ViewOptionsGetLinks ViewOptions { get; set; }
        ArchiveLink[] Links { get; }
        RulesControl RulesControl { get; }
        void Save(string path);
        IEnumerable<ICategory> GetCategories();


        IEnumerable<ArchiveLink> GetLinks(SaveMode mode, Predicate<object> Filter);
        ISnapshot GetSavingCopy(IEnumerable<ArchiveLink> links, ViewOptionsLinks Options);
    }

    //Сайт со ссылками из архива на указанную дату
    [Serializable]
    public class SiteSnapshot : NotifyObj, ISnapshot
    {
        public string FilePath { get; set; }
        public string FileName { get; set; }

        public string FolderHtmlSavePath
        {
            get => folderHtmlSavePath;
            set
            {
                folderHtmlSavePath = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FolderHtmlSaveName));
            }
        }
        private string folderHtmlSavePath;

        [JsonIgnore]
        public string FolderHtmlSaveName => string.IsNullOrEmpty(FolderHtmlSavePath) ? null : new DirectoryInfo(FolderHtmlSavePath).Name;


        public string SourceURI { get; set; }
        public DateTime Date { get; set; }
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

        public ViewOptionsGetLinks ViewOptions { get; set; }
        public ArchiveLink[] Links { get; set; }
        public RulesControl RulesControl { get; set; }




        public SiteSnapshot()
        {

        }
        protected override void InitCommands()
        {
            OpenLinkCommand = new RelayCommand(OpenLink);
            SelectSaveFolderCommand = new RelayCommand(SelectFolderSave);
        }


        [JsonIgnore]
        public ICommand OpenLinkCommand { get; private set; }
        private void OpenLink(object obj)
        {
            if (obj is string link && link.StartsWith("http"))
            {
                System.Diagnostics.Process.Start(link);
            }
        }

        [JsonIgnore]
        public ICommand SelectSaveFolderCommand { get; private set; }
        private void SelectFolderSave(object obj)
        {
            IFileDialog dialog = new FileDialog();
            string path = dialog.SelectFolder();
            if (!string.IsNullOrEmpty(path))
            {
                FolderHtmlSavePath = path;
            }
        }

        
        public IEnumerable<ArchiveLink> GetLinks(SaveMode mode, Predicate<object> Filter)
        {
            switch (mode)
            {
                case SaveMode.all:
                    return Links;
                case SaveMode.allFiltered:
                    return Links.Where(l => Filter(l));
                case SaveMode.allNotFiltered:
                    return Links.Where(l => !Filter(l));
                default:
                    return Links;
            }
        }
        public ISnapshot GetSavingCopy(IEnumerable<ArchiveLink> links,ViewOptionsLinks Options)
        {
            ISnapshot copy = new SiteSnapshot()
            {
                FolderHtmlSavePath = folderHtmlSavePath,
                SourceURI = SourceURI,
                Date = DateTime.Now,
                ViewOptions = new ViewOptionsGetLinks()
                {
                    From = Options.From,
                    To = Options.To,
                    Search = Options.Search,
                    Codes = Options.Codes,
                    Types = Options.Types,
                },
                RulesControl = RulesControl,
                Links = links.ToArray()
            };
            return copy;
        }
        public void Save(string path)
        {
            FilePath = path;
            FileName = new FileInfo(path).Name;
            LastSaveDate = DateTime.Now;

            IFileDialog fileDialog = new FileDialog();
            fileDialog.SaveFile(path, this);
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
            CheckRules();

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

        private void CheckRules()
        {
            if (RulesControl == null)
            {
                RulesControl = new RulesControl();
                RulesControl.AddRules(new RumineRules());
            }
        }
    }
}
