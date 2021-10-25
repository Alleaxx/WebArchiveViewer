using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchiveViewer
{
    //Настройки загрузки
    public class LoadOptions : NotifyObj
    {
        public override string ToString()
        {
            return $"Настройки загрузки: {LatencyMs}мс, {MaxRequestsSimultaneosly} макс, {SavingLatencyLinks} сохр";
        }

        public int LatencyMs
        {
            get => latencyMs;
            set
            {
                latencyMs = value;
                OnPropertyChanged();
            }
        }
        private int latencyMs = 1000;
        public int MaxRequestsSimultaneosly { get; set; } = 5;
        public int SavingLatencyLinks { get; set; } = 10;
        private int GetSavingLatency(int count)
        {
            int latency = count / 10;
            int min = 10;
            if (latency < min)
                latency = min;
            return latency;
        }

        public LoadOptions() : this(500) { }
        public LoadOptions(int amount)
        {
            SavingLatencyLinks = GetSavingLatency(amount);
        }

    }
}
