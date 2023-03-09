using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchive.Data.RequestParts
{
    //Фильтр по типам контента ссылок
    //Заполняется вручную
    public class RequestTypes : RequestFilter
    {
        public static readonly string[] AllTypes = new string[]
        {
            "text/html",
            "application/xml"
        };

        public RequestTypes() : base("mimetype", AllTypes)
        {

        }
    }
}
