using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WebArchiveViewer
{
    public class RelayCommand : ICommand
    {
        public override string ToString()
        {
            return $"Команда {(canExecute == IsTrue ? "с условием" : ", активна всегда")}";
        }

        private readonly Action<object> execute;
        private readonly Func<object, bool> canExecute;
 
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action<object> execute) : this(execute, IsTrue)
        {

        }
        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return canExecute == null || canExecute(parameter);
        } 
        public void Execute(object parameter)
        {
            execute(parameter);
        }


        public static bool IsTrue(object obj) => true;
    }

}
