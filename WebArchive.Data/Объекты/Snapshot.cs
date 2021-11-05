using System;
using System.Collections.Generic;
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




        //Из сохранения
        public Snapshot()
        {

        }
        //Из архива
        public Snapshot(string request, string sourceLink, IEnumerable<ArchiveLink> links)
        {
            FolderHtmlSavePath = $"{System.IO.Directory.GetCurrentDirectory()}\\Ссылки";
            ReceivingDate = DateTime.Now;
            Request = request;
            SourceURI = sourceLink;
            Links = links.ToArray();

            SetRulesIfNull();
        }

        public void InitAfterLoad()
        {
            foreach (var link in Links)
            {
                if (!string.IsNullOrEmpty(link.HtmlFilePath) && !System.IO.File.Exists(link.HtmlFilePath))
                {
                    link.HtmlFilePath = null;
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
            if (RulesControl == null)
            {
                RulesControl = new GroupRule();
                if (SourceURI.Contains("ru-minecraft.ru"))
                {
                    RulesControl.AddInner(RulesStorage.Rumine());
                }
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
