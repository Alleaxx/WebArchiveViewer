using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchiveViewer
{
    //Сортировки-группировки
    public class ListViewOptions : NotifyObj
    {
        public event Action Updated;

        public IEnumerable<IGrouping> Groups { get; private set; } = new IGrouping[]
        {
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
        private ISorting sortSelected;

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
        private IGrouping groupSelected;

        public ListViewOptions()
        {
            sortSelected = Sorts.Last();
            groupSelected = Groups.Last();
        }

        private void Update()
        {
            Updated?.Invoke();
        }

        public IEnumerable<ArchiveLink> SortLinks(ISnapshot snapshot)
        {
            var links = snapshot.Links;
            var sort = SortSelected;

            if (sort != null)
            {
                switch (sort.Name)
                {
                    case "Имя":
                        return links.OrderBy(l => l.Name);
                    case "Адрес":
                        return links.OrderBy(l => l.LinkSource);
                    case "Дата":
                        return links.OrderBy(l => l.Date);
                    case "Тип":
                        return links.OrderBy(l => l.MimeType);
                    default:
                        return links;
                }
            }
            else
                return links;
        }
    }


    public interface ISorting
    {
        string Name { get; }
        Func<IArchLink, string> KeySelector { get; }
    }
    class Sorting : ISorting
    {
        public string Name { get; private set; }
        public Func<IArchLink, string> KeySelector { get; private set; }


        public Sorting(string name, Func<IArchLink, string> func, bool enabled)
        {
            Name = name;
            KeySelector = func;

        }
    }

    public interface IGrouping
    {
        string Name { get; }
        string Key { get; }
    }
    class Grouping : IGrouping
    {
        public string Name { get; private set; }
        public string Key { get; private set; }

        public Grouping(string name, string key, bool enabled)
        {
            Name = name;
            Key = key;
        }
    }


}
