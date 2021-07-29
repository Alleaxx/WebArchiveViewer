using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchiveViewer
{

    class Option<T>
    {
        public T Value { get; set; }
        public bool Enabled { get; set; }
        public Option()
        {

        }
        public Option(T val, bool enabled = true)
        {
            Value = val;
            Enabled = enabled;
        }
    }
    class OptionCount<T> : Option<T>
    {
        public int Amount { get; set; }
        public OptionCount(T val, bool enabled = true) : base(val,enabled)
        {
            Amount = 1;
        }
    }


    [Serializable]
    class LinksViewOptions : ObjNotify
    {
        public DateTime From
        {
            get => from;
            set
            {
                from = value;
                OnPropertyChanged();
                Update();
            }
        }
        private DateTime from;
        public DateTime To
        {
            get => to;
            set
            {
                to = value;
                OnPropertyChanged();
                Update();
            }
        }
        private DateTime to;

        [JsonIgnore]
        public DateTime DateMin { get; set; }
        [JsonIgnore]
        public DateTime DateMax { get; set; }
        
        public ObservableCollection<Option<string>> Codes { get; set; } = new ObservableCollection<Option<string>>();
        public ObservableCollection<Option<string>> Types { get; set; } = new ObservableCollection<Option<string>>();

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
        private string search;

        protected virtual void Update()
        {

        }
    }

    class LinksViewOptionsGet : LinksViewOptions
    {
        public bool CodesIncluded { get; set; } = true;
        public bool TypesIncluded { get; set; } = true;
        public bool SearchIncluded { get; set; } = true;

        public int Limit
        {
            get => limit;
            set
            {
                limit = value;
                OnPropertyChanged();
            }
        }
        private int limit = -1;

        public LinksViewOptionsGet()
        {
            DateMin = new DateTime(2000, 1, 1);
            DateMax = DateTime.Now;
        }
    }
    class LinksViewOptionsView : LinksViewOptions
    {
        private ArchiveView View { get; set; }
        public bool? ShowLoaded { get; set; } = null;

        public LinksViewOptionsView()
        {

        }
        public LinksViewOptionsView(ArchiveView view,SiteSnapshot snap)
        {
            From = snap.ViewOptions.From;
            DateMin = From;
            To = snap.ViewOptions.To;
            DateMax = To;
            Search = "";
            LoadCodesTypes(snap);
            View = view;
        }
        private void LoadCodesTypes(SiteSnapshot snap)
        {
            Types.Clear();
            Codes.Clear();
            List<string> types = new List<string>();
            List<string> codes = new List<string>();
            foreach (ArchiveLink link in snap.Links)
            {
                if (!types.Contains(link.MimeType))
                    types.Add(link.MimeType);
                if (!codes.Contains(link.StatusCode))
                    codes.Add(link.StatusCode);
            }
            foreach (var item in types)
            {
                Types.Add(new Option<string>(item, true));
            }
            foreach (var item in codes)
            {
                Codes.Add(new Option<string>(item, true));
            }
        }

        protected override void Update()
        {
            base.Update();
            if(View != null)
                View.UpdateList();
        }
    }

}
