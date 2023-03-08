using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WebArchiveViewer.Services
{
    internal static class DispatcherService
    {
        public static void ExeInDispatcher(Action action)
        {
            Application.Current.Dispatcher.BeginInvoke(action);
        }
    }
}
