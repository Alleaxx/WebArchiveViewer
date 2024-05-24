using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchive.Data.RequestParts
{
    /// <summary>
    /// Фильтр по кодам и типам контента.
    /// Заполняется через конструктор (приоритетнее) или строку
    /// </summary>
    public class RequestFilter : RequestPart
    {
        public string FilterType { get; protected set; }

        /// <summary> Строка для фильтрации. Имеет приоритет над конструктором </summary>
        public string FiltersString
        {
            get => filtersString;
            set => Set(ref filtersString, value);
        }
        private string filtersString;
        public RequestFiltersList FilterConstructor { get; private set; }
        public override string RequestString => CreateRequestString();

        public RequestFilter(string filterType, IEnumerable<string> source) : base("filter", "Фильтры", "")
        {
            FilterType = filterType;
            FilterConstructor = new RequestFiltersList(source);
        }

        private string CreateRequestString()
        {
            if (!Enabled)
            {
                return string.Empty;
            }

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
            var filtersFromConstructor = FilterConstructor.GetFilters();
            if (filtersFromConstructor.Any())
            {
                return filtersFromConstructor;
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
    }
}
