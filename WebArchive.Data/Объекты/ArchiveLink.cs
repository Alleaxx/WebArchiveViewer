using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchive.Data
{
    public class ArchiveLink : NotifyObject, ILink
    {
        public const string DefaultName = "-";

        public override string ToString()
        {
            return Link;
        }

        public string Name
        {
            get => name;
            set => Set(ref name, value);
        }
        private string name;

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
            set => Set(ref category, value);
        }
        private string category;

        public string HtmlFilePath
        {
            get => htmlFilePath;
            set => Set(ref htmlFilePath, value);
        }
        private string htmlFilePath;

        public bool IsBlacklisted
        {
            get => isBlacklisted;
            set
            {
                Set(ref isBlacklisted, value);
                if(Information != null)
                {
                    foreach (var link in Information.AllLinks)
                    {
                        link.SetBlackListed(isBlacklisted);
                    }
                }
            }
        }
        private bool isBlacklisted;
        [JsonIgnore]
        public ArchiveLinkInfo Information { get; set; }
        [JsonIgnore]
        public bool IsUniq { get; set; }

        public ArchiveLink()
        {
            name = DefaultName;
            category = "Общее";
        }

        private void SetBlackListed(bool isBlacklisted)
        {
            this.isBlacklisted=isBlacklisted;
            OnPropertyChanged(nameof(IsBlacklisted));
        }
    }
}
