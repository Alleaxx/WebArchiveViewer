using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WebArchiveViewer
{
    public class GroupRule
    {
        public override string ToString() => $"[{Rules.Count}] Правило '{GroupName}': {FoundText}";

        public string GroupName { get; set; }
        public string FoundText { get; set; }

        public ObservableCollection<GroupRule> Rules { get; set; }

        public GroupRule() : this("-","-")
        {

        }
        public GroupRule(string name, string text, params GroupRule[] rules)
        {
            GroupName = name;
            FoundText = text;
            Rules = new ObservableCollection<GroupRule>(rules);

            AddRuleCommand = new RelayCommand(AddRule);
        }


        [JsonIgnore]
        public ICommand AddRuleCommand { get; private set; }
        private void AddRule(object obj)
        {
            Rules.Insert(0, new GroupRule());
        }
        public void AddRules(params GroupRule[] rules)
        {
            foreach (var rule in rules)
            {
                Rules.Add(rule);
            }
        }


        public string IsMatched(IArchLink link)
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
        public GroupRule FindOwner(GroupRule rule)
        {
            throw new NotImplementedException();
        }

        public void Remove(GroupRule ruleToRemove)
        {
            Rules.Remove(ruleToRemove as GroupRule);
            foreach (var rule in Rules)
            {
                rule.Remove(ruleToRemove);
            }
        }
    }
}
