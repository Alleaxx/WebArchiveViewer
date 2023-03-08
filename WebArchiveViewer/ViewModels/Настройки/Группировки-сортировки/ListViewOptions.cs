using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WebArchive.Data;
namespace WebArchiveViewer
{
    //Сортировки-группировки
    public class ListViewOptions : NotifyObject
    {
        public event Action OnUpdated;

        private ISorting sortSelected;
        private IGrouping groupSelected;

        protected override bool Set<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            var res = base.Set(ref field, value, propertyName);
            if (res)
            {
                Update();
            }
            return res;
        }

        public IEnumerable<IGrouping> Groups { get; private set; } = new IGrouping[]
        {
            new Grouping("Ссылка", "LinkSource", false),
            new Grouping("Имя ссылки", "Name", false),
            new Grouping("Тип","MimeType",false),
            new Grouping("Код","StatusCode",false),
            new Grouping("Категория","Category",false),
            new Grouping("Нет", null, true)
        };
        public IEnumerable<ISorting> Sorts { get; private set; } = new ISorting[]
        {
            new Sorting("Дата", null, false),
            new Sorting("Имя", l => l.Name, false),
            new Sorting("Адрес", l => l.LinkSource, false),
            new Sorting("Тип", l => l.MimeType, false),
            new Sorting("Код", l => l.StatusCode, false),
            new Sorting("Категория", l => l.Category, false),
            new Sorting("Порядок", l => "Index", false),
            new Sorting("Нет", null, true)
        };

        public ISorting SortSelected
        {
            get => sortSelected;
            set => Set(ref sortSelected, value);
        }
        public IGrouping GroupSelected
        {
            get => groupSelected;
            set => Set(ref groupSelected, value);
        }

        public ListViewOptions()
        {
            sortSelected = Sorts.ElementAt(Sorts.Count() - 1);
            groupSelected = Groups.Last();
            SortCommand = new RelayCommand(OnSortCommandExecuted);
        }
        private void Update()
        {
            OnUpdated?.Invoke();
        }

        public ICommand SortCommand { get; private set; }

        private void OnSortCommandExecuted(object obj)
        {
            if(!(obj is string propertyName))
            {
                return;
            }

            var sort = Sorts.FirstOrDefault(s => s.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase));
            if(sort == null)
            {
                return;
            }

            if(SortSelected == sort)
            {
                SortSelected.ToggleOrder();
                Update();
            }
            else
            {
                SortSelected = sort;
            }
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
