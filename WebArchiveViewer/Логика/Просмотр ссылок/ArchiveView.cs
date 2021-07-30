using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

using Newtonsoft.Json;

namespace WebArchiveViewer
{

    //Представление просмотра ссылок с архива
    class ArchiveView : NotifyObj
    {
        private IFileDialog FileDialog { get; set; }

        public string Status
        {
            get => status;
            set
            {
                status = value;
                OnPropertyChanged();
            }
        }
        private string status;


        //Инициализация
        public ArchiveView()
        {
            sortSelected = Sorts.Last();
            groupSelected = Groups.Last();
            FileDialog = new FileDialog(".json", "JSON-файл (*.json) |*.json");

            Status = "Ожидание списка ссылок";

            LoadLinksCommand = new RelayCommand(LoadLinks, RelayCommand.IsTrue);
            OpenSnapshotCommand = new RelayCommand(OpenSnapshotFileExe, RelayCommand.IsTrue);
            SaveSnapFileCommand = new RelayCommand(SaveSnapFileExe, IsSnapshotOpened);
            CloseSnapCommand = new RelayCommand(CloseSnapExe, IsSnapshotOpened);

            SaveHTMLCommand = new RelayCommand(SaveHTML, IsSnapshotOpened);
            ClearProgressCommand = new RelayCommand(ClearProgress, IsSnapshotOpened);

            ShowRulesCommand = new RelayCommand(ShowRules, RelayCommand.IsTrue);
            UpdateCategoriesCommand = new RelayCommand(UpdateCategories, IsSnapshotOpened);
            ToggleCategoriesCommand = new RelayCommand(ToggleCategories, IsSnapshotOpened);
            LoadNamesCommand = new RelayCommand(LoadNames, IsSnapshotOpened);

            CreateRules();
        }
        private void CreateRules()
        {
            RulesControl = new RulesControl();
            RulesControl.AddRules(new RumineRules());
        }

        public FileInfo OpenedFile
        {
            get => openedFile;
            private set
            {
                openedFile = value;
                OnPropertyChanged();
            }
        }
        private FileInfo openedFile;


        //Снапшот адреса
        public SiteSnapshot CurrentSnapshot
        {
            get => currentSnapshot;
            set
            {
                currentSnapshot = value;
                OnPropertyChanged();

                if (value != null)
                {
                    UpdateSnapshot();
                }
                else
                {
                    Options = null;
                    Status = "Ссылки закрыты";
                }
            }
        }
        private SiteSnapshot currentSnapshot;
        public void SetSnapshot(SiteSnapshot snapshot)
        {
            CurrentSnapshot = snapshot;
        }
        private void UpdateSnapshot()
        {
            CategoriesFound = CurrentSnapshot.GetCategories(RulesControl);
            Options = new ViewOptionsLinks(this, CurrentSnapshot);
            UpdateList();
            Status = "Ссылки загружены";
        }




        //Открытие файла со ссылками
        private bool IsSnapshotOpened(object obj) => CurrentSnapshot != null;
        private void OpenSnapshotFileExe(object obj)
        {
            string path = FileDialog.Open();
            if (!string.IsNullOrEmpty(path))
            {
                CurrentSnapshot = FileDialog.OpenReadJson<SiteSnapshot>(path);

                Status = "Ссылки из файла";
                OpenedFile = new FileInfo(path);
            }
        }
        public RelayCommand OpenSnapshotCommand { get; private set; }




        //Сохранение ссылок в файл
        private enum SaveMode
        {
            allShowed,
            allFiltered,
            allNotFiltered,
            all,
            alldefault,
        }
        private void SaveSnapFileExe(object obj)
        {
            SaveMode saveMode = GetSaveMode(obj);
            string path = GetSavingPath(saveMode);

            if(path != null)
            {
                var links = GetSavingLinks(saveMode);
                var copy = GetSavingCopy(links);
                copy.Save(path);
            }
        }

        private SaveMode GetSaveMode(object obj)
        {
            SaveMode saveMode = SaveMode.allShowed;
            if (obj != null && Enum.TryParse(obj.ToString(), out SaveMode mode))
                saveMode = mode;
            return saveMode;

        }
        private IEnumerable<ArchiveLink> GetSavingLinks(SaveMode saveMode)
        {
            switch (saveMode)
            {
                case SaveMode.allShowed:
                    return LinksPager.PageNow.Elements;
                case SaveMode.all:
                    return CurrentSnapshot.Links;
                case SaveMode.allFiltered:
                    return CurrentSnapshot.Links.Where(l => Filter(l));
                case SaveMode.allNotFiltered:
                    return CurrentSnapshot.Links.Where(l => !Filter(l));
                default:
                    return CurrentSnapshot.Links;
            }
        }
        private string GetSavingPath(SaveMode mode)
        {
            if (mode == SaveMode.alldefault)
            {
                return OpenedFile.FullName;
            }
            else
            {
                int linksCount = currentSnapshot.Links.Length;
                string link = currentSnapshot.SourceURI;

                return FileDialog.Save($"{linksCount} - {link}");
            }
        }
        private SiteSnapshot GetSavingCopy(IEnumerable<ArchiveLink> links)
        {
            SiteSnapshot copy = new SiteSnapshot()
            {
                SourceURI = CurrentSnapshot.SourceURI,
                Date = DateTime.Now,
                ViewOptions = new ViewOptionsGetLinks()
                {
                    From = Options.From,
                    To = Options.To,
                    Search = Options.Search,
                    Codes = Options.Codes,
                    Types = Options.Types,
                },
                Links = links.ToArray()
            };
            return copy;
        }
        public RelayCommand SaveSnapFileCommand { get; private set; }


