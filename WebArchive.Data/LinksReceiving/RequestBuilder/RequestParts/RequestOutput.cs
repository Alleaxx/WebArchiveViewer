using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchive.Data.RequestParts
{
    public class RequestOutput : RequestPart
    {
        public OutputTypes[] Types { get; private set; } = new OutputTypes[]
        {
            new OutputTypes(OutputType.JSON),
            new OutputTypes(OutputType.Usual),
        };
        public OutputTypes Selected { get; set; }

        public override string Value => Selected.Value;
        public RequestOutput() : base("output", "Вывод", "json")
        {
            Selected = Types[0];
        }
    }
}
