using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WebArchive.Data;

namespace WebArchiveViewer.ViewModels
{
    /// <summary>
    /// Конфигурация процесса загрузки ссылок
    /// </summary>
    public class HtmlLoadingConfiguration : NotifyObject
    {
        private SnapshotView Snapshot;

        /// <summary> Сохранение снапшота каждые n обработанных ссылок </summary>
        public int SavingLatencyLinks { get; set; }

        /// <summary> Обработка не более n ссылок </summary>
        public int LinksLimit { get; set; }

        public HtmlLoadingConfiguration()
        {
            UpdateWithLinksAmount(500);
        }
        public HtmlLoadingConfiguration(int totalLinksAmount)
        {
            UpdateWithLinksAmount(totalLinksAmount);
        }

        public void SetLinksSource(SnapshotView snapshot)
        {
            Snapshot = snapshot;
        }
        public IEnumerable<ArchiveLink> GetFilteredLinks()
        {
            return Snapshot.ListViewInfo.GetFilteredLinks();
        }

        public void UpdateWithLinksAmount(int amount)
        {
            SavingLatencyLinks = GetLatencyFromLinksCount(amount);
        }
        private int GetLatencyFromLinksCount(int links)
        {
            int latency = links / 10;
            int min = 10;
            if (latency < min)
            {
                latency = min;
            }
            return latency;
        }
    }
}
