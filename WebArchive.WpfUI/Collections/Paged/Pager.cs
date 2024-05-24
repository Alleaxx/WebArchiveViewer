using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using WebArchive.WpfUI;
using WebArchiveViewer.WpfUI.Commands;

namespace WebArchiveViewer.WpfUI.Collections.Paged
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
        T Selected { get; set; }
        void UpdateCollection(IEnumerable<T> source);
    }
    public class Pager<T> : NotifyWpfObject, IPager<T> where T : class
    {
        public IEnumerable<T> Source { get; private set; }
        public T Selected
        {
            get => selected;
            set => Set(ref selected, value);
        }
        private T selected;

        public IPage<T> PageNow
        {
            get => pageNow;
            set => Set(ref pageNow, value);
        }
        public int ElementsPerPage
        {
            get => elementsPerPage;
            set
            {
                Set(ref elementsPerPage, value);
                RecalculateMaxPageAmount();
            }
        }
        public int PageMinAmount
        {
            get => pageMinAmount;
            set => Set(ref pageMinAmount, value);
        }
        public int PageMaxAmount
        {
            get => pageMaxAmount;
            set => Set(ref pageMaxAmount, value);
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

                Set(ref pageNowNumber, value < PageMinAmount ? PageMinAmount : value > PageMaxAmount ? PageMaxAmount : value);

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
            private set => Set(ref pagesAvailable, value);
        }
        public IGrouping GroupSelected
        {
            get => groupSelected;
            set
            {
                Set(ref groupSelected, value);
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



        private bool UpdateInProcess { get; set; }
        public ICommand SetPageCommand { get; private set; }
        public ICommand PrevPageCommand { get; private set; }
        public ICommand NextPageCommand { get; private set; }


        public Pager()
        {
            elementsPerPage = 1000;
            PageMinAmount = 1;

            NextPageCommand = new RelayCommand(NextPage)
                .SetCondition(IsNextPageAvail);
            PrevPageCommand = new RelayCommand(PrevPage)
                .SetCondition(IsPrevPageAvail);
            SetPageCommand = new RelayCommand(SetPage);

            UpdateCollection(Array.Empty<T>());
        }
        public void UpdateCollection(IEnumerable<T> source)
        {
            Source = source;
            RecalculateMaxPageAmount();
        }

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
        private void RecalculateMaxPageAmount()
        {
            PageMaxAmount = Convert.ToInt32(Math.Ceiling((double)Source.Count() / ElementsPerPage));
            PageNowNumber = 1;
        }

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


        private bool IsNextPageAvail()
        {
            return PageNowNumber < PageMaxAmount;
        }
        private bool IsPrevPageAvail()
        {
            return PageNowNumber > PageMinAmount;
        }
        
        private void SetPage(object obj)
        {
            if(obj is int page)
            {
                PageNowNumber = page;
            }
        }
        private void NextPage()
        {
            PageNowNumber++;
        }
        private void PrevPage()
        {
            PageNowNumber--;
        }
    }
}
