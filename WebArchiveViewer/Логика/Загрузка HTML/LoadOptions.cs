using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchiveViewer
{
    //Настройкf процесса загрузки
    public class LoadOptions : NotifyObj
    {
        public override string ToString()
        {
            return $"Настройки загрузки: {TimeoutMinutes}мин, {SavingLatencyLinks} сохр";
        }

        public TimeSpan Timeout => new TimeSpan(0, timeoutMinutes, 0);
        public int TimeoutMinutes
        {
            get => timeoutMinutes;
            set
            {
                timeoutMinutes = value;
                OnPropertyChanged();
            }
        }
        private int timeoutMinutes;



        public int SavingLatencyLinks { get; set; } = 10;
        private int CreateLatencyFromLinks(int links)
        {
            int latency = links / 10;
            int min = 10;
            if (latency < min)
            {
                latency = min;
            }
            return latency;
        }


        public LoadOptions() : this(500) { }
        public LoadOptions(int totalLinksAmount)
        {
            TimeoutMinutes = 10;
            SavingLatencyLinks = CreateLatencyFromLinks(totalLinksAmount);
        }
    }
}
