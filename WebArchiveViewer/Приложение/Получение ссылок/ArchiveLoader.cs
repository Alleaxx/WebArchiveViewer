using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using WebArchive.Data;
namespace WebArchiveViewer
{

    public class ArchiveSnapLoader : SnapLoader
    {
        public override string ToString()
        {
            return $"Архивный загрузчик снапшота - {RequestArchiveCreator.Site.Value}";
        }

        public ArchiveSnapLoader() : this(null)
        {

        }
        public ArchiveSnapLoader(SnapshotReceiver receiver) : base(receiver)
        {
            RequestArchiveCreator = new ArchiveRequestCreator();

            Uploading = new ProcessProgress("Ожидание старта загрузки", 10);
        }

        protected override void LoadSnapshot()
        {
            LoadWindow window = new LoadWindow(this);
            window.Show();
        }


        //Сайт и настройки получения
        public IRequestCreator RequestCreator
        {
            get
            {
                if(RequestDefaultCreator == null)
                {
                    return RequestArchiveCreator;
                }
                else
                {
                    return RequestDefaultCreator;
                }
            }
        }
        public DefaultRequestCreator RequestDefaultCreator { get; set; }
        public ArchiveRequestCreator RequestArchiveCreator { get; private set; }



        protected override void InitCommands()
        {
            base.InitCommands();
            UploadLinksCommand = new RelayCommand(async obj => await UploadLinks(obj), IsUploadingAvailable);
            SetSnapshotCommand = new RelayCommand(SetSnapshotSource, IsNotEmptySnapshotReceived);
            CopyRequestCommand = new RelayCommand(CopyRequest);
        }
        public ICommand UploadLinksCommand { get; private set; }
        public ICommand SetSnapshotCommand { get; private set; }
        public ICommand CopyRequestCommand { get; private set; }


        //Процесс получения ссылки
        private bool IsUploadingAvailable(object obj) => (!string.IsNullOrEmpty(RequestCreator.GetRequest())) && !Uploading.InProgress;
        public ProcessProgress Uploading { get; private set; }


        public async Task<Snapshot> UploadLinks(object obj = null)
        {
            Uploading.SetStatus("Загрузка данных с сервера...", 3);
            using(HttpClient client = new HttpClient())
            {
                try
                {
                    string responceText = await client.GetStringAsync(RequestString);

                    Uploading.SetStatus("Обработка загруженных данных...", 7);
                    Snapshot = await Task.Run(() => CreateSnapshotFromJson(responceText));

                    Uploading.SetStatus("Загрузка завершена...", Uploading.Maximum);
                    return Snapshot;
                }
                catch (WebException ex)
                {
                    Uploading.SetStatus($"Ошибка соединения: {ex.Message}", Uploading.Maximum);
                    return null;
                }
                catch (Exception ex)
                {
                    Uploading.SetStatus($"Неопределенная ошибка: {ex.Message}", Uploading.Maximum);
                    return null;
                }
            }
        }



        //Скопировать строку запроса
        public string RequestString => RequestCreator.GetRequest();
        private void CopyRequest(object obj)
        {
            System.Windows.Clipboard.SetText(RequestString);
        }


        //Создание снапшота из полученных данных
        private Snapshot CreateSnapshotFromJson(string json)
        {
            string source = RequestArchiveCreator.Site.Value;
            if (string.IsNullOrEmpty(json))
            {
                return new Snapshot(RequestString, source, Array.Empty<ArchiveLink>());
            }
            List<List<string>> jsonArr = Newtonsoft.Json.JsonConvert.DeserializeObject<List<List<string>>>(json);

            ArchiveLink[] allLinks = jsonArr.Skip(1).AsParallel().Select((arr, index) =>
            {
                return CreateLinkFromJsonString(index, arr);
            }).ToArray();

            Snapshot newSnap = new Snapshot(RequestString, source, allLinks);
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


        //Задание полученного снапшота в качестве источника для просмотра
        private bool IsNotEmptySnapshotReceived(object obj) => Snapshot != null && Snapshot.Links.Length > 0;
        private void SetSnapshotSource(object obj)
        {
            SendSnapshot();
            ClearStatus();
        }


        //Сбросить состояние
        private void ClearStatus()
        {
            Snapshot = null;
            Uploading.SetStatus("Ожидание старта новой загрузки", 0);
        }
    }
}
