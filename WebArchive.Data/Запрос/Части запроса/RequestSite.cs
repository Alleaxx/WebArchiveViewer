using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchive.Data.RequestParts
{
    //Адрес сайта
    //Заполняется вручную
    public class RequestSite : RequestPart
    {
        protected override string PrefixChar => "";
        public RequestSite() : base("url", "Адрес", "http://ru-minecraft.ru/forum")
        {

        }
    }
}
