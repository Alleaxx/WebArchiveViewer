using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchive.Data
{
    /// <summary>
    /// Сохраняемый снапшот (список ссылок полученных из запроса)
    /// </summary>
    public class Snapshot : ICloneable
    {
        public string FolderHtmlSavePath { get; set; }
        public string FilePath { get; set; }


        public string Request { get; set; }
        public string SourceURI { get; set; }
        public DateTime ReceivingDate { get; set; }


        public GroupRule RulesControl { get; set; }
        public ArchiveLink[] Links { get; set; }

        public bool IsEmpty => !Links.Any();
        public bool IsNotEmpty => Links.Any();


        /// <summary>
        /// Из сохранения
        /// </summary>
        public Snapshot()
        {

        }
        /// <summary>
        /// Из архива
        /// </summary>
        public Snapshot(string request, string sourceLink, IEnumerable<ArchiveLink> links)
        {
            FolderHtmlSavePath = $"{Directory.GetCurrentDirectory()}\\Ссылки";
            ReceivingDate = DateTime.Now;
            Request = request;
            SourceURI = sourceLink;
            Links = links.ToArray();

            SetRulesIfNull();
        }

        public static Snapshot GetEmptySnapshot()
        {
            var empty = new Snapshot();

            empty.Links = Array.Empty<ArchiveLink>();
            empty.FolderHtmlSavePath = Directory.GetCurrentDirectory();
            empty.SourceURI = "Ссылок нет, но вы держитесь";
            empty.ReceivingDate = DateTime.Now;
            empty.RulesControl = new GroupRule();
            empty.RulesControl.AddInner(RulesStorage.Rumine());

            return empty;
        }

        public void ClearNonExistantFilePathes()
        {
            foreach (var link in Links)
            {
                if (!string.IsNullOrEmpty(link.HtmlFilePath) && !System.IO.File.Exists(link.HtmlFilePath))
                {
                    link.HtmlFilePath = null;
                }
            }
        }

        public void Posprocessing()
        {
            LoadLinksInformation();
        }
        private void LoadLinksInformation()
        {
            Dictionary<string, ArchiveLinkInfo> linksInfo = new Dictionary<string, ArchiveLinkInfo>();
            foreach(var link in Links)
            {
                string key = link.GetSourceLinkUniq();
                if (!linksInfo.ContainsKey(key))
                {
                    linksInfo.Add(key, new ArchiveLinkInfo(link));
                }
                else
                {
                    linksInfo[key].AddLink(link);
                }
            }
        }



        public void UpdateCategories()
        {
            SetRulesIfNull();
            foreach (ArchiveLink link in Links)
            {
                link.Category = RulesControl.CheckLink(link);
            }
        }
        private void SetRulesIfNull()
        {
            if(RulesControl != null)
            {
                return;
            }
            RulesControl = new GroupRule();
            if (SourceURI.Contains("ru-minecraft.ru"))
            {
                RulesControl.AddInner(RulesStorage.Rumine());
            }
        }


        public object Clone()
        {
            return CloneThis(Links);
        }
        public Snapshot CloneThis(IEnumerable<ArchiveLink> links)
        {
            var copy = MemberwiseClone() as Snapshot;
            copy.Links = links.ToArray();
            return copy;
        }
    }
}
