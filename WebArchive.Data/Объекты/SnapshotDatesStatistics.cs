using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchive.Data
{
    public class SnapshotDateStatistics
    {
        public int MaxLinksForDay { get; private set; }
        public IEnumerable<DayLinkStat> DayLinkStats { get; private set; }

        public SnapshotDateStatistics(Snapshot snapshot)
        {
            DayLinkStats = new List<DayLinkStat>();
            if(snapshot.IsEmpty)
            {
                MaxLinksForDay = 1;
                return;
            }
            UpdateWithSnapshot(snapshot);
        }
        public void UpdateWithSnapshot(Snapshot snapshot)
        {
            List<DayLinkStat> linksList = new List<DayLinkStat>();

            var links = snapshot.Links;
            DateTime firstDate = links.OrderBy(l => l.Date).Select(l => l.Date).First();
            DateTime lastDate = links.OrderBy(l => l.Date).Select(l => l.Date).Last();
            int days = (lastDate - firstDate).Days + 1;
            var grouped = links.GroupBy(l => Math.Round((double)(l.Date - firstDate).Days, 0));
            foreach (var group in grouped)
            {
                int dayPosition = (int)group.Key;
                linksList.Add(new DayLinkStat()
                {
                    DayPosition = dayPosition,
                    LinksCount = group.Count()
                });
            }

            DayLinkStats = Enumerable.Range(0, days).Select(i => new DayLinkStat()
            {
                DayPosition = i,
                LinksCount = grouped.FirstOrDefault(g => g.Key == i)?.Count() ?? 0
            });

            DayLinkStats = DayLinkStats.OrderBy(l => l.DayPosition).ToArray();

            MaxLinksForDay = DayLinkStats.Max(l => l.LinksCount);

            if(MaxLinksForDay == 1)
            {
                MaxLinksForDay = 3;
            }
        }
    }
    public class DayLinkStat
    {
        //Номер дня с минимальной даты
        public int DayPosition { get; set; }

        //Количество ссылок на указанную дату
        public int LinksCount { get; set; }
    }
}
