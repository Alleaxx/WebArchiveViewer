using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchive.Data
{
    public class DateRange : NotifyObject
    {
        public DateTime From
        {
            get => from;
            set
            {
                Set(ref from, value);
                OnPropertyChanged(nameof(DifferenceFrom));
            }
        }
        private DateTime from;
        public DateTime To
        {
            get => to;
            set
            {
                Set(ref to, value);
                OnPropertyChanged(nameof(DifferenceTo));
            }
        }
        private DateTime to;

        public DateTime Min { get; protected set; }
        public DateTime Max { get; protected set; }


        public double Difference => (Max - Min).TotalHours;
        public double DifferenceFrom
        {
            get => differenceFrom == -1 ? differenceFrom : (From - Min).TotalHours;
            set
            {
                From = Min.AddHours(value);
                Set(ref differenceFrom, value);
            }
        }
        private double differenceFrom;
        public double DifferenceTo
        {
            get => (To - Min).TotalHours;
            set
            {
                To = Min.AddHours(value);
                Set(ref differenceTo, value);
            }
        }
        private double differenceTo;

        public DateRange()
        {
            Min = new DateTime(1990, 1, 1);
            Max = DateTime.Now;
            From = new DateTime(2011, 7, 27);
            To = new DateTime(2012, 7, 27);
        }
        public DateRange(DateTime min, DateTime from, DateTime to, DateTime max)
        {
            Min = min;
            Max = max;
            From = from;
            To = to;
        }
    }
}
