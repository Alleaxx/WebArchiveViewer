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

        public override string RequestString => requestString;
        private string requestString;


        private string CreateRequestString()
        {
            RefillFilters(filtersString);
            StringBuilder sb = new StringBuilder();
            foreach (var option in Filters)
            {
                string inc = option.Value ? "" : "!";
                sb.Append($"{PrefixChar}{Prefix}={inc}{FilterType}:{option.Key}");
            }
            return sb.ToString();
        }
        private void RefillFilters(string filterText)
        {
            Filters.Clear();
            var split = filterText.Split(';');
            foreach (var option in split)
            {
                string text = option.Trim();
                bool disabled = text.StartsWith("!");
                if (disabled)
                {
                    text = text.Substring(1);
                }

                if (!Filters.ContainsKey(text) && !string.IsNullOrEmpty(text))
                {
                    Filters.Add(text, !disabled);
                }
            }
        }

        private Dictionary<string, bool> Filters { get; set; } = new Dictionary<string, bool>();
        public string FiltersString
        {
            get => filtersString;
            set
            {
                filtersString = value;
                requestString = CreateRequestString();
                OnPropertyChanged();
            }
        }
        private string filtersString;

        public RequestFilter(string filterType) : base("filter", "Фильтры", "")
        {
            FilterType = filterType;
        }
    }
}
