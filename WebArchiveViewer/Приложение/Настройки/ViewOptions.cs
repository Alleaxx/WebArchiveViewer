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
    public class ViewOptions : NotifyObj
    {
        public event Action OnUpdated;

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
        private string search = "";
        public DateRange DateRange { get; private set; }


        public StatusCode[] Codes { get; private set; }
        public MimeType[] Types { get; private set; }
        public Category[] Categories { get; private set; }
        private Dictionary<string, ICategory> CategoriesDictionary { get; set; }

        public ListViewOptions ListView { get; private set; }

        public bool? ShowOnlyLoaded { get; set; } = null;


        private bool Filter(object obj)
        {
            ArchiveLink link = obj as ArchiveLink;
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
            bool linkSearchNotFound = !string.IsNullOrEmpty(Search) && !link.LinkSource.Contains(Search);
            bool nameSearchNotFound = !string.IsNullOrEmpty(Search) && !link.Name.Contains(Search);

            return (linkSearchNotFound && nameSearchNotFound) ? false : true;
        }
        private bool FilterTypes(ArchiveLink link)
        {
            var code = Codes.Where(c => c.Code == link.StatusCode).First();
            var type = Types.Where(c => c.Type == link.MimeType).First();
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



        public ViewOptions(Snapshot snap = null)
        {
            ListView = new ListViewOptions();
            ListView.OnUpdated += Update;
            PropertyChanged += ViewOptions_PropertyChanged;
            if(snap != null)
            {
                SetSnapshot(snap);
            }
        }
        private bool UpdateBlock { get; set; }
        private void ViewOptions_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (!UpdateBlock)
            {
                Update();
            }
        }


        protected override void InitCommands()
        {
            base.InitCommands();
            ToggleCategoriesCommand = new RelayCommand(ToggleCategories);
        }
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


        public void SetSnapshot(Snapshot snap)
        {
            if(snap.Links.Count() > 0)
            {
                Snapshot = snap;
                LoadDates(snap);
                LoadCodesTypes(snap);
                LoadCategories(snap);
            }
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
            List<string> codes = new List<string>();
            List<string> types = new List<string>();
            foreach (ArchiveLink link in snap.Links)
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
        public void LoadCategories(Snapshot snap)
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
