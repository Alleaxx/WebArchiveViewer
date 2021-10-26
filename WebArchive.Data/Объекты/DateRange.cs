using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchive.Data
{
    public class DateRange : NotifyObj
    {
        public DateTime From
        {
            get => from;
            set
            {
                from = value;
                OnPropertyChanged();
            }
        }
        private DateTime from;
        public DateTime To
        {
            get => to;
            set
            {
                to = value;
                OnPropertyChanged();
            }
        }
        private DateTime to;


        public DateTime Min { get; protected set; }
        public DateTime Max { get; protected set; }


        [JsonIgnore]
        public double Difference => (Max - Min).TotalHours;
        [JsonIgnore]
        public double DifferenceFrom
        {
            get => (From - Min).TotalHours;
            set => From = Min.AddHours(value);
        }
        [JsonIgnore]
        public double DifferenceTo
        {
            get => (To - Min).TotalHours;
            set => To = Min.AddHours(value);
        }


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
