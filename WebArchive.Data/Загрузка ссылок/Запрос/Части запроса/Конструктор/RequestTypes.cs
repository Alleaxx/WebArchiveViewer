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
            "text/plain",
            "text/css",
            "image/gif",
            "image/png",
            "image/jpeg",
            "image/x-icon",
            "application/xml",
            "application/javascript",
            "application/force-download",
            "application/x-javascript",
            "application/x-shockwave-flash",
            "warc/revisit",
        };

        public RequestTypes() : base("mimetype", AllTypes)
        {

        }
    }
}
