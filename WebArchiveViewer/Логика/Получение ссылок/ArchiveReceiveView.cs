using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WebArchiveViewer
{

    //Загрузка ссылок с веб-архива
    public class ArchiveReceiveView : NotifyObj
    {
        public ArchiveReceiveView()
        {
            FileDialog = new FileDialog();
            RequestArchiveCreator = new ArchiveRequestCreator();

            Uploading = new ProcessProgress("Ожидание старта загрузки", 10);
        }
        public ArchiveReceiveView(ISnapshot snapshot) : this()
        {
            if(snapshot != null)
            {
                RequestArchiveCreator.Site.Value = snapshot.SourceURI;
                RequestArchiveCreator.Dates.Range.From = snapshot.ViewOptions.DateRange.From;
                RequestArchiveCreator.Dates.Range.To = snapshot.ViewOptions.DateRange.To;
                RequestArchiveCreator.Limit.Amount = snapshot.Links.Length;
            }
        }
        protected override void InitCommands()
        {
            base.InitCommands();
            UploadLinksCommand = new RelayCommand(UploadLinksExe, IsUploadingAvailable);
            SaveSnapFileCommand = new RelayCommand(SaveSnapFile, IsNotEmptySnapshotReceived);
            SetSnapshotCommand = new RelayCommand(SetSnapshotSource, IsNotEmptySnapshotReceived);
            CopyRequestCommand = new RelayCommand(CopyRequest);
        }


        //Сайт и настройки получения
        public IRequestCreator RequestCreator => RequestDefaultCreator ?? RequestArchiveCreator;
        public IRequestCreator RequestDefaultCreator { get; set; }
        public IArhiveRequest RequestArchiveCreator { get; private set; }
        private IFileDialog FileDialog { get; set; }


        //Процесс получения ссылки
        private bool IsUploadingAvailable(object obj) => (!string.IsNullOrEmpty(RequestCreator.GetRequest())) && !Uploading.InProgress;
        public ICommand UploadLinksCommand { get; private set; }
        private async void UploadLinksExe(object obj)
        {
            await UploadLinks();
        }
        public async Task<ISnapshot> UploadLinks()
        {
            Uploading.SetStatus("Создание запроса", 1);
            WebRequest request = WebRequest.Create(RequestString);

            Uploading.SetStatus("Ожидание ответа от сервера...", 2);
            WebResponse responce = await request.GetResponseAsync();

            Uploading.SetStatus("Загрузка данных с сервера...", 4);
            var responceText = await ReadResponce(responce);

            Uploading.SetStatus("Обработка загруженных данных...", 7);
            ReceivedSnapshot = await Task.Run(() => CreateSnapshotFromJson(responceText));

            Uploading.SetStatus("Загрузка завершена...", Uploading.Maximum);
            return ReceivedSnapshot;
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
        }
        public string RequestString => RequestCreator.GetRequest();


        public ICommand CopyRequestCommand { get; private set; }
        private void CopyRequest(object obj)
        {
            System.Windows.Clipboard.SetText(RequestString);
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
            string source = RequestArchiveCreator.Site.Value;
            ISnapshot newSnap = new SiteSnapshot(RequestString, source, allLinks);
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
        private bool IsNotEmptySnapshotReceived(object obj) => ReceivedSnapshot != null && ReceivedSnapshot.Links.Length > 0;


        //Сохранение полученного снапшота в файл
        private void SaveSnapFile(object obj)
        {
            var file = FileDialog.Save($"{ReceivedSnapshot.Links.Length} - {ReceivedSnapshot.SourceURI}");
            if (file != null && file.Exists)
            {
                string path = file.FullName;
                ReceivedSnapshot.Save(path);
                AppView.Ex.Archive.CurrentSnapshot = ReceivedSnapshot;
                ClearStatus();
            }
        }
        public ICommand SaveSnapFileCommand { get; private set; }

        //Задание полученного снапшота в качестве источника для просмотра
        private void SetSnapshotSource(object obj)
        {
            AppView.Ex.Archive.CurrentSnapshot = ReceivedSnapshot;
            ClearStatus();
        }
        public ICommand SetSnapshotCommand { get; private set; }


        //Сбросить состояние
        private void ClearStatus()
        {
            ReceivedSnapshot = null;
            Uploading.SetStatus("Ожидание старта новой загрузки", 0);
        }
    }

    public class ProcessProgress : NotifyObj
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
