using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WebArchiveViewer.WpfUI.Commands
{
    public class RelayCommand : ICommand
    {
        protected Action<object> execute;
        protected Func<object, bool> canExecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        protected RelayCommand()
        {

        }
        public RelayCommand(Action execute)
        {
            this.execute = obj => execute();
            this.canExecute = IsTrue;
        }
        public RelayCommand(Action<object> execute)
        {
            this.execute = execute;
            this.canExecute = IsTrue;
        }

        public RelayCommand SetCondition(Func<object, bool> func)
        {
            this.canExecute = func;
            return this;
        }
        public RelayCommand SetCondition(Func<bool> func)
        {
            this.canExecute = obj => func.Invoke();
            return this;
        }

        public bool CanExecute(object parameter)
        {
            return canExecute == null || canExecute(parameter);
        }
        public void Execute(object parameter)
        {
            execute(parameter);
        }

        public static bool IsTrue(object obj)
        {
            return true;
        }
    }
}
