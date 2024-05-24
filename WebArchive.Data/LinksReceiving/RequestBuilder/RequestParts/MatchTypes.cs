using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebArchive.Data.RequestParts;

namespace WebArchive.Data.RequestParts
{
    public enum MatchType
    {
        exact,
        prefix,
        host,
        domain
    }
    /// <summary>
    /// Тип поиска по ссылке
    /// Выбор из предустановленных 4-х значений: exact, prefix, host, domain
    /// </summary>
    public class MatchTypes : RequestPart
    {
        public LinkMatchType[] Types { get; private set; } = new LinkMatchType[]
        {
            new LinkMatchType(MatchType.exact),
            new LinkMatchType(MatchType.prefix),
            new LinkMatchType(MatchType.host),
            new LinkMatchType(MatchType.domain),
        };
        public LinkMatchType Selected { get; set; }

        public override string Value => Selected.Value;

        public MatchTypes() : base("matchType", "Тип поиска", "")
        {
            Selected = Types[1];
        }
    }
}
