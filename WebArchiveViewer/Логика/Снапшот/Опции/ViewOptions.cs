using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WebArchiveViewer
{
    [Serializable]
    public class ViewOptions : NotifyObj
    {
        public event Action OnUpdated;

        //Для снапшота
        [JsonIgnore]
        public SiteSnapshot Snapshot { get; private set; }

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
        private string search = "";
        public DateRange DateRange { get; set; }

        public StatusCode[] Codes { get; set; }
        public MimeType[] Types { get; set; }
        public Category[] Categories { get; set; }
        private Dictionary<string, ICategory> CategoriesDictionary { get; set; }

        [JsonIgnore]
        public ListViewOptions ListView { get; private set; }

        public bool? ShowOnlyLoaded { get; set; } = null;


        private bool Filter(object obj)
        {
            IArchLink link = obj as IArchLink;
            var dates = DateRange;
            if (link.Date < dates.From || link.Date > dates.To)
                return false;

            if (!FilterTypes(link))
                return false;
            if (!FilterLoaded(link))
                return false;
            if (!FilterSearch(link))
                return false;
            return true;
        }
        private bool FilterSearch(IArchLink link)
        {
            bool linkSearchNotFound = !string.IsNullOrEmpty(Search) && !link.LinkSource.Contains(Search);
            bool nameSearchNotFound = !string.IsNullOrEmpty(Search) && !link.Name.Contains(Search);
            if (linkSearchNotFound && nameSearchNotFound)
                return false;

            return true;
        }
        private bool FilterTypes(IArchLink link)
        {
            var code = Codes.Where(c => c.Code == link.StatusCode).First();
            var type = Types.Where(c => c.Type == link.MimeType).First();
            var cate = CategoriesDictionary[link.Category];

            if (!code.Enabled)
                return false;
            if (!type.Enabled)
                return false;
            if (!cate.Enabled)
                return false;

            return true;
        }
        private bool FilterLoaded(IArchLink link)
        {
            bool onlyLoaded = ShowOnlyLoaded.HasValue && ShowOnlyLoaded.Value;
            bool onlyUnloaded = ShowOnlyLoaded.HasValue && !ShowOnlyLoaded.Value;
            bool loaded = !string.IsNullOrEmpty(link.HtmlFilePath);
            bool unloaded = !loaded;
            if ((onlyUnloaded && loaded) || (onlyLoaded && unloaded))
                return false;

            return true;
        }


        public IEnumerable<ArchiveLink> GetFilteredLinks() => GetFilteredLinks(SaveMode.AllFiltered);
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

        public int LinksFilteredAmount
        {
            get => linksFilteredAmount;
            set
            {
                linksFilteredAmount = value;
                UpdateBlock = true;
                OnPropertyChanged();
                UpdateBlock = false;
            }
        }
        private int linksFilteredAmount;



        public ViewOptions()
        {
            ListView = new ListViewOptions();
            ListView.OnUpdated += Update;
            PropertyChanged += ViewOptions_PropertyChanged;
        }
        private bool UpdateBlock { get; set; }
        private void ViewOptions_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (!UpdateBlock)
            {
                Update();
            }
        }

        public ViewOptions(SiteSnapshot snap) : this()
        {
            SetSnapshot(snap);
        }


        protected override void InitCommands()
        {
            base.InitCommands();
            ToggleCategoriesCommand = new RelayCommand(ToggleCategories);
        }
        [JsonIgnore]
        public ICommand ToggleCategoriesCommand { get; private set; }
        private void ToggleCategories(object obj)
        {
            var cates = Categories;
            bool newState = !cates[0].Enabled;
            foreach (var cate in cates)
            {
                cate.Enabled = newState;
            }
            Update();
        }


        public void SetSnapshot(SiteSnapshot snap)
        {
            if(snap.Links.Count() > 0)
            {
                Snapshot = snap;
                LoadDates(snap);
                LoadCodesTypes(snap);
                LoadCategories(snap);
            }
        }
        private void LoadDates(SiteSnapshot snap)
        {
            var orderedDate = snap.Links.OrderBy(l => l.Date);
            DateTime minDate = orderedDate.First().Date;
            DateTime maxDate = orderedDate.Last().Date;

            DateRange = new DateRange(minDate, minDate, maxDate, maxDate);
            DateRange.PropertyChanged += Dates_Updated;
        }
        private void LoadCodesTypes(SiteSnapshot snap)
        {
            List<string> codes = new List<string>();
            List<string> types = new List<string>();
            foreach (IArchLink link in snap.Links)
            {
                string code = link.StatusCode;
                string type = link.MimeType;

                if (!codes.Contains(code))
                    codes.Add(code);
                if (!types.Contains(type))
                    types.Add(type);
            }
            Codes = codes.Select(c => new StatusCode(c)).ToArray();
            Types = types.Select(t => new MimeType(t)).ToArray();
            OnPropertyChanged(nameof(Codes));
            OnPropertyChanged(nameof(Types));
        }
        public void LoadCategories(SiteSnapshot snap)
        {
            var res = snap.GetCategories();
            Categories = res.Collection.OfType<Category>().ToArray();
            CategoriesDictionary = res.Dictionary;
            OnPropertyChanged(nameof(Categories));
        }


        private void Dates_Updated(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Update();
        }
        protected void Update()
        {
            OnUpdated?.Invoke();
        }
    }
}
