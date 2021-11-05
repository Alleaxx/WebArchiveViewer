using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using WebArchive.Data;
namespace WebArchiveViewer
{
    public class RulesView : NotifyObj
    {
        public readonly SnapshotView Owner;
        public GroupRule RulesControl { get; private set; }


        public RulesView()
        {
            RulesControl = new GroupRule();
        }
        public RulesView(SnapshotView snap)
        {
            Owner = snap;
            RulesControl = snap?.Current?.RulesControl;
            CreateCommands();
        }
        private void CreateCommands()
        {
            ShowRulesCommand = new RelayCommand(ShowRules, Exist);
            RemoveRuleCommand = new RelayCommand(RemoveRule, IsNotMainRule);
            AddRuleCommand = new RelayCommand(AddRule, Exist);
        }


        public ICommand ShowRulesCommand { get; private set; }
        public ICommand RemoveRuleCommand { get; private set; }
        public ICommand AddRuleCommand { get; private set; }


        private bool Exist(object obj)
        {
            return RulesControl != null;
        }
        private bool IsNotMainRule(object obj)
        {
            return Exist(obj) && obj != RulesControl;
        }


        private void ShowRules(object obj)
        {
            RulesWindow window = new RulesWindow(this);
            window.ShowDialog();
        }
        private void RemoveRule(object obj)
        {
            if (obj is GroupRule ruleToRemove)
            {
                RulesControl.RemoveInner(ruleToRemove);
            }
        }
        private void AddRule(object obj)
        {
            if (obj is GroupRule rule)
            {
                rule.Rules.Insert(0, new GroupRule("Новое правило", "???"));
            }
        }
    }
}
