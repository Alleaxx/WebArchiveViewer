using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchive.Data
{
    interface ILink
    {
        string Name { get; set; }
        string Link { get; }
    }
    public class UsualLink : ILink
    {
        public string Name { get; set; }
        public string Link { get; set; }
        public UsualLink(string name, string link)
        {
            Name = name;
            Link = link;
        }
    }
    public class ArchiveLink : NotifyObj, ILink
    {
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

        public const string DefaultName = "-";

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
