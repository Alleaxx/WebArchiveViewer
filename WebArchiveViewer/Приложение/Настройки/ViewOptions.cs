using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using WebArchive.Data;
namespace WebArchiveViewer
{
    //Настройки отображения ссылок
    //Фильтрация, сортировка, группировка, отображаемые колонки
    public class ViewOptions : NotifyObject
    {
        public event Action OnUpdated;

        private MimeType[] types;
        private StatusCode[] codes;
        private ICategory[] categories;
        private string search;
        private int linksFilteredAmount;

        //Для снапшота
        public Snapshot Snapshot { get; private set; }

        //Настройки
        public string Search
        {
            get => search;
            set
            {
                search = value;
                OnPropertyChanged();
            }
        }
        public DateRange DateRange { get; private set; }


        public StatusCode[] Codes
        {
            get => codes;
            private set
            {
                codes = value;
                OnPropertyChanged();
            }
        }
        public MimeType[] Types
        {
            get => types;
            private set
            {
                types = value;
                OnPropertyChanged();
            }
        }
        public ICategory[] Categories
        {
            get => categories;
            private set
            {
                categories = value;
                OnPropertyChanged();
            }
        }
        public ColumnsInfo ShowColumns { get; private set; }
        public ListViewOptions ListView { get; private set; }
        private Dictionary<string, ICategory> CategoriesDictionary { get; set; }

        public bool? ShowOnlyLoaded { get; set; }
        public int LinksFilteredAmount
        {
            get => linksFilteredAmount;
            set
            {
                linksFilteredAmount = value;
                UpdateBlocking = true;
                OnPropertyChanged();
                UpdateBlocking = false;
            }
        }
        private bool UpdateBlocking { get; set; }

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

            return linkSearchFound || !nameSearchFound;
        }
        private bool FilterTypes(ArchiveLink link)
        {
            var code = Codes.First(c => c.Code == link.StatusCode);
            var type = Types.First(c => c.Type == link.MimeType);
            var cate = CategoriesDictionary[link.Category];

            return code.Enabled && type.Enabled && cate.Enabled;
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


        public IEnumerable<ArchiveLink> GetFilteredLinks()
        {
            return GetFilteredLinks(SaveMode.AllFiltered);
        }
        public IEnumerable<ArchiveLink> GetFilteredLinks(SaveMode mode)
        {
            var links = Snapshot.Links;
            switch (mode)
            {
                case SaveMode.All:
                    return links;
                case SaveMode.AllFiltered:
                    return links.AsParallel().Where(l => Filter(l));
                case SaveMode.AllNotFiltered:
                    return links.AsParallel().Where(l => !Filter(l));
                default:
                    return links;
            }
        }


        public ViewOptions() : this(null)
        {

        }
        public ViewOptions(Snapshot snap)
        {
            search = "";
            ShowOnlyLoaded = null;
            ListView = new ListViewOptions();
            ListView.OnUpdated += Update;
            PropertyChanged += ViewOptions_PropertyChanged;

            ShowColumns = new ColumnsInfo();
            ShowColumns
                .AddColumn("№")
                .AddColumn("Дата")
                .AddColumn("Время")
                .AddColumn("Код")
                .AddColumn("Тип")
                .AddColumn("Категория")
                .AddColumn("Имя страницы")
                .AddColumn("Ссылка")
                .AddColumn("Веб-архив")
                .AddColumn("Загрузка");

            if(snap != null)
            {
                SetSnapshot(snap);
            }
        }
        private void ViewOptions_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Update();
        }
        private void Dates_Updated(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Update();
        }


        protected override void InitCommands()
        {
            ToggleCategoriesCommand = new RelayCommand(ToggleCategories);
        }
        public ICommand ToggleCategoriesCommand { get; private set; }
        private void ToggleCategories(object obj = null)
        {
            var cates = CategoriesDictionary.Values;
            bool newState = !cates.First().Enabled;
            foreach (ICategory cate in cates)
            {
                cate.Enabled = newState;
            }
            Update();
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


        protected void Update()
        {
            if (UpdateBlocking)
            {
                return;
            }
            OnUpdated?.Invoke();
        }
    }
}
