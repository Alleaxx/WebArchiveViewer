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
        public event Action Updated;

        public string Search
        {
            get => search;
            set
            {
                search = value;
                OnPropertyChanged();
                Update();
            }
        }
        private string search = "";
        public DateRange DateRange { get; set; }

        public StatusCode[] Codes { get; set; }
        public MimeType[] Types { get; set; }
        public Category[] Categories { get; set; }

        [JsonIgnore]
        public ListViewOptions ListView { get; private set; }

        public bool? ShowOnlyLoaded { get; set; } = null;


        private bool Filter(object obj)
        {
            IArchLink link = obj as IArchLink;
            var dates = DateRange;
            if (link.Date < dates.From || link.Date > dates.To)
                return false;

            var code = Codes.Where(c => c.Code == link.StatusCode).First();
            var type = Types.Where(c => c.Type == link.MimeType).First();
            var cate = Categories.Where(c => c.Name == link.Category).First();

            if (!code.Enabled)
                return false;
            if (!type.Enabled)
                return false;
            if (!cate.Enabled)
                return false;


            bool onlyLoaded = ShowOnlyLoaded.HasValue && ShowOnlyLoaded.Value;
            bool onlyUnloaded = ShowOnlyLoaded.HasValue && !ShowOnlyLoaded.Value;
            bool loaded = !string.IsNullOrEmpty(link.HtmlFilePath);
            bool unloaded = !loaded;
            if (onlyUnloaded && loaded || onlyLoaded && unloaded)
                return false;

            bool linkSearchNotFound = !string.IsNullOrEmpty(Search) && !link.LinkSource.Contains(Search);
            bool nameSearchNotFound = !string.IsNullOrEmpty(Search) && !link.Name.Contains(Search);
            if (linkSearchNotFound && nameSearchNotFound)
                return false;

            return true;
        }
        public IEnumerable<ArchiveLink> GetFilteredLinks(ISnapshot snapshot) => snapshot.Links.Where(l => Filter(l));
        public IEnumerable<ArchiveLink> GetFilteredLinks(ISnapshot snapshot,SaveMode mode)
        {
            var Links = snapshot.Links;
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

        public int LinksFilteredAmount
        {
            get => linksFilteredAmount;
            set
            {
                linksFilteredAmount = value;
                OnPropertyChanged();
            }
        }
        private int linksFilteredAmount;



        public ViewOptions()
        {
            ListView = new ListViewOptions();
            ListView.Updated += Update;
        }
        public ViewOptions(ISnapshot snap) : this()
        {
            UpdateForSnapshot(snap);
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


        public void UpdateForSnapshot(ISnapshot snap)
        {
            if(snap.Links.Count() > 0)
            {
                LoadDates(snap);
                LoadCodesTypes(snap);
                LoadCategories(snap);
            }
        }
        private void LoadDates(ISnapshot snap)
        {
            var orderedDate = snap.Links.OrderBy(l => l.Date);
            DateTime minDate = orderedDate.First().Date;
            DateTime maxDate = orderedDate.Last().Date;

            DateRange = new DateRange(minDate, minDate, maxDate, maxDate);
            DateRange.PropertyChanged += Dates_Updated;
        }
        private void LoadCodesTypes(ISnapshot snap)
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
        public void LoadCategories(ISnapshot snap)
        {
            Categories = snap.GetCategories().OfType<Category>().ToArray();
            OnPropertyChanged(nameof(Categories));
        }


        private void Dates_Updated(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Update();
        }
        protected void Update()
        {
            Updated?.Invoke();
        }
    }
}
