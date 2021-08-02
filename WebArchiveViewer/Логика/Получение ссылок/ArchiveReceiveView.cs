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

    //Загрузка ссылок с веб-архива
    class ArchiveReceiveView : NotifyObj
    {
        public ArchiveReceiveView()
        {
            FileDialog = new FileDialog();
            UploadLinksCommand = new RelayCommand(UploadLinkExe, IsUploadingAvailable);
            SaveSnapFileCommand = new RelayCommand(SaveSnapFile, IsSnapshotReceived);
            SetSnapshotCommand = new RelayCommand(SetSnapshotSource, IsSnapshotReceived);

            Site = "https://ru-minecraft.ru/forum";
            Options = new ViewOptionsGetLinks();
            Options.From = new DateTime(2011, 7, 27);
            Uploading = new ProcessProgress("Ожидание старта загрузки", 10);

            TimeSpan year = new TimeSpan(24 * 365, 0, 0);
            Options.To = Options.From + year;
        }


        //Сайт и настройки получения
        public string Site { get; set; }
        public ViewOptionsGetLinks Options { get; private set; }
        private IFileDialog FileDialog { get; set; }


        //Процесс получения ссылки
        private bool IsUploadingAvailable(object obj) => !string.IsNullOrEmpty(Site) && !Uploading.InProgress;
        public RelayCommand UploadLinksCommand { get; private set; }
        private async void UploadLinkExe(object obj)
        {
            Uploading.SetStatus("Создание запроса", 1);
            WebRequest request = WebRequest.Create(GetRequestString());

            Uploading.SetStatus("Ожидание ответа от сервера...", 2);
            WebResponse responce = await request.GetResponseAsync();

            Uploading.SetStatus("Загрузка данных с сервера...", 4);
            var responceText = await ReadResponce(responce);

            Uploading.SetStatus("Обработка загруженных данных...", 7);
            ReceivedSnapshot = await Task.Run(() => CreateSnapshotFromJson(responceText));
            ReceivedSnapshot.ViewOptions = Options;

            Uploading.SetStatus("Загрузка завершена...", Uploading.Maximum);
        }


        private async Task<string> ReadResponce(WebResponse responce)
        {
            using (Stream stream = responce.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    string responceText = await reader.ReadToEndAsync();
                    return responceText;
                }
            }
            throw new Exception("Не удалось нормально завершить поток");
        }
        public string RequestString => GetRequestString();
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
                if (int.TryParse(option.Value, out int codeNum))
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
        private ISnapshot CreateSnapshotFromJson(string json)
        {
            List<List<string>> jsonArr = JsonConvert.DeserializeObject<List<List<string>>>(json);
            List<ArchiveLink> allLinks = new List<ArchiveLink>();

            int counter = 0;
            foreach (var stringList in jsonArr)
            {
                bool first = counter == 0;
                if (first)
                {
                    counter++;
                    continue;
                }
                else
                {
                    counter++;
                    ArchiveLink newEntry = CreateLinkFromJsonString(counter, stringList);
                    allLinks.Add(newEntry);
                }
            }

            ISnapshot newSnap = new SiteSnapshot()
            {
                Date = DateTime.Now,
                SourceURI = Site,
                Links = allLinks.ToArray()
            };
            return newSnap;
        }
        private ArchiveLink CreateLinkFromJsonString(int counter, List<string> stringList)
        {
            ArchiveLink newEntry = new ArchiveLink()
            {
                TimeStamp = stringList[1],
                LinkSource = stringList[2],
                MimeType = stringList[3],
                StatusCode = stringList[4],
                Index = counter
            };
            newEntry.Date = new DateTime(
                Convert.ToInt32(newEntry.TimeStamp.Substring(0, 4)),
                Convert.ToInt32(newEntry.TimeStamp.Substring(4, 2)),
                Convert.ToInt32(newEntry.TimeStamp.Substring(6, 2)),
                Convert.ToInt32(newEntry.TimeStamp.Substring(8, 2)),
                Convert.ToInt32(newEntry.TimeStamp.Substring(10, 2)),
                Convert.ToInt32(newEntry.TimeStamp.Substring(12, 2)));
            return newEntry;
        }


        //Процесс загрузки
        public ProcessProgress Uploading { get; private set; }


        //Текущий полученный снапшот
        public ISnapshot ReceivedSnapshot
        {
            get => receivedSnapshot;
            private set
            {
                receivedSnapshot = value;
                OnPropertyChanged();
            }
        }
        private ISnapshot receivedSnapshot;
        private bool IsSnapshotReceived(object obj) => ReceivedSnapshot != null;


        //Сохранение полученного снапшота в файл
        private void SaveSnapFile(object obj)
        {
            string path = FileDialog.Save($"{ReceivedSnapshot.Links.Length} - {ReceivedSnapshot.SourceURI}");
            if (!string.IsNullOrEmpty(path))
            {
                ReceivedSnapshot.Save(path);
            }
        }
        public RelayCommand SaveSnapFileCommand { get; private set; }

        //Задание полученного снапшота в качестве источника для просмотра
        private void SetSnapshotSource(object obj)
        {
            AppView.Ex.Archive.CurrentSnapshot = ReceivedSnapshot;
            AppView.Ex.Archive.SetSnapshot(ReceivedSnapshot);
            ClearStatus();
        }
        public RelayCommand SetSnapshotCommand { get; private set; }


        //Сбросить состояние
        private void ClearStatus()
        {
            ReceivedSnapshot = null;
            Uploading.SetStatus("Ожидание старта новой загрузки", 0);
        }
    }

    class ProcessProgress : NotifyObj
    {
        public bool InProgress => Now > 0 && Now < Maximum;

        public int Maximum { get; private set; }
        public int Now { get; private set; }
        public string Status { get; private set; }

        public void SetStatus(string status, int now)
        {
            Status = status;
            Now = now;

            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(Now));
            OnPropertyChanged(nameof(InProgress));
        }

        public ProcessProgress(string status, int max)
        {
            Status = status;
            Now = 0;
            Maximum = max;
        }
    }
}
