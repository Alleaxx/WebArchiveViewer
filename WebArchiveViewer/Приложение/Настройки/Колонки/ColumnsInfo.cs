using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchiveViewer
{
    public class ColumnsInfo
    {
        public event Action<ListViewColumn> OnColumnVisibilityChanged;
        public List<ListViewColumn> Columns { get; private set; }

        public ColumnsInfo()
        {
            Columns = new List<ListViewColumn>();
        }

        public ColumnsInfo AddColumn(string name, bool hidden = false)
        {
            int order = Columns.Count;
            var newCol = new ListViewColumn()
            {
                Name = name,
                Order = order,
                Hidden = hidden
            };
            Columns.Add(newCol);
            newCol.PropertyChanged += Column_HiddenChanged;
            return this;
        }

        private void Column_HiddenChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnColumnVisibilityChanged?.Invoke(sender as ListViewColumn);
        }
    }
}
