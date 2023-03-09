using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WebArchive.Data;
using WebArchiveViewer.Views.Windows;

namespace WebArchiveViewer.UI.Commands
{
    /// <summary>
    /// Команда на просмотр исключения в отдельном окне
    /// </summary>
    internal class OpenExceptionWindowCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }


        public bool CanExecute(object parameter)
        {
            return parameter is Exception;
        }

        public void Execute(object parameter)
        {
            if(parameter is Exception ex)
            {
                var window = new ExceptionWindow(ex);
                window.ShowDialog();
            }
        }
    }
}
