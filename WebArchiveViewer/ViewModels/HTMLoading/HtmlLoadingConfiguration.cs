using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WebArchive.Data;
using WebArchive.Data.HtmlLoading;

namespace WebArchiveViewer.ViewModels
{
    /// <summary>
    /// Конфигурация процесса загрузки ссылок
    /// </summary>
    public class HtmlLoadingConfiguration : NotifyObject
    {
        private SnapshotView Snapshot;

        public bool OnlyHtmlType
        {
            get => onlyHtmlType;
            set => Set(ref onlyHtmlType, value);
        }
        private bool onlyHtmlType;
        public bool OnlyWithoutTitles
        {
            get => onlyWithoutTitles;
            set => Set(ref onlyWithoutTitles, value);
        }
        private bool onlyWithoutTitles;

        /// <summary>
        /// Пропускать ссылки с уже загруженными файлами
        /// </summary>
        public bool IgnoreLoadedLinks
        {
            get => ignoreLoadedLinks;
            set
            {
                ignoreLoadedLinks = value;
                OnPropertyChanged();
            }
        }
        private bool ignoreLoadedLinks;

        /// <summary>
        /// Сохранение снапшота каждые n обработанных ссылок
        /// </summary>
        public int SavingLatencyLinks { get; set; }

        /// <summary>
        /// Обработка не более n ссылок
        /// </summary>
        public int LinksLimit { get; set; }

        public HtmlLoadingConfiguration() : this(500)
        {

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
            var filteredLinks = Snapshot.ListViewInfo.GetFilteredLinks()
                .Where(FilterOverall);
            return filteredLinks;
        }
        private bool FilterOverall(ArchiveLink link)
        {
            if (IgnoreLoadedLinks && IsFileExist(link))
            {
                return false;
            }
            if (OnlyHtmlType && link.MimeType != "text/html")
            {
                return false;
            }
            if (OnlyWithoutTitles && !string.IsNullOrEmpty(link.Name))
            {
                return false;
            }

            return true;
        }
        private bool IsFileExist(ArchiveLink link)
        {
            string futureFileName = LinkProcessingHelper.CreateFullFilePath(Snapshot.SavingHtmlFolder.FullName, link);
            FileInfo futureFileInfo = new FileInfo(futureFileName);
            return futureFileInfo.Exists;
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
