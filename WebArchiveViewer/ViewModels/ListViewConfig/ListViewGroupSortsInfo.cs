using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WebArchive.Data;
using WebArchiveViewer.WpfUI.Collections;
using WebArchiveViewer.WpfUI.Commands;

namespace WebArchiveViewer.ViewModels
{
    /// <summary>
    /// Информация о применяемых сортировках и группировках для списка
    /// </summary>
    public class ListViewGroupSortsInfo : NotifyObject
    {
        public event Action OnUpdated;

        public IEnumerable<IGrouping> Groups { get; private set; } = new IGrouping[]
        {
            new Grouping(GroupsEnum.LinkURL, "Ссылка", "LinkSource", false),
            new Grouping(GroupsEnum.PageName, "Имя ссылки", "Name", false),
            new Grouping(GroupsEnum.MimeType, "Тип","MimeType", false),
            new Grouping(GroupsEnum.StatusCode, "Код","StatusCode", false),
            new Grouping(GroupsEnum.Category, "Категория","Category", false),
            new Grouping(GroupsEnum.None, "Нет", null, true)
        };
        public IEnumerable<ISorting> Sorts { get; private set; } = new ISorting[]
        {
            new Sorting(SortsEnum.Date, "Дата", null, false),
            new Sorting(SortsEnum.PageName, "Имя", l => l.Name, false),
            new Sorting(SortsEnum.LinkURL, "Адрес", l => l.LinkSource, false),
            new Sorting(SortsEnum.MimeType, "Тип", l => l.MimeType, false),
            new Sorting(SortsEnum.StatusCode, "Код", l => l.StatusCode, false),
            new Sorting(SortsEnum.Category, "Категория", l => l.Category, false),
            new Sorting(SortsEnum.LinkIndex, "Порядок", l => "Index", false),
            new Sorting(SortsEnum.Tag, "Тег", l => l.Tag, false),
            new Sorting(SortsEnum.None, "Нет", null, true)
        };

        public IGrouping NoGrouping { get; private set; }
        public ISorting NoSorting { get; private set; }

        public ISorting SortSelected
        {
            get => sortSelected;
            set => Set(ref sortSelected, value);
        }
        private ISorting sortSelected;
        public IGrouping GroupSelected
        {
            get => groupSelected;
            set => Set(ref groupSelected, value);
        }
        private IGrouping groupSelected;

        public ICommand RemoveSortCommand { get; private set; }

        public ListViewGroupSortsInfo()
        {
            sortSelected = Sorts.ElementAt(Sorts.Count() - 1);
            groupSelected = Groups.Last();
            NoGrouping = Groups.Last();
            NoSorting = Sorts.Last();

            RemoveSortCommand = new RelayCommand(OnRemoveSortCommandExecuted);
        }

        private void OnRemoveSortCommandExecuted(object obj)
        {
            SortSelected = NoSorting;
        }
        public void Update()
        {
            OnUpdated?.Invoke();
        }
        protected override bool Set<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            var res = base.Set(ref field, value, propertyName);
            if (res)
            {
                Update();
            }
            return res;
        }


        public IGrouping GetGroup(GroupsEnum type)
        {
            return Groups.First(s => s.Type == type);
        }
        public ISorting GetSort(SortsEnum type)
        {
            return Sorts.First(s => s.Type == type);
        }
        public IEnumerable<ArchiveLink> SortLinks(IEnumerable<ArchiveLink> links)
        {
            var sort = SortSelected;

            if(sort == null)
            {
                return links;
            }

            IEnumerable<ArchiveLink> linksResult = null;
            switch (sort.Name)
            {
                case "Имя":
                    linksResult = links.AsParallel().OrderBy(l => l.Name);
                    break;
                case "Адрес":
                    linksResult = links.AsParallel().OrderBy(l => l.LinkSource);
                    break;
                case "Дата":
                    linksResult = links.AsParallel().OrderBy(l => l.Date);
                    break;
                case "Тип":
                    linksResult = links.AsParallel().OrderBy(l => l.MimeType);
                    break;
                case "Порядок":
                    linksResult = links.AsParallel().OrderBy(l => l.Index);
                    break;
                case "Код":
                    linksResult = links.AsParallel().OrderBy(l => l.StatusCode);
                    break;
                case "Категория":
                    linksResult = links.AsParallel().OrderBy(l => l.Category);
                    break;
                default:
                    return links;
            }

            if (!sort.Ascending)
            {
                linksResult = linksResult.Reverse();
            }

            return linksResult;
        }
    }
}
