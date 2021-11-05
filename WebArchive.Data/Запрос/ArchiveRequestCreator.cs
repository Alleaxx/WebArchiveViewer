using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchive.Data
{
    //Запрос из шаблона для архива
    public class ArchiveRequestCreator : IRequestCreator
    {
        public override string ToString()
        {
            return $"Архивный построитель запроса - {Request}";
        }

        public IRequestPart Site { get; private set; }
        public IRequestPart Output { get; private set; }
        public MatchTypes MatchType { get; private set; }
        public RequestDates Dates { get; private set; }
        public RequestLimit Limit { get; private set; }
        public RequestCodes Codes { get; private set; }
        public RequestTypes Types { get; private set; }
        public RequestSearch Search { get; private set; }

        public ArchiveRequestCreator()
        {
            Site = new RequestSite();
            Output = new RequestOutput();
            MatchType = new MatchTypes();
            Limit = new RequestLimit();
            Dates = new RequestDates();

            Codes = new RequestCodes();
            Types = new RequestTypes();
            Search = new RequestSearch();
        }

        //Ссылка на API
        //https://github.com/internetarchive/wayback/tree/master/wayback-cdx-server
        
        //Образец формируемой ссылки
        //http://web.archive.org/cdx/search/cdx?url=http://ru-minecraft.ru*&output=json&from=2010&to=2011
        public string Request => GetRequest();
        public string GetRequest()
        {
            string WebArhiveSearch = "http://web.archive.org/cdx/search/cdx?";
            string site = Site.RequestString;
            string json = Output.RequestString;
            string matchType = MatchType.RequestString;
            string dates = Dates.RequestString;
            string limit = Limit.RequestString;

            string filters = $"{Codes.RequestString}{Types.RequestString}{Search.RequestString}";

            if (!string.IsNullOrEmpty(site))
            {
                return $"{WebArhiveSearch}{site}{matchType}{json}{limit}{dates}{filters}";
            }
            else
            {
                return "";
            }
        }
    }
}
