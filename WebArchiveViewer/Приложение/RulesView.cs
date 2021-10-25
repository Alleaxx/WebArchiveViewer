using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WebArchiveViewer
{
    public class RulesView : NotifyObj
    {
        public RulesControl RulesControl { get; private set; }

        public RulesView()
        {
            RulesControl = new RulesControl();
        }
        public RulesView(Snapshot snap)
        {
            RulesControl = snap == null ? null : snap.RulesControl;
            CreateCommands();
        }
        private void CreateCommands()
        {
            if (RulesControl == null)
            {
                ShowRulesCommand = new RelayCommand(Nothing, NotAvailable);
                RemoveRuleCommand = new RelayCommand(Nothing, NotAvailable);
                AddRuleCommand = new RelayCommand(Nothing, NotAvailable);
            }
            else
            {
                ShowRulesCommand = new RelayCommand(ShowRules);
                RemoveRuleCommand = new RelayCommand(RemoveRule, IsNotMainRule);
                AddRuleCommand = new RelayCommand(AddRule, IsNotMainRule);
            }
            void Nothing(object obj)
            {

            }
            bool NotAvailable(object obj)
            {
                return false;
            }
        }

        public ICommand ShowRulesCommand { get; private set; }
        public ICommand RemoveRuleCommand { get; private set; }
        public ICommand AddRuleCommand { get; private set; }

        private bool IsNotMainRule(object obj)
        {
            return obj != RulesControl.Rule;
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
                RulesControl.Rule.Remove(ruleToRemove);
            }
        }
        private void AddRule(object obj)
        {
            if (obj is GroupRule rule)
            {
                rule.Rules.Insert(0, new GroupRule());
            }
        }
    }
}
