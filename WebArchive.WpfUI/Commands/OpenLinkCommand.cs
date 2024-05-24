using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WebArchiveViewer.WpfUI.Commands
{
    /// <summary>
    /// Команда на открытие ссылки в браузере или проводнике
    /// </summary>
    public class OpenLinkCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }


        public bool CanExecute(object parameter)
        {
            if(parameter is string stringLink && !string.IsNullOrWhiteSpace(stringLink))
            {
                return true;
            }
            //if(parameter is ILink link && !string.IsNullOrWhiteSpace(link.Link))
            //{
            //    return true;
            //}
            return false;
        }

        public void Execute(object parameter)
        {
            if (parameter is string link)
            {
                System.Diagnostics.Process.Start(link);
            }
            //if (parameter is ILink linkObj)
            //{
            //    System.Diagnostics.Process.Start(linkObj.Link);
            //}
        }
    }
}
