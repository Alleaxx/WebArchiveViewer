using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WebArchiveViewer
{
    public class RulesControl : NotifyObj
    {
        public override string ToString() => $"Управление правилами категоризации";


        public GroupRule Rule { get; set; }
        [JsonIgnore]
        public GroupRule[] Rules => new GroupRule[1] { Rule };

        public RulesControl()
        {
            Rule = new GroupRule("Всё", "");
        }
        protected override void InitCommands()
        {
            base.InitCommands();
            RemoveRuleCommand = new RelayCommand(RemoveRule, IsNotMainRule);
            ShowRulesCommand = new RelayCommand(ShowRules);
        }


        private bool IsNotMainRule(object obj) => obj != Rule;

        [JsonIgnore]
        public ICommand RemoveRuleCommand { get; private set; }
        private void RemoveRule(object obj)
        {
            if (obj is GroupRule ruleToRemove)
            {
                Rule.Remove(ruleToRemove);
            }
        }

        [JsonIgnore]
        public ICommand ShowRulesCommand { get; private set; }
        private void ShowRules(object obj)
        {
            RulesWindow window = new RulesWindow(this);
            window.ShowDialog();
        }


        public void AddRules(IRules rules)
        {
            Rule.AddRules(rules.GetRules().ToArray());
        }
        public string CheckLink(IArchLink link) => CheckLink(link.LinkSource);
        public string CheckLink(string link) => Rule.IsMatched(link);

    }
}
