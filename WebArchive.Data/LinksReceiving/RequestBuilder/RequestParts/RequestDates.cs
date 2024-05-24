using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchive.Data.RequestParts
{
    /// <summary>
    /// Фильтр по датам захвата ссылок
    /// Заполняется вручную
    /// </summary>
    public class RequestDates : RequestPart
    {
        private string PrefixFrom { get; set; } = "from";
        private string PrefixTo { get; set; } = "to";

        public override string RequestString => GetRequestString();
        private string RequestFrom => $"{PrefixChar}{PrefixFrom}={Range.From.ToString(DateFormat)}";
        private string RequestTo => $"{PrefixChar}{PrefixTo}={Range.To.ToString(DateFormat)}";

        public DateRange Range { get; private set; } = new DateRange();
        private string DateFormat { get; set; } = "yyyyMMddhhmmss";

        public RequestDates() : base("", "Даты", "")
        {

        }

        private string GetRequestString()
        {
            if (!Enabled)
            {
                return "";
            }
            bool isFromEnabled = Range.DifferenceFrom != -1;
            bool isToEnabled = Range.DifferenceTo != -1;
            string requestFromPart = isFromEnabled ? RequestFrom : "";
            string requestToPart = isToEnabled ? RequestTo : "";
            return $"{requestFromPart}{requestToPart}";
        }
    }
}
