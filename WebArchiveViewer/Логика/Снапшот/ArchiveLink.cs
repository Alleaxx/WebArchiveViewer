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
        string Name { get; }
        string LinkSource { get; }
        string Category { get; set; }
        DateTime Date { get; }
        string StatusCode { get; }
        string ActualState { get; }
        string MimeType { get; }
        string TimeStamp { get; }


        RelayCommand LoadNameCommand { get; }
    }

    //Ссылка в веб архиве на сайт
    [Serializable]
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

        public string ActualState
        {
            get => actualState;
            set
            {
                actualState = value;
                OnPropertyChanged();
            }
        }
        private string actualState;

        public ArchiveLink()
        {
            LoadNameCommand = new RelayCommand(LoadName, IsLoadNameAvailable);
        }


        //Загрузка имени страницы
        private WebClient Client { get; set; }
        private string[] ForbiddenCodes = new string[] { "404", "502", "302" };
        private bool IsLoadNameAvailable(object obj) => Client == null && !ForbiddenCodes.Contains(StatusCode);
        [JsonIgnore]
        public RelayCommand LoadNameCommand { get; private set; }
        private void LoadName(object obj)
        {
            Client = new WebClient();
            Client.DownloadStringCompleted += Client_LoadCompleted;
            Client.DownloadStringAsync(new Uri(Link));
        }

        private void Client_LoadCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            Client.DownloadStringCompleted -= Client_LoadCompleted;
            if(e.Error == null)
            {
                string source = e.Result;
                Name = Regex.Match(source, @"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\</title\>", RegexOptions.IgnoreCase).Groups["Title"].Value;
            }
            Client = null;
        }
    }

}
