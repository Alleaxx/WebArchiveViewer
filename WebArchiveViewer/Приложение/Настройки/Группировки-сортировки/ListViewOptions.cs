using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WebArchive.Data;
namespace WebArchiveViewer
{
    //Сортировки-группировки
    public class ListViewOptions : NotifyObject
    {
        public event Action OnUpdated;

        private ISorting sortSelected;
        private IGrouping groupSelected;

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
            new Sorting("Порядок", l => "Index", false),
            new Sorting("Нет", null, true)
        };

        public ISorting SortSelected
        {
            get => sortSelected;
            set
            {
                sortSelected = value;
                OnPropertyChanged();
                Update();
            }
        }
        public IGrouping GroupSelected
        {
            get => groupSelected;
            set
            {
                groupSelected = value;
                OnPropertyChanged();
                Update();
            }
        }

        public ListViewOptions()
        {
            sortSelected = Sorts.Last();
            groupSelected = Groups.Last();
        }
        private void Update()
        {
            OnUpdated?.Invoke();
        }

        public IEnumerable<ArchiveLink> SortLinks(IEnumerable<ArchiveLink> links)
        {
            var sort = SortSelected;

            if(sort == null)
            {
                return links;
            }

            switch (sort.Name)
            {
                case "Имя":
                    return links.AsParallel().OrderBy(l => l.Name);
                case "Адрес":
                    return links.AsParallel().OrderBy(l => l.LinkSource);
                case "Дата":
                    return links.AsParallel().OrderBy(l => l.Date);
                case "Тип":
                    return links.AsParallel().OrderBy(l => l.MimeType);
                case "Порядок":
                    return links.AsParallel().OrderBy(l => l.Index);
                default:
                    return links;
            }
        }
    }
}
