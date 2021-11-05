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
        public void AddInner(IEnumerable<GroupRule> newRules)
        {
            foreach (GroupRule rule in newRules)
            {
                Rules.Add(rule);
            }
        }
        public void RemoveInner(GroupRule ruleToRemove)
        {
            Rules.Remove(ruleToRemove);
            foreach (GroupRule rule in Rules)
            {
                rule.RemoveInner(ruleToRemove);
            }
        }


        public GroupRule() : this("Всё", "")
        {

        }
        public GroupRule(string name, string text)
        {
            GroupName = name;
            FoundText = text;
            Rules = new ObservableCollection<GroupRule>();
        }
        public GroupRule(string name, string text, params GroupRule[] rules)
        {
            GroupName = name;
            FoundText = text;
            Rules = new ObservableCollection<GroupRule>(rules);
        }

        public string CheckLink(ArchiveLink link)
        {
            return CheckLink(link.LinkSource);
        }
        public string CheckLink(string link)
        {
            if (link.Contains(FoundText))
            {
                foreach (GroupRule rule in Rules)
                {
                    if (rule.CheckLink(link) is string res && !string.IsNullOrEmpty(res))
                    {
                        return res;
                    }
                }
                return GroupName;
            }
            return null;
        }
    }
}
