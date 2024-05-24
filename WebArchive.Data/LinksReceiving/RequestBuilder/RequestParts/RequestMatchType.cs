using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchive.Data.RequestParts
{
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
