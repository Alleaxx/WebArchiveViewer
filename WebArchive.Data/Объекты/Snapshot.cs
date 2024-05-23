using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchive.Data
{
    //Сохраняемый снапшот (список ссылок полученных из запроса)
    public class Snapshot : ICloneable
    {
        //Сведения о файлах
        public string FolderHtmlSavePath { get; set; }
        public string FilePath { get; set; }


        //Сведения о запросе
        public string Request { get; set; }
        public string SourceURI { get; set; }
        public DateTime ReceivingDate { get; set; }


        public GroupRule RulesControl { get; set; }
        public ArchiveLink[] Links { get; set; }

        public bool IsEmpty => !Links.Any();
        public bool IsNotEmpty => Links.Any();


        //Из сохранения
        public Snapshot()
        {

        }
        //Из архива
        public Snapshot(string request, string sourceLink, IEnumerable<ArchiveLink> links)
        {
            FolderHtmlSavePath = $"{Directory.GetCurrentDirectory()}\\Ссылки";
            ReceivingDate = DateTime.Now;
            Request = request;
            SourceURI = sourceLink;
            Links = links.ToArray();

            LoadLinksInformation();
            SetRulesIfNull();
        }
        public static Snapshot GetEmptySnapshot()
        {
            var empty = new Snapshot();

            empty.Links = Array.Empty<ArchiveLink>();
            empty.FolderHtmlSavePath = Directory.GetCurrentDirectory();
            empty.SourceURI = "Ссылок нет, но вы держитесь";
            empty.ReceivingDate = DateTime.Now;

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

        public void LoadLinksInformation()
        {
            Dictionary<string, ArchiveLinkInfo> linksInfo = new Dictionary<string, ArchiveLinkInfo>();
            foreach(var link in Links)
            {
                string key = link.LinkSource;
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

        //Категории, их обновление
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


        //Копия для сохранения
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
