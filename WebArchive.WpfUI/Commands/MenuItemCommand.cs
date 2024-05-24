using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WebArchiveViewer.WpfUI.Commands
{
    public class MenuCommand : RelayCommand, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public bool Visible => VisibleFunc.Invoke();
        public string Title => TitleFunc.Invoke();

        private string titleStatic;
        private Func<string> TitleFunc;
        private Func<bool> VisibleFunc;

        public MenuCommand(string name, Action<object> execute) : base(execute)
        {
            titleStatic = name;
            TitleFunc = () => titleStatic;
            VisibleFunc = () => true;
        }

        public MenuCommand SetTitleFunc(Func<string> func)
        {
            TitleFunc = func;
            return this;
        }
        public MenuCommand SetVisibleFunc(Func<bool> func)
        {
            VisibleFunc = func;
            return this;
        }
    }
}
