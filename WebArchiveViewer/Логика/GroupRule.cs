using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchiveViewer
{
    class GroupRule
    {
        public string GroupName { get; set; }
        public string FoundText { get; set; }

        public List<GroupRule> Rules { get; set; }

        public GroupRule(string name, string text, params GroupRule[] rules)
        {
            GroupName = name;
            FoundText = text;
            Rules = new List<GroupRule>(rules);
        }

        public string IsMatched(ArchiveLink link)
        {
            if (IsMatchedStrict(link))
            {
                if (Rules.Count == 0)
                {
                    return GroupName;
                }
                else
                {
                    foreach (GroupRule rule in Rules)
                    {
                        if (rule.IsMatched(link) is string res && !string.IsNullOrEmpty(res))
                            return res;
                    }
                }
                return GroupName;
            }
            return null;
        }
        public bool IsMatchedStrict(ArchiveLink link) => link.LinkSource.Contains(FoundText);
    }
}
