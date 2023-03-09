using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchive.Data.RequestParts
{
    //Тип поиска по ссылке
    //Выбор из предустановленных 4-х значений: exact, prefix, host, domain
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
    public enum MatchType
    {
        exact,
        prefix,
        host,
        domain
    }
    public class LinkMatchType : RequestPart
    {
        public LinkMatchType(MatchType type) : base("matchType", "Тип поиска", "exact")
        {
            switch (type)
            {
                case MatchType.exact:
                    Value = "exact";
                    Name = "Точный [exact]";
                    Description = "Возвращает результаты только по указанному адресу";
                    break;
                case MatchType.prefix:
                    Value = "prefix";
                    Name = "По префиксу [prefix]";
                    Description = "Возвращает результаты со всеми под-ссылками";
                    break;
                case MatchType.host:
                    Value = "host";
                    Name = "По хосту [host]";
                    Description = "Возвращает результаты по основному хосту адреса";
                    break;
                case MatchType.domain:
                    Value = "domain";
                    Name = "По домену [domain]";
                    Description = "Возвращает результаты по всем хостам адреса (домену)";
                    break;
            }

        }
    }
}
