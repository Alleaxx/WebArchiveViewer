using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WebArchive.Data;
namespace WebArchiveViewer
{
    //Настройкf процесса загрузки
    public class LoadConfiguration : NotifyObject
    {
        public override string ToString()
        {
            return $"Настройки загрузки: {TimeoutMinutes}мин, {SavingLatencyLinks} сохр";
        }

        public TimeSpan Timeout => new TimeSpan(0, timeoutMinutes, 0);
        public int TimeoutMinutes
        {
            get => timeoutMinutes;
            set => Set(ref timeoutMinutes, value);
        }
        private int timeoutMinutes;

        public int SavingLatencyLinks { get; set; }
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


        public LoadConfiguration()
        {
            UpdateWithLinksAmount(500);
        }
        public LoadConfiguration(int totalLinksAmount)
        {
            UpdateWithLinksAmount(totalLinksAmount);
        }

        public void UpdateWithLinksAmount(int amount)
        {
            SavingLatencyLinks = 10;
            TimeoutMinutes = 10;
            SavingLatencyLinks = CreateLatencyFromLinks(amount);
        }
    }
}
