using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WebArchiveViewer
{

    //Представление загрузки ссылок с архива
    class ArchiveLoadView : ObjNotify
    {
        //Инициализация
        public ArchiveLoadView()
        {
            UploadLinksCommand = new RelayCommand(UploadLinkExe, UploadLinksCan);
            SaveSnapFileCommand = new RelayCommand(SaveSnapFileExe, SaveSnapFileCan);
            OpenSnapshotCommand = new RelayCommand(OpenSnapshotFileExe, OpenSnapshotCan);
            Site = "https://ru-minecraft.ru/forum";

            Options = new LinksViewOptionsGet();
            Options.From = new DateTime(2011, 7, 27);
            Options.To = Options.From + new TimeSpan(24 * 365,0,0);
        }


        //Сайт и опции получения
        public string Site { get; set; }
        public LinksViewOptionsGet Options { get; set; }



        //Процесс получения ссылки
        private bool UploadLinksCan(object obj) => !string.IsNullOrEmpty(Site) && !UploadInProgress;
        public RelayCommand UploadLinksCommand { get; set; }
        private async void UploadLinkExe(object obj)
        {
            UploadInProgress = true;

            WebRequest request = WebRequest.Create(GetRequestString());
            UploadLinksProgressMax = 10;
            UploadLinksProgressNow = 0;

            UploadLinksStatus = "Ожидание ответа от сервера...";
            WebResponse responce = await request.GetResponseAsync();
            UploadLinksProgressNow = 2;

            UploadLinksStatus = "Загрузка данных с сервера...";
            using (Stream stream = responce.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    string responceText = await reader.ReadToEndAsync();

                    CurrentReceivedSnapshot = await Task.Run(() => GetSnapshotFromJson(responceText));
                    CurrentReceivedSnapshot.ViewOptions = Options;

                    UploadLinksStatus = $"Найдено {CurrentReceivedSnapshot.Links.Length} ссылок";
                }
            }
            UploadInProgress = false;
        }


        //Формирование строки запроса
        private string GetRequestString()
        {
            //https://github.com/internetarchive/wayback/tree/master/wayback-cdx-server
            //http://web.archive.org/cdx/search/cdx?url=http://ru-minecraft.ru*&output=json&from=2010&to=2011

            const string WebArhiveSearch = "http://web.archive.org/cdx/search/cdx?";
            string site = $"url={Site}*";
            string json = "&output=json";
            string dateFrom = Options.From.Year > 1990 ? $"&from={Options.From:yyyyMMddhhmmss}" : "";
            string dateTo = Options.To.Year > 1990 ? $"&to={Options.To:yyyyMMddhhmmss}" : "";
            string limit = Options.Limit > 0 ? $"&limit={Options.Limit}" : "";

            string filters = "";
            foreach (var option in Options.Codes)
            {
                if(int.TryParse(option.Value, out int codeNum))
                {
                    string inc = option.Enabled ? "" : "!";
                    filters += $"&filter={inc}statuscode:{codeNum}";
                }
            }
            foreach (var option in Options.Types)
            {
                string inc = option.Enabled ? "" : "!";
                filters += $"&filter={inc}mimetype:{option.Value}";
            }
            if (!string.IsNullOrEmpty(Options.Search))
            {
                string inc = Options.SearchIncluded ? "" : "!";
                filters += $"&filter={inc}urlkey:{Options.Search}";
            }
            return $"{WebArhiveSearch}{site}{json}{dateFrom}{dateTo}{limit}{filters}";            
        }
        //Создание снапшота из полученных данных
        private SiteSnapshot GetSnapshotFromJson(string json)
        {
            UploadLinksProgressNow = 6;
            UploadLinksStatus = "Обработка загруженных данных...";
            List<List<string>> jsonArr = JsonConvert.DeserializeObject<List<List<string>>>(json);
            List<ArchiveLink> allEntries = new List<ArchiveLink>();

            int counter = 0;
            foreach (var list in jsonArr)
            {
                if (jsonArr[0] == list)
                    continue;

                counter++;
                ArchiveLink newEntry = new ArchiveLink()
                {
                    TimeStamp = list[1],
                    LinkSource = list[2],
                    MimeType = list[3],
                    StatusCode = list[4],
                    Index = counter                           
                };
                newEntry.Date = new DateTime(
                    Convert.ToInt32(newEntry.TimeStamp.Substring(0, 4)),
                    Convert.ToInt32(newEntry.TimeStamp.Substring(4, 2)),
                    Convert.ToInt32(newEntry.TimeStamp.Substring(6, 2)),
                    Convert.ToInt32(newEntry.TimeStamp.Substring(8, 2)),
                    Convert.ToInt32(newEntry.TimeStamp.Substring(10, 2)),
                    Convert.ToInt32(newEntry.TimeStamp.Substring(12, 2)));
                    allEntries.Add(newEntry);
                if (newEntry.LinkSource.Contains("ru-minecraft.ru"))
                    newEntry.SetRumineCategory();
            }
                    
            UploadLinksProgressNow = 10;

            SiteSnapshot newSnap = new SiteSnapshot()
            {
                Date = DateTime.Now,
                SourceURI = Site,
                Links = allEntries.ToArray()
            };
            return newSnap;
        }


        //Статус запроса
        public string UploadLinksStatus
        {
            get => uploadLinksStatus;
            set
            {
                uploadLinksStatus = value;
                OnPropertyChanged();
            }
        }
        private string uploadLinksStatus;
        public int UploadLinksProgressNow
        {
            get => uploadLinksProgressNow;
            set
            {
                uploadLinksProgressNow = value;
                OnPropertyChanged();
            }
        }
        private int uploadLinksProgressNow = 0;
        public int UploadLinksProgressMax
        {
            get => uploadLinksProgressMax;
            set
            {
                uploadLinksProgressMax = value;
                OnPropertyChanged();
            }
        }
        private int uploadLinksProgressMax = 0;
        public bool UploadInProgress { get; set; }



        //Текущий полученный снапшот
        private SiteSnapshot CurrentReceivedSnapshot { get; set; }



        //Сохранение полученного снапшота в файл
        private bool SaveSnapFileCan(object obj) => CurrentReceivedSnapshot != null;
        private void SaveSnapFileExe(object obj)
        {
            Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.FileName = $"{CurrentReceivedSnapshot.Links.Length} - {CurrentReceivedSnapshot.SourceURI}";
            dialog.DefaultExt = ".json";
            dialog.InitialDirectory = Directory.GetCurrentDirectory();
            dialog.Filter = "JSON-файл (*.json) |*.json";
            if (dialog.ShowDialog() == true)
            {
                CurrentReceivedSnapshot.Save(dialog.FileName);
            }
        }
        public RelayCommand SaveSnapFileCommand { get; set; }


        //Задание полученного снапшота в качестве источника для просмотра
        private bool OpenSnapshotCan(object obj) => CurrentReceivedSnapshot != null;
        private void OpenSnapshotFileExe(object obj)
        {
            AppView.Ex.Archive.CurrentSnapshot = CurrentReceivedSnapshot;
            CurrentReceivedSnapshot = null;
            UploadLinksProgressMax = 0;
            UploadLinksProgressNow = 0;
            UploadLinksStatus = "Ожидание запроса";
        }
        public RelayCommand OpenSnapshotCommand { get; set; }
    }
}
