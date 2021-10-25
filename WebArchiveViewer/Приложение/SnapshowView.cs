using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WebArchiveViewer
{
    public class SnapshotView : NotifyObj
    {
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
            CreateCommands();
            RulesView = new RulesView(Current);
            ViewOptions = new ViewOptions(Current);
        }
        private void CreateCommands()
        {
            if (Current != null)
            {
                OpenLinkCommand = new RelayCommand(OpenLink, IsCorrectLink);
                SelectSaveFolderCommand = new RelayCommand(SelectFolderSave);
                SaveSnapFileCommand = new RelayCommand(Save);

                SaveHTMLCommand = new RelayCommand(SaveHTML);
                UpdateCategoriesCommand = new RelayCommand(UpdateCategories);
                LoadNamesCommand = new RelayCommand(LoadNames);
                ClearProgressCommand = new RelayCommand(ClearProgress);
                OpenOptionsCommand = new RelayCommand(OpenOptions);
            }
            else
            {
                OpenLinkCommand = new RelayCommand(Nothing, NotAvailable);
                SelectSaveFolderCommand = new RelayCommand(Nothing, NotAvailable);
                SaveSnapFileCommand = new RelayCommand(Nothing, NotAvailable);

                SaveHTMLCommand = new RelayCommand(Nothing, NotAvailable);
                UpdateCategoriesCommand = new RelayCommand(Nothing, NotAvailable);
                LoadNamesCommand = new RelayCommand(Nothing, NotAvailable);
                ClearProgressCommand = new RelayCommand(Nothing, NotAvailable);
                OpenOptionsCommand = new RelayCommand(Nothing, NotAvailable);

                void Nothing(object obj)
                {

                }
                bool NotAvailable(object obj)
                {
                    return false;
                }
            }
        }

        //----
        public ICommand SelectSaveFolderCommand { get; private set; }
        public ICommand SaveHTMLCommand { get; private set; }
        public ICommand OpenLinkCommand { get; private set; }
        public ICommand ClearProgressCommand { get; private set; }
        public ICommand LoadNamesCommand { get; private set; }
        public ICommand UpdateCategoriesCommand { get; private set; }
        public ICommand SaveSnapFileCommand { get; private set; }
        public ICommand OpenOptionsCommand { get; private set; }
        //-----


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
        private void SelectFolderSave(object obj)
        {
            IFileDialog dialog = new FileDialog();
            var folder = dialog.SelectFolder();
            if (folder != null && folder.Exists)
            {
                Current.FolderHtmlSavePath = folder.FullName;
            }
        }


        public void Save(SaveMode mode)
        {
            Save(mode);
        }
        public async void Save(object obj)
        {
            IFileDialog fileDialog = new FileDialog();
            SaveMode mod = GetSaveMode(obj);

            Snapshot saveCopy = new Snapshot(Current, ViewOptions.GetFilteredLinks(mod));
            int linksCount = saveCopy.Links.Count();

            if (Current.FilePath != null && mod == SaveMode.AllDefaultPath)
            {
                await Task.Run(() => fileDialog.SaveFile(Current.FilePath, saveCopy));
                SaveComplete(Current.FilePath);
            }
            else
            {
                var file = new FileDialog().Save($"Снапшот - {linksCount}");
                if(file != null)
                {
                    await Task.Run(() => fileDialog.SaveFile(file.FullName, saveCopy));
                    SaveComplete(file.FullName);
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
        private void SaveHTML(object obj)
        {
            SaveHTMLView saveHTMLView = new SaveHTMLView(this);
            SaveHTMLWindow w = new SaveHTMLWindow(saveHTMLView);
            w.Show();
        }
        private void OpenOptions(object obj)
        {
            PathOptionsWindow w = new PathOptionsWindow(this);
            w.ShowDialog();
        }
        private void ClearProgress(object obj)
        {
            foreach (var link in Current.Links)
            {
                link.HtmlFilePath = null;
            }
        }
        

        //Загрузка имен
        private async void LoadNames(object obj)
        {
            var links = Current.Links.Select(l => new LoadLink(l)).Where(l => l.LoadNameCommand.CanExecute(null) && l.Link.Name == ArchiveLink.DefaultName);
            Task loading = Task.Run(() => LoadNamesProcess(links));
            await loading;
        }
        private void LoadNamesProcess(IEnumerable<LoadLink> links)
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



        public void UpdateCategories(object obj)
        {
            ViewOptions.LoadCategories(Current);
        }
    }

    public class LoadLink
    {
        public readonly ArchiveLink Link;
        public LoadLink(ArchiveLink link)
        {
            Link = link;
        }
        //Загрузка имени страницы
        private string[] ForbiddenCodes = new string[] { "404", "502", "302" };
        private bool IsLoadNameAvailable(object obj) => !ForbiddenCodes.Contains(Link.StatusCode);

        public RelayCommand LoadNameCommand { get; private set; }
        private async void LoadNameAsync(object obj)
        {
            //try
            //{
            //    WebClient client = new WebClient();
            //    string html = await client.DownloadStringTaskAsync(new Uri(Link.Link));
            //    Link.Name = await Task.Run(() => GetTitleFromHtml(html));
            //}
            //catch (WebException ex)
            //{
            //    Console.WriteLine(ex.Message);
            //}
        }


        public static string GetTitleFromHtml(string html)
        {
            string name = System.Text.RegularExpressions.Regex.Match(html, @"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\</title\>",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase).Groups["Title"].Value;
            return name;
        }
    }
}
