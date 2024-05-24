using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchive.Data
{
    public class DayLinkStat
    {
        //Номер дня с минимальной даты
        public int DayPosition { get; set; }

        //Количество ссылок на указанную дату
        public int LinksCount { get; set; }
    }
}
