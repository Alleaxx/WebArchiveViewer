using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WebArchiveViewer
{
    //Разделение списка по страницам
    class Pager<T> : ObjNotify where T:class
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
        private int elementsPerPage = 24;

        public IEnumerable<T> Source { get; set; }


        public int PageMinAmount { get; set; } = 1;
        public int PageMaxAmount { get; set; }

        public int PageNowNumber
        {
            get => pageNowNumber;
            set
            {
                if (value < PageMinAmount)
                    value = PageMinAmount;
                if (value > PageMaxAmount)
                    value = PageMaxAmount;

                pageNowNumber = value;
                OnPropertyChanged();
                PageNow = new Page<T>(PageNowNumber, ElementsPerPage, Source);
                if(groupSelected != null)
                    GroupSelected = groupSelected;
            }
        }
        private int pageNowNumber;

        public Page<T> PageNow
        {
            get => pageNow;
            set
            {
                pageNow = value;
                OnPropertyChanged();
            }
        }
        private Page<T> pageNow;

        public Option<string> GroupSelected
        {
            get => groupSelected;
            set
            {
                groupSelected = value;
                OnPropertyChanged();
                PageNow.Source.View.GroupDescriptions.Clear();
                switch (value.Value)
                {
                    case "Тип":
                        PageNow.Source.View.GroupDescriptions.Add(new PropertyGroupDescription("MimeType"));
                        break;
                    case "Код":
                        PageNow.Source.View.GroupDescriptions.Add(new PropertyGroupDescription("StatusCode"));
                        break;
                    case "Категория":
                        PageNow.Source.View.GroupDescriptions.Add(new PropertyGroupDescription("Category"));
                        break;
                }
            }
        }
        private Option<string> groupSelected;

        public Pager(IEnumerable<T> coll)
        {
            Source = coll;
            Recount();
        }
        private void Recount()
        {
            PageMaxAmount = Convert.ToInt32(Math.Ceiling((double)Source.Count() / ElementsPerPage));
            OnPropertyChanged(nameof(PageMaxAmount));
            PageNowNumber = 1;
        }
    }

    //"Страница", содержащая определенное число элементов
    class Page<T> where T:class
    {
        public readonly int Number;
        public T[] Elements { get; set; }
        public CollectionViewSource Source { get; set; }

        public Page(int num,int perPage, IEnumerable<T> source)
        {
            Number = num;
            Elements = source.Skip((num - 1) * perPage).Take(perPage).ToArray();
            Source = new CollectionViewSource();
            Source.Source = Elements;
        }
    }
}
