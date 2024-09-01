using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using WebArchive.Data;
using WebArchiveViewer.Views.Windows;
using WebArchiveViewer.WpfUI.Commands;

namespace WebArchiveViewer.ViewModels
{
    public class RulesInfoViewModel : NotifyObject
    {
        public SnapshotView SnapshotView { get; private set; }
        public GroupRule RulesControl { get; private set; }

        public ICommand OpenRulesWindowCommand { get; private set; }
        public ICommand RemoveRuleCommand { get; private set; }
        public ICommand AddRuleCommand { get; private set; }
        public ICommand MoveRuleUpCommand { get; private set; }
        public ICommand MoveRuleDownCommand { get; private set; }

        public RulesInfoViewModel()
        {
            RulesControl = new GroupRule();
        }
        public RulesInfoViewModel(SnapshotView snap)
        {
            SnapshotView = snap;
            RulesControl = snap?.SnapshotModel?.RulesControl ?? new GroupRule("Главная категория", string.Empty);
            OpenRulesWindowCommand = new RelayCommand(OnOpenRulesWindowCommandExecuted);
            RemoveRuleCommand = new RelayCommand(OnRemoveRuleCommandExecuted)
                .SetCondition(IsNotMainRule);
            AddRuleCommand = new RelayCommand(OnAddRuleCommandExecuted);
            MoveRuleUpCommand = new RelayCommand(OnMoveRuleUpCommandExecuted);
            MoveRuleDownCommand = new RelayCommand(OnMoveRuleDownCommandExecuted);
        }

        //Условия
        private bool IsNotNull(object obj)
        {
            return RulesControl != null;
        }
        private bool IsNotMainRule(object obj)
        {
            return IsNotNull(obj) && obj != RulesControl;
        }

        //Команды
        private void OnOpenRulesWindowCommandExecuted(object obj)
        {
            RulesWindow window = new RulesWindow(this);
            window.ShowDialog();
        }
        private void OnRemoveRuleCommandExecuted(object obj)
        {
            if (obj is GroupRule ruleToRemove)
            {
                RulesControl.RemoveInner(ruleToRemove);
            }
        }
        private void OnAddRuleCommandExecuted(object obj)
        {
            if (obj is GroupRule rule)
            {
                rule.Rules.Insert(0, new GroupRule("Новое правило", "???"));
            }
        }
        private void OnMoveRuleUpCommandExecuted(object obj)
        {
            if (obj is GroupRule ruleToMove)
            {
                MoveRule(ruleToMove, -1);
            }
        }
        private void OnMoveRuleDownCommandExecuted(object obj)
        {
            if (obj is GroupRule ruleToMove)
            {
                MoveRule(ruleToMove, 1);
            }
        }

        private void MoveRule(GroupRule ruleToMove, int offset)
        {
            var ruleContainer = FindRuleParent(ruleToMove);
            var rules = ruleContainer.Rules;
            var oldIndex = rules.IndexOf(ruleToMove);
            int newIndex = oldIndex + offset;
            if(newIndex >= 0 && newIndex < rules.Count)
            {
                ruleContainer.Rules.Move(oldIndex, newIndex);
            }
        }

        private GroupRule FindRuleParent(GroupRule ruleToFound)
        {
            var allRules = RulesControl.GetAllRules().ToArray();
            return allRules.FirstOrDefault(r => r.Rules.Contains(ruleToFound));
        }
    }
}
