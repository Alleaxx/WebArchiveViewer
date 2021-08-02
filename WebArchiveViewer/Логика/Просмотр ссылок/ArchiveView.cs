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
    public enum SaveMode
    {
        allShowed,
        allFiltered,
        allNotFiltered,
        all,
        alldefault,
    }

    //Представление просмотра ссылок с архива
    public class ArchiveView : NotifyObj
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
        public ISnapshot CurrentSnapshot
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
        private ISnapshot currentSnapshot;
        public void SetSnapshot(ISnapshot snapshot)
        {
            CurrentSnapshot = snapshot;
        }
        private void UpdateSnapshot()
        {
            UpdateCategories(null);

            if (Options != null)
                Options.Updated -= Options_Updated;
            Options = new ViewOptionsLinks(CurrentSnapshot);
            Options.Updated += Options_Updated;

            UpdateList();
            Status = "Ссылки загружены";
        }

        private void Options_Updated()
        {
            UpdateList();
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
        private void SaveSnapFileExe(object obj)
        {
            SaveMode saveMode = GetSaveMode(obj);
            string path = GetSavingPath(saveMode);

            if(path != null)
            {
                var links = GetSavingLinks(saveMode);
                var copy = currentSnapshot.GetSavingCopy(links ,options);
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
                default:
                    return CurrentSnapshot.GetLinks(saveMode, Filter);
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


        //Окна загрузки с веб-архива ссылок и HTML
        public RelayCommand LoadLinksCommand { get; private set; }
        private void LoadLinks(object obj)
        {
            LoadWindow window = new LoadWindow();
            window.ShowDialog();
        }

        public RelayCommand SaveHTMLCommand { get; private set; }
        private void SaveHTML(object obj)
        {
            SaveHTMLView saveHTMLView = new SaveHTMLView(currentSnapshot);
            SaveHTMLWindow w = new SaveHTMLWindow(saveHTMLView);
            w.Show();
        }


        //Обзор правил построения категорий и 
        public RelayCommand ShowRulesCommand { get; private set; }
        private void ShowRules(object obj)
        {
            RulesWindow window = new RulesWindow(this);
            window.ShowDialog();
        }

        public RelayCommand UpdateCategoriesCommand { get; private set; }
        private void UpdateCategories(object obj)
        {
            CategoriesFound = CurrentSnapshot.GetCategories();
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

        public IPager<ArchiveLink> LinksPager
        {
            get => linksPager;
            private set
            {
                linksPager = value;
                OnPropertyChanged();
            }
        }
        private IPager<ArchiveLink> linksPager;
        public int LinksFilteredAmount
        {
            get => linksFilteredAmount;
            private set
            {
                linksFilteredAmount = value;
                OnPropertyChanged();
            }
        }
        private int linksFilteredAmount;


        //Обновление списка ссылок
        public void UpdateList()
        {
            var links = currentSnapshot.Links.Where(l => Filter(l));
            links = SortLinks(links, sortSelected);
            LinksFilteredAmount = links.Count();

            LinksPager = new Pager<ArchiveLink>(links, GroupSelected);
        }
        private IEnumerable<ArchiveLink> SortLinks(IEnumerable<ArchiveLink> links, ISorting sort)
        {
            if (sort != null)
            {
                switch (sort.Name)
                {
                    case "Имя":
                        return links.OrderBy(l => l.Name);
                    case "Адрес":
                        return links.OrderBy(l => l.LinkSource);
                    case "Дата":
                        return links.OrderBy(l => l.Date);
                    case "Тип":
                        return links.OrderBy(l => l.MimeType);
                    default:
                        return links;
                }
            }
            else
                return links;
        }
        private bool Filter(object obj)
        {
            IArchLink link = obj as IArchLink;
            if (Options.From.Year > 1990 && (link.Date < Options.From || link.Date > Options.To))
                return false;
            if (!Options.Codes.ToList().Find(opt => opt.Value == link.StatusCode).Enabled)
                return false;
            if (!Options.Types.ToList().Find(opt => opt.Value == link.MimeType).Enabled)
                return false;
            if (!CategoriesFound.Where(cate => cate.Name == link.Category).First().Enabled)
                return false;
            if (Options.ShowLoaded.HasValue && ((!Options.ShowLoaded.Value && link.ActualState == "200") || (Options.ShowLoaded.Value && link.ActualState == null)))
                return false;
            if (!string.IsNullOrEmpty(Options.Search) && !link.LinkSource.Contains(Options.Search) && !link.Name.Contains(Options.Search))
                return false;
            return true;
        }


        public IEnumerable<ICategory> CategoriesFound
        {
            get => categoriesFound;
            set
            {
                categoriesFound = value;
                OnPropertyChanged();
            }
        }
        private IEnumerable<ICategory> categoriesFound;


        public IEnumerable<IGrouping> Groups { get; private set; } = new IGrouping[]
        {
            new Grouping("Имя ссылки", "Name", false),
            new Grouping("Тип","MimeType",false),
            new Grouping("Код","StatusCode",false),
            new Grouping("Категория","Category",false),
            new Grouping("Нет", null, true)
        };
        public IEnumerable<ISorting> Sorts { get; private set; } = new ISorting[]
        {
            new Sorting("Дата", null, false),
            new Sorting("Имя", l => l.Name, false),
            new Sorting("Адрес", l => l.LinkSource, false),
            new Sorting("Тип", l => l.MimeType, false),
            new Sorting("Нет", null, true)
        };


        //Выбранные сортировки и группировки
        public ISorting SortSelected
        {
            get => sortSelected;
            set
            {
                sortSelected = value;
                OnPropertyChanged();
                UpdateList();
            }
        }
        private ISorting sortSelected;


        public IGrouping GroupSelected
        {
            get => groupSelected;
            set
            {
                groupSelected = value;
                OnPropertyChanged();
                LinksPager.GroupSelected = value;
            }
        }
        private IGrouping groupSelected;
    }
}
