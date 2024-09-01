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
        public const string DefaultName = "";
        public static readonly string RemoveTag = "К удалению";
        public static readonly string[] AllTags = new string[]
        {
            RemoveTag,
            "Неизвестно",
            "Отобрано",
            "Изучено",
            "Избранное",
        };

        public override string ToString()
        {
            return Link;
        }

        /// <summary>
        /// Название страницы (тег title)
        /// </summary>
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

        public string Tag
        {
            get => tag;
            set
            {
                Set(ref tag, value);
                if (Information != null)
                {
                    foreach (var link in Information.AllLinks)
                    {
                        link.tag = value;
                    }
                }
            }
        }
        private string tag;

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
        [JsonIgnore]
        public bool IsImage => CheckIsImage();
        [JsonIgnore]
        public string[] AvailableTags => AllTags;


        public ArchiveLink()
        {
            name = DefaultName;
            category = "Общее";
            Tag = AllTags[1];
        }
        public string GetSourceLinkUniq()
        {
            string sourceLink = LinkSource;
            if (sourceLink.Contains(":80"))
            {
                sourceLink = sourceLink.Replace(":80", "");
            }
            if (sourceLink.EndsWith("/"))
            {
                sourceLink = sourceLink.Substring(0, sourceLink.Length - 1);
            }
            return sourceLink;
        }

        public bool CheckIsImage()
        {
            return MimeType.StartsWith("image");
        }

        private void SetBlackListed(bool isBlacklisted)
        {
            this.isBlacklisted=isBlacklisted;
            OnPropertyChanged(nameof(IsBlacklisted));
        }
    }
}
