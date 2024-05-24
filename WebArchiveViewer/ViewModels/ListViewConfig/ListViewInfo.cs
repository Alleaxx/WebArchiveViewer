using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using WebArchive.Data;
using WebArchiveViewer.WpfUI.Collections;
using WebArchiveViewer.WpfUI.Commands;

namespace WebArchiveViewer.ViewModels
{
    /// <summary>
    /// Настройки отображения списка ссылок
    /// Фильтрация, сортировка, группировка, отображаемые колонки
    /// </summary>
    public class ListViewInfo : NotifyObject
    {
        public MainWindowViewModel MainModel { get; set; }

        private MimeType[] types;
        private StatusCode[] codes;
        private ICategory[] categories;
        private string search;
        private int linksFilteredAmount;

        public Snapshot Snapshot { get; private set; }

        public string Search
        {
            get => search;
            set => Set(ref search, value);
        }

        public bool SearchInverted
        {
            get => searchInverted;
            set=> Set(ref searchInverted, value);
        }
        private bool searchInverted;
        public bool ShowUniq
        {
            get => showUniq;
            set => Set(ref showUniq, value);
        }
        private bool showUniq;

        public string TagSelected
        {
            get => tagSelected;
            set => Set(ref tagSelected, value);
        }
        private string tagSelected;

        public bool NoBlacklisted
        {
            get => noBlacklisted;
            set => Set(ref noBlacklisted, value);
        }
        private bool noBlacklisted;
        public bool ShowDatesControl
        {
            get => showDatesControl;
            set => Set(ref showDatesControl, value);
        }
        private bool showDatesControl;

        public DateRange DateRange { get; private set; }

        public StatusCode[] Codes
        {
            get => codes;
            private set => Set(ref codes, value);
        }
        public MimeType[] Types
        {
            get => types;
            private set => Set(ref types, value);
        }
        public ICategory[] Categories
        {
            get => categories;
            private set => Set(ref categories, value);
        }
        public string NoLimitsTag { get; set; } = "без ограничений";
        public string[] AvailableTags => ArchiveLink.AllTags.Union(new string[] { NoLimitsTag }).ToArray();
        
        public bool IsSingleCode => Codes.Length == 1;
        public bool IsSingleType => Types.Length == 1;


        public ListViewColumnsInfo ColumnsInfo { get; private set; }
        public ListViewGroupSortsInfo GroupSortsInfo { get; private set; }
        private Dictionary<string, ICategory> CategoriesDictionary { get; set; }

        public bool? ShowOnlyLoaded { get; set; }
        public int LinksFilteredAmount
        {
            get => linksFilteredAmount;
            set
            {
                Set(ref linksFilteredAmount, value);
            }
        }
        private bool UpdateBlocking { get; set; }

        public ICommand ToggleCategoriesCommand { get; private set; }

        public ListViewInfo(MainWindowViewModel mainWindow, Snapshot snap)
        {
            UpdateBlocking = true;
            MainModel = mainWindow;
            search = string.Empty;
            tagSelected = NoLimitsTag;
            ShowOnlyLoaded = null;
            GroupSortsInfo = new ListViewGroupSortsInfo();
            GroupSortsInfo.OnUpdated += async () => await Update();
            PropertyChanged += ViewOptions_PropertyChanged;

            ToggleCategoriesCommand = new RelayCommand(ToggleCategories);

            ColumnsInfo = new ListViewColumnsInfo(this);
            ColumnsInfo
                .AddColumn("№", SortsEnum.LinkIndex, GroupsEnum.None, true)
                .AddColumn("Дата", SortsEnum.Date, GroupsEnum.None)
                .AddColumn("Время", SortsEnum.Date, GroupsEnum.None, true)
                .AddColumn("Код", SortsEnum.StatusCode, GroupsEnum.StatusCode)
                .AddColumn("Тип", SortsEnum.MimeType, GroupsEnum.MimeType)
                .AddColumn("Категория", SortsEnum.Category, GroupsEnum.Category)
                .AddColumn("Имя страницы", SortsEnum.PageName, GroupsEnum.PageName, true)
                .AddColumn("Ссылка", SortsEnum.LinkURL, GroupsEnum.LinkURL)
                .AddColumn("Веб-архив", SortsEnum.None, GroupsEnum.None)
                .AddColumn("Файл HTML", SortsEnum.None, GroupsEnum.None, true)
                .AddColumn("Теги", SortsEnum.Tag, GroupsEnum.None)
                .AddColumn("Превью", SortsEnum.None, GroupsEnum.None, true);



            Codes = Array.Empty<StatusCode>();
            Types = Array.Empty<MimeType>();
            SetSnapshot(snap);
            if (IsSingleCode)
            {
                ColumnsInfo.Columns[3].Hidden = true;
            }
            if (IsSingleType)
            {
                ColumnsInfo.Columns[4].Hidden = true;
            }
            if(snap.Links.Any(l => !string.IsNullOrEmpty(l.Name)))
            {
                ColumnsInfo.Columns[6].Hidden = false;
            }
            GroupSortsInfo.SortSelected = GroupSortsInfo.Sorts.First(s => s.Type == SortsEnum.Date);
            UpdateBlocking = false;
        }


