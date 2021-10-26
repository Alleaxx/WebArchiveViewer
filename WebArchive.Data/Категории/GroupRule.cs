using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchive.Data
{
    public class GroupRule
    {
        public override string ToString()
        {
            return $"[{Rules.Count}] Правило '{GroupName}': {FoundText}";
        }

        //Имя правила и соответствующий паттерн
        public string GroupName { get; set; }
        public string FoundText { get; set; }

        public ObservableCollection<GroupRule> Rules { get; private set; }

        public GroupRule() : this("-", "-")
        {

        }
        public GroupRule(string name, string text, params GroupRule[] rules)
        {
            GroupName = name;
            FoundText = text;
            Rules = new ObservableCollection<GroupRule>(rules);
        }


        public void AddRules(params GroupRule[] rules)
        {
            foreach (var rule in rules)
            {
                Rules.Add(rule);
            }
        }
        public void AddRules(IEnumerable<GroupRule> rules)
        {
            foreach (var rule in rules)
            {
                Rules.Add(rule);
            }
        }

        public string IsMatched(ArchiveLink link)
        {
            return IsMatched(link.LinkSource);
        }
        public string IsMatched(string link)
        {
            if (link.Contains(FoundText))
            {
                if (Rules.Count == 0)
                {
                    return GroupName;
                }
                else
                {
                    foreach (var rule in Rules)
                    {
                        if (rule.IsMatched(link) is string res && !string.IsNullOrEmpty(res))
                            return res;
                    }
                }
                return GroupName;
            }
            return null;
        }


        public void Remove(GroupRule ruleToRemove)
        {
            Rules.Remove(ruleToRemove);
            foreach (GroupRule rule in Rules)
            {
                rule.Remove(ruleToRemove);
            }
        }
    }
}
