using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using WebArchive.Data;
using WebArchiveViewer.Views.Windows;

namespace WebArchiveViewer.ViewModels
{
    public class RulesInfoViewModel : NotifyObject
    {
        #region Ссылки

        public readonly SnapshotView SnapshotView;
        public GroupRule RulesControl { get; private set; }
        
        #endregion

        public RulesInfoViewModel()
        {
            RulesControl = new GroupRule();
        }
        public RulesInfoViewModel(SnapshotView snap)
        {
            SnapshotView = snap;
            RulesControl = snap?.SnapshotModel?.RulesControl;
            CreateCommands();
        }

        #region Команды

        private void CreateCommands()
        {
            OpenRulesWindowCommand = new RelayCommand(OnOpenRulesWindowCommandExecuted, IsNotNull);
            RemoveRuleCommand = new RelayCommand(OnRemoveRuleCommandExecuted, IsNotMainRule);
            AddRuleCommand = new RelayCommand(OnAddRuleCommandExecuted, IsNotNull);
        }

        public ICommand OpenRulesWindowCommand { get; private set; }
        public ICommand RemoveRuleCommand { get; private set; }
        public ICommand AddRuleCommand { get; private set; }

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

        #endregion
    }
}
