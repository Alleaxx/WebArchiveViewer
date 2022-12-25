using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchive.Data.RequestParts
{
    //Фильтр по датам захвата ссылок
    //Заполняется вручную
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
}
