using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchive.Data
{
    public class ArchiveLink : NotifyObj, ILink
    {
        public const string DefaultName = "-";

        public override string ToString()
        {
            return Link;
        }


        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }
        private string name = DefaultName;

        public int Index { get; set; }
        public string TimeStamp { get; set; }
        public DateTime Date { get; set; }
        public string LinkSource { get; set; }
        public string Link => $"https://web.archive.org/web/{TimeStamp}/{LinkSource}";
        public string MimeType { get; set; }
        public string StatusCode { get; set; }

        public string Category
        {
            get => category;
            set
            {
                category = value;
                OnPropertyChanged();
            }
        }
        private string category = "Общее";

        public string HtmlFilePath
        {
            get => htmlFilePath;
            set
            {
                htmlFilePath = value;
                OnPropertyChanged();
            }
        }
        private string htmlFilePath;

        public ArchiveLink()
        {

        }
    }
}