        public RelayCommand CloseSnapCommand { get; private set; }
        private void CloseSnapExe(object obj)
        {
            CurrentSnapshot = null;
            LinksPager = null;
        }
        
        
        public RelayCommand ClearProgressCommand { get; private set; }
        private void ClearProgress(object obj)
        {
            foreach (var link in CurrentSnapshot.Links)
            {
                link.ActualState = null;
            }
        }
        
        
        public RelayCommand LoadNamesCommand { get; private set; }
        private async void LoadNames(object obj)
        {
            var links = CurrentSnapshot.Links.Where(l => l.LoadNameCommand.CanExecute(null) && l.Name == ArchiveLink.DefaultName);
            Task loading = new Task(() => LoadNamesProcess(links));
            loading.Start();
            await loading;
        }
        private void LoadNamesProcess(IEnumerable<ArchiveLink> links)
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




        public RelayCommand ShowRulesCommand { get; private set; }
        private void ShowRules(object obj)
        {
            RulesWindow window = new RulesWindow(RulesControl);
            window.ShowDialog();
        }

        public RelayCommand UpdateCategoriesCommand { get; private set; }
        private void UpdateCategories(object obj)
        {
            CategoriesFound = CurrentSnapshot.GetCategories(RulesControl);
        }


        public RelayCommand LoadLinksCommand { get; private set; }
        private void LoadLinks(object obj)
        {
            LoadWindow window = new LoadWindow();
            window.ShowDialog();
        }

        public RelayCommand SaveHTMLCommand { get; private set; }
        private void SaveHTML(object obj)
        {
            SaveHTMLWindow w = new SaveHTMLWindow();
            w.Show();
        }

        public RelayCommand ToggleCategoriesCommand { get; private set; }
        private void ToggleCategories(object obj)
        {
            bool newState = !CategoriesFound.First().Enabled;
            foreach (var cate in CategoriesFound)
            {
                cate.Enabled = newState;
            }
            UpdateList();
        }





        //Опции просмотра снапшота
        public ViewOptionsLinks Options
        {
            get => options;
            private set
            {
                options = value;
                OnPropertyChanged();
            }
        }
        private ViewOptionsLinks options;

        public Pager<ArchiveLink> LinksPager
        {
            get => linksPager;
            private set
            {
                linksPager = value;
                OnPropertyChanged();
            }
        }
        private Pager<ArchiveLink> linksPager;
        public int LinksFilteredAmount { get; private set; }

        //Обновление списка ссылок
        public void UpdateList()
        {
            var links = currentSnapshot.Links.Where(l => Filter(l));
            if (SortSelected != null)
            {
                switch (SortSelected.Value)
                {
                    case "Имя":
                        links = links.OrderBy(l => l.LinkSource);
                        break;
                    case "Дата":
                        links = links.OrderBy(l => l.Date);
                        break;
                    case "Тип":
                        links = links.OrderBy(l => l.MimeType);
                        break;
                }
            }

            LinksFilteredAmount = links.Count();
            OnPropertyChanged(nameof(LinksFilteredAmount));

            LinksPager = new Pager<ArchiveLink>(links, GroupSelected);
        }
        private bool Filter(object obj)
        {
            ArchiveLink link = obj as ArchiveLink;
            if (Options.From.Year > 1990 && (link.Date < Options.From || link.Date > Options.To))
                return false;
            if (!Options.Codes.ToList().Find(opt => opt.Value == link.StatusCode).Enabled)
                return false;
            if (!Options.Types.ToList().Find(opt => opt.Value == link.MimeType).Enabled)
                return false;
            if (!CategoriesFound.Find(opt => opt.Value == link.Category).Enabled)
                return false;
            if (Options.ShowLoaded.HasValue && ((!Options.ShowLoaded.Value && link.ActualState == "200") || (Options.ShowLoaded.Value && link.ActualState == null)))
                return false;
            if (!string.IsNullOrEmpty(Options.Search) && !link.LinkSource.Contains(Options.Search) && !link.Name.Contains(Options.Search))
                return false;
            return true;
        }



        //Сортировка и группировка
        public IRulesControl RulesControl { get; private set; }


        public List<OptionCount<string>> CategoriesFound
        {
            get => categoriesFound;
            set
            {
                categoriesFound = value;
                OnPropertyChanged();
            }
        }
        private List<OptionCount<string>> categoriesFound;

        public List<Option<string>> Groups { get; private set; } = new List<Option<string>>()
        {
            new Option<string>("Тип",false),
            new Option<string>("Код",false),
            new Option<string>("Категория",false),
            new Option<string>("Нет", true)
        };
        public List<Option<string>> Sorts { get; private set; } = new List<Option<string>>()
        {
            new Option<string>("Дата", false),
            new Option<string>("Имя", false),
            new Option<string>("Тип", false),
            new Option<string>("Нет", true)
        };


        //Выбранные сортировки и группировки
        public Option<string> SortSelected
        {
            get => sortSelected;
            set
            {
                sortSelected = value;
                OnPropertyChanged();
                UpdateList();
            }
        }
        private Option<string> sortSelected;


        public Option<string> GroupSelected
        {
            get => groupSelected;
            set
            {
                groupSelected = value;
                OnPropertyChanged();
                LinksPager.GroupSelected = value;
            }
        }
        private Option<string> groupSelected;
    }
}
