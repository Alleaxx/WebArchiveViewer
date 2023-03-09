using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchive.Data.RequestParts
{
    //Фильтр по коду ответа ссылок
    //Заполняется вручную
    public class RequestCodes : RequestFilter
    {
        public static readonly string[] AllCodes = new string[]
        {
            "200",
            "404",
            "302",
            "502",
            "301",
            "400",
            "405"

        };

        public RequestCodes() : base("statuscode", AllCodes)
        {

        }
    }
}
