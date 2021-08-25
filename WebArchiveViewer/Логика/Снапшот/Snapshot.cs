using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchiveViewer
{
    //Сохраняемый снапшот
    public class Snapshot : NotifyObj
    {
        //Сведения о файлах
        public string FolderHtmlSavePath
        {
            get => folderHtmlSavePath ?? $"{System.IO.Directory.GetCurrentDirectory()}\\Ссылки";
            set
            {
                folderHtmlSavePath = value;
                OnPropertyChanged();
            }
        }
        private string folderHtmlSavePath;
        public string FilePath { get; set; }


        //Сведения о запросе
        public string Request { get; set; }
        public string SourceURI { get; set; }
        public DateTime ReceivingDate { get; set; }


        public ViewOptions ViewOptions { get; set; }
        public RulesControl RulesControl { get; set; }
        public GroupRule Rule { get; set; }
        public ArchiveLink[] Links { get; set; }


        public Snapshot()
        {

        }
        public Snapshot(Snapshot snapshot)
        {
            FolderHtmlSavePath = snapshot.FolderHtmlSavePath;
            FilePath = snapshot.FilePath;
            Request = snapshot.Request;
            SourceURI = snapshot.SourceURI;
            ReceivingDate = snapshot.ReceivingDate;
            ViewOptions = snapshot.ViewOptions;
            RulesControl = snapshot.RulesControl;
            Links = snapshot.Links;
        }
    }
}
