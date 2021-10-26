using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

using WebArchive.Data;
namespace WebArchiveViewer
{
    //Разделение списка по страницам
    public interface IPager
    {
        int ElementsPerPage { get; set; }
        IGrouping GroupSelected { get; set; }
    }
    public interface IPager<T> : IPager
    {
        IEnumerable<T> Source { get; }
        IPage<T> PageNow { get; }
    }
    public class Pager<T> : NotifyObj, IPager<T> where T:class
    {
        public int ElementsPerPage
        {
            get => elementsPerPage;
            set
            {
                elementsPerPage = value;
                OnPropertyChanged();
                Recount();
            }
        }
        private int elementsPerPage = 50;

        public IEnumerable<T> Source { get; private set; }


        public int PageMinAmount { get; private set; } = 1;
        public int PageMaxAmount { get; private set; }

        public int PageNowNumber
        {
            get => pageNowNumber;
            set
            {
                if (!Updating)
                {
                    if (value < PageMinAmount)
                        value = PageMinAmount;
                    if (value > PageMaxAmount)
                        value = PageMaxAmount;

                    pageNowNumber = value;
                    OnPropertyChanged();

                    PageNow = new Page<T>(PageNowNumber, ElementsPerPage, Source);
                    if (groupSelected != null)
                        GroupSelected = groupSelected;

                    UpdatePagesAvailable();
                }
            }
        }
        private int pageNowNumber;

        public IPage<T> PageNow
        {
            get => pageNow;
            set
            {
                pageNow = value;
                OnPropertyChanged();
            }
        }
        private IPage<T> pageNow;


        public IGrouping GroupSelected
        {
            get => groupSelected;
            set
            {
                groupSelected = value;
                OnPropertyChanged();
                SetGrouping(value);
            }
        }
        private IGrouping groupSelected;
        private void SetGrouping(IGrouping value)
        {
            var descriptions = PageNow.Source.View.GroupDescriptions;
            descriptions.Clear();
            string grouping = value.Key;
            if (!string.IsNullOrEmpty(grouping))
            {
                descriptions.Add(new PropertyGroupDescription(grouping));
            }
        }



        public Pager(IEnumerable<T> coll, IGrouping group, IPager prevPager)
        {
            if(prevPager != null)
            {
                elementsPerPage = prevPager.ElementsPerPage;
            }
            groupSelected = group;

            Source = coll;
            Recount();
        }
        protected override void InitCommands()
        {
            NextPageCommand = new RelayCommand(NextPage, IsNextPageAvail);
            PrevPageCommand = new RelayCommand(PrevPage, IsPrevPageAvail);
            SetPageCommand = new RelayCommand(SetPage);
        }

        private void Recount()
        {
            PageMaxAmount = Convert.ToInt32(Math.Ceiling((double)Source.Count() / ElementsPerPage));
            OnPropertyChanged(nameof(PageMaxAmount));
            PageNowNumber = 1;
        }

        public int[] PagesAvailable
        {
            get => pagesAvailable;
            private set
            {
                pagesAvailable = value;
                OnPropertyChanged();
            }
        }
        private int[] pagesAvailable;

        private bool Updating { get; set; }
        private void UpdatePagesAvailable()
        {
            Updating = true;
            int size = 10;
            List<int> arr = new List<int>(size) { pageNowNumber };

            int counter = 1;
            bool spaceExist = arr.Count <= size;
            bool pagesExist = arr.Count <= PageMaxAmount - PageMinAmount;
            while(spaceExist && pagesExist)
            {
                int prev = pageNowNumber - counter;
                int next = pageNowNumber + counter;

                if (prev >= PageMinAmount)
                    arr.Insert(0, prev);
                if (next <= PageMaxAmount)
                    arr.Add(next);

                spaceExist = arr.Count <= 10;
                pagesExist = arr.Count <= PageMaxAmount - PageMinAmount;

                counter++;
            }

            PagesAvailable = arr.ToArray();
            Updating = false;
        }


        public ICommand SetPageCommand { get; private set; }
        private void SetPage(object obj)
        {
            if(obj is int page)
            {
                PageNowNumber = page;
            }
        }


        public ICommand NextPageCommand { get; private set; }
        private void NextPage(object obj)
        {
            PageNowNumber++;
        }
        private bool IsNextPageAvail(object obj) => PageNowNumber < PageMaxAmount;


        public ICommand PrevPageCommand { get; private set; }
        private void PrevPage(object obj)
        {
            PageNowNumber--;
        }
        private bool IsPrevPageAvail(object obj) => PageNowNumber > PageMinAmount;
    }


    //"Страница", содержащая определенное число элементов
    public interface IPage<T>
    {
        int Number { get; }
        IEnumerable<T> Elements { get; }
        CollectionViewSource Source { get; }
    }
    public class Page<T> : IPage<T> where T:class
    {
        public int Number { get; private set; }
        public IEnumerable<T> Elements { get; private set; }
        public CollectionViewSource Source { get; private set; }

        public Page(int num,int perPage, IEnumerable<T> source)
        {
            Number = num;
            Elements = source.Skip((num - 1) * perPage).Take(perPage).ToArray();
            Source = new CollectionViewSource();
            Source.Source = Elements;
        }
    }
}
