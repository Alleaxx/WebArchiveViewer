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
        private static IList<Action> actions = new List<Action>();

        public static void ExeInDispatcher(Action action)
        {
            actions.Add(action);
            Application.Current.Dispatcher.BeginInvoke(action);
        }
        public static async Task ExeInDispatcherAsync(Action action)
        {
            await Application.Current.Dispatcher.InvokeAsync(action);
        }
        public static void DoAll()
        {
            foreach (var action in actions)
            {
                Application.Current.Dispatcher?.Invoke(action);
            }
            actions.Clear();
        }


    }
}
