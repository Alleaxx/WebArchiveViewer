using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace WebArchiveViewer
{
    public class SaveHTMLView : NotifyObj
    {
        public SaveHTMLView() : this(AppView.Ex.Archive.CurrentSnapshot)
        {

        }
        public SaveHTMLView(ISnapshot snapshot) : base()
        {
            Snapshot = snapshot as SiteSnapshot;
            var remainingLinks = snapshot.Links.Where(l => string.IsNullOrEmpty(l.HtmlFilePath));
            LinksRemainingList = new ObservableCollection<ArchiveLink>(remainingLinks);
            LinksLoadedCount = snapshot.Links.Length - linksRemainingList.Count;
            SavingLatency = GetSavingLatency(snapshot.Links.Length);

            StartDateTime = DateTime.Now;
            Snapshot.LastSaveDate = DateTime.Now;

            if (string.IsNullOrEmpty(Snapshot.FilePath))
                Snapshot.Save(@"D:\Users\Allexx\Documents\Румайн\Проекты\История Румине\ProjectArch 3.0\ПоследниеСсылки.json");

            ActiveLinkRequests = new ObservableCollection<LinkState>();
            ErrorLinkRequests = new ObservableCollection<LinkState>();
            FinishedLinkRequests = new ObservableCollection<LinkState>();
        }
        protected override void InitCommands()
        {
            StartDownloadCommand = new RelayCommand(StartDownload, obj => !IsStarted);
            SaveProgressCommand = new RelayCommand(SaveProgress, obj => IsStarted && !Stopped);
            StopProgressCommand = new RelayCommand(StopProgress, obj => IsStarted && !Stopped);
        }
        private int GetSavingLatency(int count)
        {
            int latency = count / 10;
            int min = 10;
            if (latency < min)
                latency = min;
            return latency;
        }


        public SiteSnapshot Snapshot { get; private set; }
        public DirectoryInfo FolderWrite => new DirectoryInfo(Snapshot.FolderHtmlSavePath);




        //Загрузка и запись файлов
        public bool IsStarted
        {
            get => isStarted;
            private set
            {
                isStarted = value;
                OnPropertyChanged();
            }
        }
        private bool isStarted = false;
        public DateTime StartDateTime { get; private set; }
        public ICommand StartDownloadCommand { get; private set; }
        private async void StartDownload(object obj)
        {
            if (!FolderWrite.Exists)
            {
                FolderWrite.Create();
            }

            Task loading = new Task(Downloading);
            loading.Start();
            IsStarted = true;
            OnPropertyChanged(nameof(IsPauseEnabled));
            StartDateTime = DateTime.Now;
            OnPropertyChanged(nameof(StartDateTime));
            await loading;
        }


        public TimeSpan FromStart => DateTime.Now - StartDateTime;

        //Прогресс загрузки
        public ObservableCollection<ArchiveLink> LinksRemainingList
        {
            get => linksRemainingList;
            private set
            {
                linksRemainingList = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<ArchiveLink> linksRemainingList;

        public int LinksLoadedCount
        {
            get => linksLoadedCount;
            private set
            {
                linksLoadedCount = value;
                OnPropertyChanged();
            }
        }
        private int linksLoadedCount;

        public ArchiveLink LastLink
        {
            get => lastLink;
            private set
            {
                lastLink = value;
                OnPropertyChanged();
            }
        }
        private ArchiveLink lastLink;


        public double SpeedLinksPerMinute => LinksLoadedCount / FromStart.TotalMinutes > 0 ? LinksLoadedCount / FromStart.TotalMinutes : 10;
        public TimeSpan TimeLeft => TimeSpan.FromMinutes(LinksRemainingList.Count / SpeedLinksPerMinute); 


        //Настройки загрузки
        public int LatencyMs
        {
            get => latencyMs;
            set
            {
                latencyMs = value;
                OnPropertyChanged();
            }
        }
        private int latencyMs = 1500;
        private int SavingLatency { get; set; } = 10;

        //Состояние паузы
        public bool IsPauseEnabled => IsStarted && !Stopped;
        public bool IsPaused
        {
            get => PauseState.IsPaused;
            set
            {
                if (!Stopped)
                {
                    PauseState = new PauseState(value);
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(PauseState));
                    OnPropertyChanged(nameof(IsPausedInverted));
                }
            }
        }
        public bool IsPausedInverted
        {
            get => PauseState.IsPausedInverted;
            set
            {
                IsPaused = !value;
            }
        }
        public PauseState PauseState { get; private set; } = new PauseState(false);


        //Загрузка кода страниц:
        //каждые n миллисекунд задержки посылает запрос на код сайта
        //запрос может выполняться случайное число времени
        //активные запросы должны быть видны в интерфейсе
        //должна быть возможность поставить на паузу процесс
        //Алгоритм:
        //- посылаем запрос
        //- ждем n секунд
        //- посылаем следующий запрос
        //- ... повторяем

        
        //Необходимо видеть
        //- сокращенный адрес ссылки (с возможностью перехода)
        //- текущий статус ссылки
        //- её позиция
        public ObservableCollection<LinkState> ActiveLinkRequests { get; private set; }
        public ObservableCollection<LinkState> FinishedLinkRequests { get; private set; }
        public ObservableCollection<LinkState> ErrorLinkRequests { get; private set; }
        private void Downloading()
        {
            ArchiveLink[] links = LinksRemainingList.ToArray();
            for (int i = 0; i < links.Length; )
            {
                Thread.Sleep(LatencyMs);
                if (!IsPaused && ActiveLinkRequests.Count <= 5)
                {
                    ArchiveLink link = links[i];
                    Task currentLinkTask = new Task(() => StartRequest(link));
                    currentLinkTask.Start();
                    i++;
                    if (i % SavingLatency == 0)
                        SaveProgress(null);
                }
                UpdateSpeed();
            }
            StopProgress(null);
        }
        private async void StartRequest(ArchiveLink link)
        {
            //Условное выполнение задачи
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(link.Link);
            LinkState state = new LinkState(link.Index, link);
            try
            {
                await Application.Current.Dispatcher.BeginInvoke(new Action(() => ActiveLinkRequests.Add(state)));
                HttpWebResponse response = (HttpWebResponse) await request.GetResponseAsync();
                using (Stream stream = response.GetResponseStream())
                {
                    state.Status = "Соединение установлено...";


                    // запись в файл
                    using (StreamReader reader = new StreamReader(stream, Encoding.GetEncoding(1251)))
                    {
                        string name = GetSaveName(link);

                        string filePath = $"{FolderWrite.FullName}\\{name}.html";
                        using (FileStream fstream = new FileStream(filePath, FileMode.OpenOrCreate))
                        {
                            byte[] array = Encoding.Default.GetBytes(reader.ReadToEnd());
                            await fstream.WriteAsync(array, 0, array.Length);

                            LinksLoadedCount++;
                            LinksRemainingList.Remove(link);
                            state.Status = $"Записан в {name}";
                            link.HtmlFilePath = filePath;
                            LastLink = link;
                            await Application.Current.Dispatcher.BeginInvoke(new Action(() => FinishedLinkRequests.Add(state)));
                            await Application.Current.Dispatcher.BeginInvoke(new Action(() => ActiveLinkRequests.Remove(state)));
                        }
                    }
                }

            }
            catch (WebException ex)
            {
                link.HtmlFilePath = null;
                state.Status = ex.Message;
                await Application.Current.Dispatcher.BeginInvoke(new Action(() => ActiveLinkRequests.Remove(state)));
                await Application.Current.Dispatcher.BeginInvoke(new Action(() => ErrorLinkRequests.Add(state)));
            }
            catch (Exception ex)
            {

            }
        }
        private string GetSaveName(IArchLink link)
        {
            string name = link.Name == ArchiveLink.DefaultName ? $"{link.TimeStamp} - {link.Index}" : $"{link.TimeStamp} - {link.Index} - {link.Name}";
            foreach (var invalidChar in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(invalidChar, '_');
            }
            return name;
        }


        public ICommand SaveProgressCommand { get; private set; }
        private void SaveProgress(object obj)
        {
            Snapshot.Save();
        }
        private void UpdateSpeed()
        {
            if(!IsPaused && !Stopped)
            {
                OnPropertyChanged(nameof(FromStart));
                OnPropertyChanged(nameof(SpeedLinksPerMinute));
                OnPropertyChanged(nameof(TimeLeft));
            }
        }

        private bool Stopped { get; set; }
        public ICommand StopProgressCommand { get; private set; }
        private void StopProgress(object obj)
        {
            IsPaused = true;
            Stopped = true;
            SaveProgress(null);
            OnPropertyChanged(nameof(IsPauseEnabled));
        }
    }
    public class PauseState
    {
        public bool IsPaused { get; set; }
        public bool IsPausedInverted => !IsPaused;
        public string Color { get; set; }

        public PauseState(bool paused)
        {
            IsPaused = paused;
            if (paused)
            {
                Color = "LightGray";
            }
            else
            {
                Color = "Green";
            }
        }
    }
    public class LinkState : NotifyObj
    {
        public int Index { get; set; }
        public ArchiveLink Link { get; set; }
        public string Status
        {
            get => status;
            set
            {
                status = value;
                OnPropertyChanged();
            }
        }
        private string status;

        public LinkState(int index, ArchiveLink link)
        {
            Index = index;
            Link = link;
            Status = "Ожидание ответа...";
        }
    }
}
