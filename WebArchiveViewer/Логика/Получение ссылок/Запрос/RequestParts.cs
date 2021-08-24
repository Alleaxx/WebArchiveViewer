using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchiveViewer
{
    public class RequestSite : RequestPart
    {
        protected override string PrefixChar => "";
        public RequestSite() : base("url", "Адрес", "http://ru-minecraft.ru/forum")
        {

        }
    }

    public class MatchTypes : RequestPart
    {
        public LinkMatchType[] Types { get; private set; } = new LinkMatchType[]
        {
            new LinkMatchType(MatchType.exact),
            new LinkMatchType(MatchType.prefix),
            new LinkMatchType(MatchType.host),
            new LinkMatchType(MatchType.domain),
        };
        public LinkMatchType Selected { get; set; }

        public override string Value => Selected.Value;

        public MatchTypes() : base("matchType", "Тип поиска", "")
        {
            Selected = Types[1];
        }
    }
    public enum MatchType
    {
        exact,
        prefix,
        host,
        domain
    }
    public class LinkMatchType : RequestPart
    {
        public LinkMatchType(MatchType type) : base("matchType", "Тип поиска", "exact")
        {
            switch (type)
            {
                case MatchType.exact:
                    Value = "exact";
                    Name = "Точный [exact]";
                    Description = "Возвращает результаты только по указанному адресу";
                    break;
                case MatchType.prefix:
                    Value = "prefix";
                    Name = "По префиксу [prefix]";
                    Description = "Возвращает результаты со всеми под-ссылками";
                    break;
                case MatchType.host:
                    Value = "host";
                    Name = "По хосту [host]";
                    Description = "Возвращает результаты по основному хосту адреса";
                    break;
                case MatchType.domain:
                    Value = "domain";
                    Name = "По домену [domain]";
                    Description = "Возвращает результаты по всем хостам адреса (домену)";
                    break;
            }

        }
    }


    public class RequestLimit : RequestPart
    {
        public int Min { get; private set; } = -1;
        public int Max { get; private set; } = 2000;
        public int Amount
        {
            get => amount;
            set
            {
                if (value < Min)
                    amount = Min;
                else if (value > Max)
                    amount = Max;
                else
                    amount = value;
                OnPropertyChanged();
            }
        }
        private int amount = -1;

        public override string Value => Amount > 0 ? Amount.ToString() : "";

        public RequestLimit() : base("limit", "Лимит", "-1")
        {

        }
    }
    public class RequestOutput : RequestPart
    {
        public bool JSON { get; set; }
        public bool Usual
        {
            get => !JSON;
            set
            {
                if (value)
                    JSON = false;
                else
                    JSON = true;

                OnPropertyChanged();
                OnPropertyChanged(nameof(JSON));
            }
        }

        public override string Value => JSON ? "json" : "";
        public RequestOutput() : base("output", "Вывод", "json")
        {
            JSON = true;
        }
    }
    public class RequestDates : RequestPart
    {
        private string PrefixFrom { get; set; } = "from";
        private string PrefixTo { get; set; } = "to";

        public override string RequestString => Enabled ? $"{RequestFrom}{RequestTo}" : "";
        private string RequestFrom => $"{PrefixChar}{PrefixFrom}={Range.From.ToString(DateFormat)}";
        private string RequestTo => $"{PrefixChar}{PrefixTo}={Range.To.ToString(DateFormat)}";


        public DateRange Range { get; private set; } = new DateRange();
        private string DateFormat { get; set; } = "yyyyMMddhhmmss";

        public RequestDates() : base("", "Даты", "")
        {

        }
    }


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




            //string filters = "";
            //foreach (var option in Options.Codes)
            //{
            //    if (int.TryParse(option.Value, out int codeNum))
            //    {
            //        string inc = option.Enabled ? "" : "!";
            //        filters += $"&filter={inc}statuscode:{codeNum}";
            //    }
            //}
            //foreach (var option in Options.Types)
            //{
            //    string inc = option.Enabled ? "" : "!";
            //    filters += $"&filter={inc}mimetype:{option.Value}";
            //}
            //if (!string.IsNullOrEmpty(Options.Search))
            //{
            //    string inc = Options.SearchIncluded ? "" : "!";
            //    filters += $"&filter={inc}urlkey:{Options.Search}";
            //}
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
                    text = text.Substring(1);

                if (!Filters.ContainsKey(text) && !string.IsNullOrEmpty(text))
                    Filters.Add(text, !disabled);
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

    public class RequestCodes : RequestFilter
    {
        public RequestCodes() : base("statuscode")
        {

        }
    }
    public class RequestTypes : RequestFilter
    {
        public RequestTypes() : base("mimetype")
        {

        }
    }
    public class RequestSearch : RequestFilter
    {
        public RequestSearch() : base("urlkey")
        {

        }
    }

}
