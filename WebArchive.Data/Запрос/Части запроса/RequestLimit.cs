using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchive.Data.RequestParts
{
    //Ограничение максимального числа получаемых ссылок
    //Заполняется вручную
    public class RequestLimit : RequestPart
    {
        public int Min { get; private set; } = -1;
        public int Max { get; private set; } = 2000;
        public int Amount
        {
            get => amount;
            set
            {
                if (value < Min)
                {
                    Set(ref amount, Min);
                }
                else if (value > Max)
                {
                    Set(ref amount, Max);
                }
                else
                {
                    Set(ref amount, value);
                }
            }
        }
        private int amount = -1;

        public override string Value => Amount > 0 ? Amount.ToString() : "";

        public RequestLimit() : base("limit", "Лимит", "-1")
        {

        }
    }
}
