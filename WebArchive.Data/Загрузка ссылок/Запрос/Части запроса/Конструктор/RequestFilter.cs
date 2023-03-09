using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchive.Data.RequestParts
{
    //Фильтр по кодам и типам контента
    //Заполняется вручную
    public class RequestFilter : RequestPart
    {
        public string FilterType { get; protected set; }

        public override string RequestString => CreateRequestString();
        private string CreateRequestString()
        {
            Dictionary<string, bool> filters = GetFilters();

            StringBuilder sb = new StringBuilder();
            foreach (var option in filters)
            {
                string inc = option.Value ? "" : "!";
                sb.Append($"{PrefixChar}{Prefix}={inc}{FilterType}:{option.Key}");
            }
            return sb.ToString();
        }


        //Если не заполнена текстовая строка, то фильтры заполняются из конструктора
        //Если текстовая строка заполнена, то фильтр не учитывается
        private Dictionary<string, bool> GetFilters()
        {
            if (string.IsNullOrEmpty(FiltersString) && FilterConstructor.List != null && FilterConstructor.List.Any())
            {
                return RefillFilters(FilterConstructor);
            }
            else if (!string.IsNullOrEmpty(FiltersString))
            {
                return RefillFilters(FiltersString);
            }
            else
            {
                return new Dictionary<string, bool>();
            }
        }
        private Dictionary<string, bool> RefillFilters(string filterText)
        {
            var filters = new Dictionary<string, bool>();
            var split = filterText.Split(';');
            foreach (var option in split)
            {
                string text = option.Trim();
                bool disabled = text.StartsWith("!");
                if (disabled)
                {
                    text = text.Substring(1);
                }

                if (!filters.ContainsKey(text) && !string.IsNullOrEmpty(text))
                {
                    filters.Add(text, !disabled);
                }
            }
            return filters;
        }
        private Dictionary<string, bool> RefillFilters(RequestTypesList constructor)
        {
            var filters = new Dictionary<string, bool>();
            foreach (var item in constructor.List)
            {
                string text = item.Content;
                bool disabled = !item.Enabled;

                if (!filters.ContainsKey(text) && !string.IsNullOrEmpty(text))
                {
                    filters.Add(text, !disabled);
                }
            }
            return filters;
        }

        /// <summary> Строка для фильтрации. Имеет приоритет над конструктором </summary>
        public string FiltersString
        {
            get => filtersString;
            set => Set(ref filtersString, value);
        }
        private string filtersString;
        public RequestTypesList FilterConstructor { get; private set; }


        public RequestFilter(string filterType, IEnumerable<string> source) : base("filter", "Фильтры", "")
        {
            FilterType = filterType;
            FilterConstructor = new RequestTypesList(source);
        }
    }

    public class RequestTypesList
    {
        public string[] Source { get; private set; }
        public IList<RequestType> List { get; private set; }


        public RequestTypesList(IEnumerable<string> source)
        {
            Source = source.ToArray();
        }
        public void SetCollection(IList<RequestType> list)
        {
            List = list;
        }

        public IEnumerable<string> GetSourceWithoutPicked()
        {
            return Source.Except(List.Select(l => l.Content)).ToArray();
        }
        public string GetFilterString()
        {
            return List.Any() ? string.Join(",", List.Select(l => l.Enabled ? l.Content : $"!{l.Content}")) : null;
        }
    }


    public class RequestType
    {
        public string Content { get; set; }
        public bool Enabled { get; set; }
    }
}
