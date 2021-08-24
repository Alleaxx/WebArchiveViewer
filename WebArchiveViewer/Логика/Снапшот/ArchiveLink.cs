using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WebArchiveViewer
{
    public interface IArchLink
    {
        int Index { get; }
        string Name { get; set; }
        string Link { get; }
        string LinkSource { get; }
        string Category { get; set; }
        DateTime Date { get; }
        string StatusCode { get; }
        string MimeType { get; }
        string TimeStamp { get; }

        string HtmlFilePath { get; set; }


        RelayCommand LoadNameCommand { get; }
    }

    public class ArchiveLink : NotifyObj, IArchLink
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
        protected override void InitCommands()
        {
            base.InitCommands();
            LoadNameCommand = new RelayCommand(LoadNameAsync, IsLoadNameAvailable);
        }

        //Загрузка имени страницы
        private string[] ForbiddenCodes = new string[] { "404", "502", "302" };
        private bool IsLoadNameAvailable(object obj) => !ForbiddenCodes.Contains(StatusCode);
        [JsonIgnore]
        public RelayCommand LoadNameCommand { get; private set; }
        private async void LoadNameAsync(object obj)
        {
            try
            {
                WebClient client = new WebClient();
                string html = await client.DownloadStringTaskAsync(new Uri(Link));
                Name = await Task.Run(() => GetTitleFromHtml(html));
            }
            catch (WebException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static string GetTitleFromHtml(string html)
        {
            string name = Regex.Match(html, @"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\</title\>",
                RegexOptions.IgnoreCase).Groups["Title"].Value;
            return name;
        }
    }

}
