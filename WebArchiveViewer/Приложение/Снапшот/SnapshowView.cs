using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using WebArchive.Data;
namespace WebArchiveViewer
{
    public class SnapshotView : NotifyObj
    {
        public override string ToString()
        {
            return Current != null ? $"Модель {Current}" : "Нулевой снапшот";
        }

        public string Status
        {
            get
            {
                if(Current == null)
                {
                    return "Не открыт";
                }
                return string.IsNullOrEmpty(Current.FilePath) ? "Снапшот из архива, не сохранен" : "Снапшот сохранен";
            }
        }
        public FileInfo File
        {
            get
            {
                if(Current == null)
                {
                    return null;
                }
                return string.IsNullOrEmpty(Current.FilePath) ? null : new FileInfo(Current.FilePath);
            }
        }

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


        public Snapshot Current { get; private set; }
        public RulesView RulesView { get; private set; }
        public ViewOptions ViewOptions { get; private set; }
        public DirectoryInfo FolderHtmlSavePath => new DirectoryInfo(Current.FolderHtmlSavePath);

        public SnapshotView(Snapshot snap)
        {
            Current = snap;
            LastSaveDate = DateTime.Now;
            RulesView = new RulesView(this);
            ViewOptions = new ViewOptions(Current);
        }
        protected override void InitCommands()
        {
            base.InitCommands();
            OpenLinkCommand = new RelayCommand(OpenLink, IsCorrectLink);
            SelectSaveFolderCommand = new RelayCommand(SelectFolderSave, NotNull);
            SaveSnapFileCommand = new RelayCommand(Save, NotNull);

            OpenLoadOptionsCommand = new RelayCommand(OpenLoadHtml, NotNull);
            OpenOptionsCommand = new RelayCommand(OpenOptions, NotNull);

            UpdateCategoriesCommand = new RelayCommand(UpdateCategories, NotNull);
            ClearProgressCommand = new RelayCommand(ClearProgress, NotNull);
            LoadLinkNameCommand = new RelayCommand(LoadNameAsync, IsLoadNameAvailable);
        }

        public ICommand OpenLinkCommand { get; private set; }


        public ICommand SelectSaveFolderCommand { get; private set; }
        public ICommand UpdateCategoriesCommand { get; private set; }
        public ICommand SaveSnapFileCommand { get; private set; }
        public ICommand ClearProgressCommand { get; private set; }


        public ICommand OpenOptionsCommand { get; private set; }
        public ICommand OpenLoadOptionsCommand { get; private set; }


        private bool NotNull(object obj)
        {
            return Current != null;
        }
        private bool IsCorrectLink(object obj)
        {
            return obj is string link && !string.IsNullOrEmpty(link);
        }
        private void OpenLink(object obj)
        {
            if (obj is string link)
            {
                System.Diagnostics.Process.Start(link);
            }
        }



        public void Save(SaveMode mode)
        {
            Save(mode as object);
        }
        private async void Save(object obj)
        {
            IFileDialog fileDialog = new FileDialog();
            SaveMode mod = GetSaveMode(obj);

            Snapshot saveCopy = Current.CloneThis(ViewOptions.GetFilteredLinks(mod));

            string filePath = null;
            if (Current.FilePath != null && File.Exists && mod == SaveMode.AllDefaultPath)
            {
                filePath = Current.FilePath;
            }
            else
            {
                int linksCount = saveCopy.Links.Count();
                var file = new FileDialog().Save($"{Current.ReceivingDate:yyyy-MM-dd} снапшот - {linksCount} ссылок");
                if(file != null)
                {
                    filePath = file.FullName;
                }
            }
            await Save();


            async Task Save()
            {
                if (!string.IsNullOrEmpty(filePath))
                {
                    await Task.Run(() => fileDialog.SaveFile(filePath, saveCopy));
                    SaveComplete(filePath);
                }
            }
            void SaveComplete(string path)
            {
                Current.FilePath = path;
                LastSaveDate = DateTime.Now;
                OnPropertyChanged(nameof(File));
            }
            SaveMode GetSaveMode(object objMode)
            {
                SaveMode modeS = SaveMode.AllShowed;
                if (objMode != null && Enum.TryParse($"{objMode}", out SaveMode mode))
                {
                    modeS = mode;
                }
                return modeS;
            }
        }
        private void OpenLoadHtml(object obj)
        {
            LinksLoaderView saveHTMLView = new LinksLoaderView(this);
            SaveHTMLWindow w = new SaveHTMLWindow(saveHTMLView);
            w.Show();
        }
        private void OpenOptions(object obj)
        {
            PathOptionsWindow w = new PathOptionsWindow(this);
            w.ShowDialog();
        }
        private void SelectFolderSave(object obj)
        {
            IFileDialog dialog = new FileDialog();
            var folder = dialog.SelectFolder();
            if (folder != null && folder.Exists)
            {
                Current.FolderHtmlSavePath = folder.FullName;
                OnPropertyChanged(nameof(FolderHtmlSavePath));
            }
        }
        private void ClearProgress(object obj)
        {
            foreach (var link in Current.Links)
            {
                link.HtmlFilePath = null;
            }
        }
        public void UpdateCategories(object obj)
        {
            ViewOptions.LoadCategories(Current);
        }



        //Загрузка имени страницы
        private List<ArchiveLink> LoadingLinks { get; set; } = new List<ArchiveLink>(); 
        private readonly string[] ForbiddenCodes = new string[] { "404", "502", "302" };
        private bool IsLoadNameAvailable(object obj)
        {
            return obj is ArchiveLink Link && !ForbiddenCodes.Contains(Link.StatusCode) && Link.MimeType == "text/html" && !LoadingLinks.Contains(Link);
        }

        public ICommand LoadLinkNameCommand { get; private set; }
        private async void LoadNameAsync(object obj)
        {
            if (obj is ArchiveLink Link)
            {
                LoadingLinks.Add(Link);
                using (System.Net.Http.HttpClient client = new System.Net.Http.HttpClient())
                {
                    IHtmlLoader loader = new HttpClientHTMLoader(client, Link, new LoadHtmlOptions());
                    ILinkLoad res = await loader.LoadHtmlAsync();
                    await Task.Run(res.Process);
                }
                LoadingLinks.Remove(Link);
            }
        }
    }
}