        private bool Filter(object obj)
        {
            var link = obj as ArchiveLink;
            if (link.Date < DateRange.From || link.Date > DateRange.To)
            {
                return false;
            }
            if (!FilterTypes(link))
            {
                return false;
            }
            if (!FilterLoaded(link))
            {
                return false;
            }
            if (!FilterSearch(link))
            {
                return false;
            }
            if (!FilterUniq(link))
            {
                return false;
            }
            if (!FilterTags(link))
            {
                return false;
            }
            return true;
        }
        private bool FilterSearch(ArchiveLink link)
        {
            if (string.IsNullOrEmpty(Search))
            {
                return true;
            }

            bool linkSearchFound = link.LinkSource.Contains(Search);
            bool nameSearchFound = link.Name.Contains(Search);

            if (searchInverted)
            {
                return !linkSearchFound && !nameSearchFound;
            }
            return linkSearchFound || nameSearchFound;
        }
        private bool FilterTypes(ArchiveLink link)
        {
            var code = Codes.First(c => c.Code == link.StatusCode);
            var type = Types.First(c => c.Type == link.MimeType);
            if (CategoriesDictionary.ContainsKey(link.Category))
            {
                var cate = CategoriesDictionary[link.Category];

                return code.Enabled && type.Enabled && cate.Enabled;
            }
            return false;
        }
        private bool FilterLoaded(ArchiveLink link)
        {
            bool onlyLoaded = ShowOnlyLoaded.HasValue && ShowOnlyLoaded.Value;
            bool onlyUnloaded = ShowOnlyLoaded.HasValue && !ShowOnlyLoaded.Value;
            bool loaded = !string.IsNullOrEmpty(link.HtmlFilePath);
            bool unloaded = !loaded;
            if ((onlyUnloaded && loaded) || (onlyLoaded && unloaded))
            {
                return false;
            }
            return true;
        }
        private bool FilterUniq(ArchiveLink link)
        {
            return !ShowUniq || link.IsUniq;
        }
        private bool FilterTags(ArchiveLink link)
        {
            return string.IsNullOrEmpty(TagSelected) || TagSelected == NoLimitsTag || link.Tag == TagSelected;
        }


        public IEnumerable<ArchiveLink> GetFilteredLinks()
        {
            return GetFilteredLinks(SaveMode.AllFiltered);
        }
        public IEnumerable<ArchiveLink> GetFilteredLinks(SaveMode mode)
        {
            if(Snapshot == null || mode == SaveMode.OnlyRules)
            {
                return Array.Empty<ArchiveLink>();
            }

            var links = Snapshot.Links;
            switch (mode)
            {
                case SaveMode.All:
                    return links;
                case SaveMode.AllFiltered:
                    return links.Where(l => Filter(l));
                case SaveMode.AllNotFiltered:
                    return links.Where(l => !Filter(l));
                default:
                    return links;
            }
        }


        private async void ViewOptions_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            await Update();
        }
        private async void Dates_Updated(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            await Update();
        }
        private async void ToggleCategories(object obj = null)
        {
            var cates = CategoriesDictionary.Values;
            bool newState = !cates.First().Enabled;
            foreach (ICategory cate in cates)
            {
                cate.Enabled = newState;
            }
            await Update();
        }


        public void SetSnapshot(Snapshot snap)
        {
            if (!snap.Links.Any())
            {
                return;
            }
            Snapshot = snap;
            LoadDates(snap);
            LoadCodesTypes(snap);
            LoadCategories(snap);
        }
        private void LoadDates(Snapshot snap)
        {
            var orderedDate = snap.Links.OrderBy(l => l.Date);
            DateTime minDate = orderedDate.First().Date;
            DateTime maxDate = orderedDate.Last().Date;

            DateRange = new DateRange(minDate, minDate, maxDate, maxDate);
            DateRange.PropertyChanged += Dates_Updated;
        }
        private void LoadCodesTypes(Snapshot snap)
        {
            var codes = snap.Links.Select(l => l.StatusCode).Distinct();
            var types = snap.Links.Select(l => l.MimeType).Distinct();
            Codes = codes.Select(c => new StatusCode(c)).ToArray();
            Types = types.Select(t => new MimeType(t)).ToArray();
        }
        public void LoadCategories(Snapshot snap)
        {
            snap.UpdateCategories();
            var info = new CategoriesInfo(snap);
            Categories = new ICategory[1] { info.MainCategory };
            CategoriesDictionary = info.Dictionary;
            OnPropertyChanged(nameof(Categories));
        }


        protected async Task Update()
        {
            if (UpdateBlocking)
            {
                return;
            }
            await MainModel.UpdatePagerLinks();
        }
    }
}
