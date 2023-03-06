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
    public class SnapshotView : NotifyObject
    {
        public override string ToString()
        {
            return CurrentSnapshot != null ? $"Модель {CurrentSnapshot}" : "Нулевой снапшот";
        }

        public string Status
        {
            get
            {
                string status;
                if(CurrentSnapshot == null)
                {
                    return "Не открыт";
                }
                if (string.IsNullOrEmpty(CurrentSnapshot.FilePath))
                {
                    status = "Снапшот из архива, не сохранен";
                }
                else
                {
                    status = "Снапшот сохранен";
                }
                return status;
            }
        }
        public FileInfo File
        {
            get
            {
                if(CurrentSnapshot == null)
                {
                    return null;
                }
                return string.IsNullOrEmpty(CurrentSnapshot.FilePath) ? null : new FileInfo(CurrentSnapshot.FilePath);
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


        public Snapshot CurrentSnapshot { get; private set; }
        public RulesView RulesView { get; private set; }
        public ViewOptions ViewOptions { get; private set; }
        public HtmlLinkLoader LinkLoader { get; private set; }
        public DirectoryInfo SavingFolderHtmlContent => new DirectoryInfo(CurrentSnapshot.FolderHtmlSavePath);

        public SnapshotView(Snapshot snap)
        {
            CurrentSnapshot = snap;
            LastSaveDate = DateTime.Now;
            RulesView = new RulesView(this);
            ViewOptions = new ViewOptions(CurrentSnapshot);
            LinkLoader = new HtmlLinkLoader();
            CreateCommands();
        }
        private void CreateCommands()
        {
            OpenLinkCommand = new RelayCommand(OpenLink, IsCorrectLink);
            SelectSaveFolderCommand = new RelayCommand(SelectFolderSave, NotNull);
            SaveSnapFileCommand = new RelayCommand(Save, NotNull);

            OpenLoadOptionsCommand = new RelayCommand(OpenLoadHtml, NotNull);
            OpenOptionsCommand = new RelayCommand(OpenOptions, NotNull);

            UpdateCategoriesCommand = new RelayCommand(UpdateCategories, NotNull);
            ClearProgressCommand = new RelayCommand(ClearProgress, NotNull);
        }

        public ICommand OpenLinkCommand { get; private set; }

        public ICommand SelectSaveFolderCommand { get; private set; }
        public ICommand UpdateCategoriesCommand { get; private set; }
        public ICommand SaveSnapFileCommand { get; private set; }
        public ICommand ClearProgressCommand { get; private set; }


        public ICommand OpenOptionsCommand { get; private set; }
        public ICommand OpenLoadOptionsCommand { get; private set; }


        //Условия
        private bool NotNull(object obj)
        {
            return CurrentSnapshot != null;
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


        //Команды
        public void Save(SaveMode mode)
        {
            Save(mode as object);
        }
        private async void Save(object obj)
        {
            IFileDialog fileDialog = new FileDialog();
            SaveMode mod = GetSaveMode(obj);

            Snapshot saveCopy = CurrentSnapshot.CloneThis(ViewOptions.GetFilteredLinks(mod));

            string filePath = null;
            if (CurrentSnapshot.FilePath != null && File.Exists && mod == SaveMode.AllDefaultPath)
            {
                filePath = CurrentSnapshot.FilePath;
            }
            else
            {
                int linksCount = saveCopy.Links.Count();
                var file = new FileDialog().Save($"{CurrentSnapshot.ReceivingDate:yyyy-MM-dd} снапшот - {linksCount} ссылок");
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
                CurrentSnapshot.FilePath = path;
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
                CurrentSnapshot.FolderHtmlSavePath = folder.FullName;
                OnPropertyChanged(nameof(SavingFolderHtmlContent));
            }
        }
        private void ClearProgress(object obj)
        {
            foreach (var link in CurrentSnapshot.Links)
            {
                link.HtmlFilePath = null;
            }
        }
        public void UpdateCategories(object obj)
        {
            ViewOptions.LoadCategories(CurrentSnapshot);
        }
    }
}
