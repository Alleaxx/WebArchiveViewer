using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WebArchiveViewer.ViewModels
{
    /// <summary>
    /// Информация об отображаемых колонках в списке ссылок
    /// </summary>
    public class ListViewColumnsInfo
    {
        private readonly ListViewInfo ListViewInfo;

        public List<ListViewColumn> Columns { get; private set; }

        public ListViewColumnsInfo(ListViewInfo viewInfo)
        {
            ListViewInfo = viewInfo;
            Columns = new List<ListViewColumn>();
        }

        public ListViewColumnsInfo AddColumn(string name, SortsEnum sort, GroupsEnum group, bool hidden = false)
        {
            int order = Columns.Count;
            var newCol = new ListViewColumn(ListViewInfo)
            {
                Name = name,
                Order = order,
                Hidden = hidden,
                Sorting = ListViewInfo.GroupSortsInfo.GetSort(sort),
                Grouping = ListViewInfo.GroupSortsInfo.GetGroup(group)
            };
            Columns.Add(newCol);
            newCol.OnColumnUpdated += OnColumnUpdated;
            return this;
        }

        private void OnColumnUpdated(ListViewColumn obj)
        {
            foreach (var col in Columns)
            {
                col.UpdateProperties();
            }
        }
    }
}
