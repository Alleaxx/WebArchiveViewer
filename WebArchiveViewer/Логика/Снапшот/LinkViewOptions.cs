using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchiveViewer
{
    [Serializable]
    public class ViewOptions : NotifyObj
    {
        public event Action Updated;


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
        public DateTime DateMin { get; protected set; }
        [JsonIgnore]
        public DateTime DateMax { get; protected set; }


        [JsonIgnore]
        public double Difference => (DateMax - DateMin).TotalHours;
        [JsonIgnore]
        public double DifferenceFrom
        {
            get => (From - DateMin).TotalHours;
            set
            {
                From = DateMin.AddHours(value);
            }
        }
        [JsonIgnore]
        public double DifferenceTo
        {
            get => (To - DateMin).TotalHours;
            set
            {
                To = DateMin.AddHours(value);
            }
        }



        public ObservableCollection<Option<string>> Codes { get; set; } = new ObservableCollection<Option<string>>();
        public ObservableCollection<Option<string>> Types { get; set; } = new ObservableCollection<Option<string>>();



        [JsonIgnore]
        public string CodesText
        {
            get => codesText;
            set
            {
                codesText = value;
                OnPropertyChanged();

                Codes.Clear();
                var res = codesText.Split(';');
                foreach (var code in res)
                {
                    string newCode = code.Trim();
                    Codes.Add(new Option<string>(newCode));
                }
            }
        }
        private string codesText;

        [JsonIgnore]
        public string TypesText
        {
            get => typesText;
            set
            {
                typesText = value;
                OnPropertyChanged();

                Types.Clear();
                var res = typesText.Split(';');
                foreach (var type in res)
                {
                    string newType = type.Trim();
                    Types.Add(new Option<string>(newType));
                }

            }
        }
        private string typesText;

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
            Updated?.Invoke();
        }
    }

    public class ViewOptionsGetLinks : ViewOptions
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

        public ViewOptionsGetLinks()
        {
            DateMin = new DateTime(2000, 1, 1);
            DateMax = DateTime.Now;
        }
    }
    public class ViewOptionsLinks : ViewOptions
    {

        private ArchiveView View { get; set; }
        public bool? ShowLoaded { get; set; } = null;

        public ViewOptionsLinks()
        {

        }
        public ViewOptionsLinks(ISnapshot snap)
        {
            From = snap.ViewOptions.From;
            DateMin = From;
            To = snap.ViewOptions.To;
            DateMax = To;
            Search = "";
            LoadCodesTypes(snap);
        }
        private void LoadCodesTypes(ISnapshot snap)
        {
            Types.Clear();
            Codes.Clear();

            List<string> types = new List<string>();
            List<string> codes = new List<string>();

            foreach (IArchLink link in snap.Links)
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
    }

}
