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
    public class Pager<T> : NotifyObject, IPager<T> where T : class
    {
        public IEnumerable<T> Source { get; private set; }


        public IPage<T> PageNow
        {
            get => pageNow;
            set
            {
                pageNow = value;
                OnPropertyChanged();
            }
        }
        public int ElementsPerPage
        {
            get => elementsPerPage;
            set
            {
                elementsPerPage = value;
                OnPropertyChanged();
                RecalculateMaxPageAmount();
            }
        }
        public int PageMinAmount
        {
            get => pageMinAmount;
            set
            {
                pageMinAmount = value;
                OnPropertyChanged();
            }
        }
        public int PageMaxAmount
        {
            get => pageMaxAmount;
            set
            {
                pageMaxAmount = value;
                OnPropertyChanged();
            }
        }
        public int PageNowNumber
        {
            get => pageNowNumber;
            set
            {
                if (UpdateInProcess)
                {
                    return;
                }

                pageNowNumber = value < PageMinAmount ? PageMinAmount : value > PageMaxAmount ? PageMaxAmount : value;
                OnPropertyChanged();

                PageNow = new Page<T>(PageNowNumber, ElementsPerPage, Source);
                if (groupSelected != null)
                {
                    GroupSelected = groupSelected;
                }

                UpdateAvailablePagesArray();
            }
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


        private IPage<T> pageNow;
        private int pageNowNumber;
        public int pageMaxAmount;
        public int pageMinAmount;
        private int elementsPerPage;
        private int[] pagesAvailable;
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



        public Pager(IEnumerable<T> source, IGrouping grouping, IPager previousPager)
        {
            elementsPerPage = 50;
            PageMinAmount = 1;
            if(previousPager != null)
            {
                elementsPerPage = previousPager.ElementsPerPage;
            }
            groupSelected = grouping;

            NextPageCommand = new RelayCommand(NextPage, IsNextPageAvail);
            PrevPageCommand = new RelayCommand(PrevPage, IsPrevPageAvail);
            SetPageCommand = new RelayCommand(SetPage);

            Source = source;
            RecalculateMaxPageAmount();
        }

        private void RecalculateMaxPageAmount()
        {
            PageMaxAmount = Convert.ToInt32(Math.Ceiling((double)Source.Count() / ElementsPerPage));
            PageNowNumber = 1;
        }

        private bool UpdateInProcess { get; set; }
        private void UpdateAvailablePagesArray()
        {
            UpdateInProcess = true;
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
            UpdateInProcess = false;
        }


        //Команды
        public ICommand SetPageCommand { get; private set; }
        public ICommand PrevPageCommand { get; private set; }
        public ICommand NextPageCommand { get; private set; }

        private bool IsNextPageAvail(object obj) => PageNowNumber < PageMaxAmount;
        private bool IsPrevPageAvail(object obj) => PageNowNumber > PageMinAmount;
        
        private void SetPage(object obj)
        {
            if(obj is int page)
            {
                PageNowNumber = page;
            }
        }
        private void NextPage(object obj)
        {
            PageNowNumber++;
        }
        private void PrevPage(object obj)
        {
            PageNowNumber--;
        }
    }
}
