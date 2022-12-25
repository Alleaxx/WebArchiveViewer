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
        public RequestCodes() : base("statuscode")
        {

        }
    }
}
