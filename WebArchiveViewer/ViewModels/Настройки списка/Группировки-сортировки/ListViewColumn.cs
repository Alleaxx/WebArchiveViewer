using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using WebArchive.Data;
using WebArchiveViewer.UI.Commands;

namespace WebArchiveViewer.ViewModels
{
    public class ListViewColumn : NotifyObject
    {
        public ListViewInfo ViewInfo { get; private set; }

        public event Action<ListViewColumn> OnColumnUpdated;

        public string Name { get; set; }
        public int Order { get; set; }
        public bool Hidden
        {
            get => hidden;
            set
            {
                Set(ref hidden, value);
                OnPropertyChanged(nameof(Shown));
            }
        }
        private bool hidden;

        public bool Shown
        {
            get => !hidden;
            set
            {
                Hidden = !value;
            }
        }

        public ISorting Sorting { get; set; }
        public IGrouping Grouping { get; set; }

        public bool IsSortingSelected => ViewInfo.GroupSortsInfo.SortSelected == Sorting && !IsNoSort;
        public bool IsGroupingSelected => ViewInfo.GroupSortsInfo.GroupSelected == Grouping && !IsNoGroup;

        private bool IsNoSort => ViewInfo.GroupSortsInfo.NoSorting == Sorting;
        private bool IsNoGroup => ViewInfo.GroupSortsInfo.NoGrouping == Grouping;

        public ListViewColumn(ListViewInfo viewInfo)
        {
            ViewInfo = viewInfo;

            SortColumnCommand = new MenuCommand("Отсортировать по столбцу", OnSortCommandExecuted)
                .SetVisibleFunc(() => !IsNoSort);

            UnsortColumnCommand = new MenuCommand("Сбросить сортировку", OnUnsortCommandExecuted)
                .SetVisibleFunc(() => IsSortingSelected);

            GroupColumnCommand = new MenuCommand("Сгруппировать по столбцу", OnGroupCommandExecuted)
                .SetVisibleFunc(() => !IsNoGroup);

            HideColumnCommand = new MenuCommand("Скрытие столбца", OnHideColumnCommandExecuted)
                .SetTitleFunc(() => Hidden ? "Отобразить столбец" : "Скрыть столбец");


            MenuCommands = new MenuCommand[]
            {
                SortColumnCommand as MenuCommand,
                UnsortColumnCommand as MenuCommand,
                GroupColumnCommand as MenuCommand,
                HideColumnCommand as MenuCommand
            };
        }


        public IEnumerable<MenuCommand> MenuCommands { get; private set; }

        public ICommand SortColumnCommand { get; private set; }
        public ICommand GroupColumnCommand { get; private set; }
        public ICommand HideColumnCommand { get; private set; }
        public ICommand UnsortColumnCommand { get; private set; }

        private void OnSortCommandExecuted(object obj)
        {
            var sort = Sorting;
            if (sort == null)
            {
                return;
            }

            if (IsSortingSelected)
            {
                ViewInfo.GroupSortsInfo.SortSelected.ToggleOrder();
                ViewInfo.GroupSortsInfo.Update();
            }
            else
            {
                ViewInfo.GroupSortsInfo.SortSelected = sort;
            }
            UpdatePropertiesWithEvent();
        }
        private void OnUnsortCommandExecuted(object obj)
        {
            ViewInfo.GroupSortsInfo.SortSelected = ViewInfo.GroupSortsInfo.NoSorting;
            UpdatePropertiesWithEvent();
        }
        private void OnGroupCommandExecuted(object obj)
        {
            var group = Grouping;
            if (group == null)
            {
                return;
            }

            if (IsGroupingSelected)
            {
                ViewInfo.GroupSortsInfo.GroupSelected = ViewInfo.GroupSortsInfo.Groups.Last();
            }
            else
            {
                ViewInfo.GroupSortsInfo.GroupSelected = group;
            }
            UpdatePropertiesWithEvent();
        }
        public void OnHideColumnCommandExecuted(object o)
        {
            Hidden = !Hidden;
        }

        private void UpdatePropertiesWithEvent()
        {
            OnColumnUpdated?.Invoke(this);
            UpdateProperties();
        }
        public void UpdateProperties()
        {
            OnPropertyChanged(nameof(IsSortingSelected));
            OnPropertyChanged(nameof(IsGroupingSelected));
        }
    }
}
