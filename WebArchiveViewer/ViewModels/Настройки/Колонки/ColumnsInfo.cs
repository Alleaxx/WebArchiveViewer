using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WebArchiveViewer.ViewModels.ViewOptions;

namespace WebArchiveViewer
{
    public class ColumnsInfo
    {
        public event Action<ListViewColumn> OnColumnVisibilityChanged;
        private readonly ViewOptions Options;

        public List<ListViewColumn> Columns { get; private set; }

        public ColumnsInfo(ViewOptions options)
        {
            Options = options;
            Columns = new List<ListViewColumn>();
            HideColumnCommand = new RelayCommand(OnHideColumnCommandExecuted, CanHideColumnCommandExecute);
        }

        public ColumnsInfo AddColumn(string name, SortsEnum sort, GroupsEnum group, bool hidden = false)
        {
            int order = Columns.Count;
            var newCol = new ListViewColumn()
            {
                Name = name,
                Order = order,
                Hidden = hidden,
                Sorting = Options.ListView.GetSort(sort),
                Grouping = Options.ListView.GetGroup(group)
            };
            Columns.Add(newCol);
            newCol.PropertyChanged += Column_HiddenChanged;
            return this;
        }

        public ICommand HideColumnCommand { get; private set; }

        private bool CanHideColumnCommandExecute(object o)
        {
            return true;
        }
        public void OnHideColumnCommandExecuted(object o)
        {
            if(!(o is ListViewColumn columnInfo))
            {
                return;
            }
            columnInfo.Hidden = !columnInfo.Hidden;
        }

        private void Column_HiddenChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnColumnVisibilityChanged?.Invoke(sender as ListViewColumn);
        }
    }
}
