using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WebArchiveViewer
{
    public class RulesControl
    {
        public override string ToString()
        {
            return $"Управление правилами категоризации";
        }

        public GroupRule Rule { get; set; }
        [JsonIgnore]
        public GroupRule[] Rules => new GroupRule[1] { Rule };

        public RulesControl()
        {
            Rule = new GroupRule("Всё", "");
        }


        public void AddRules(IRules rules)
        {
            Rule.AddRules(rules.GetRules());
        }
        public string CheckLink(ArchiveLink link)
        {
            return CheckLink(link.LinkSource);
        }
        public string CheckLink(string link)
        {
            return Rule.IsMatched(link);
        }
    }
}
