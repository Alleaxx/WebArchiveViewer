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
        public RequestTypes() : base("mimetype")
        {

        }
    }
}
