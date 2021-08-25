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
        public override string ToString() => "Загрузка HTML-разметки";

        public SaveHTMLView() : this(AppView.Ex.Archive.CurrentSnapshot)
        {

        }
        public SaveHTMLView(SiteSnapshot snapshot) : base()
        {
            Snapshot = snapshot;
            var remainingLinks = snapshot.Links.Where(l => string.IsNullOrEmpty(l.HtmlFilePath));
            LinksRemaining = new ObservableCollection<IArchLink>(remainingLinks);
            LinksLoadedCount = snapshot.Links.Length - remainingLinks.Count();
            Options = new LoadOptions(snapshot.Links.Length);

            StartDateTime = DateTime.Now;
            Snapshot.LastSaveDate = DateTime.Now;

            if (string.IsNullOrEmpty(Snapshot.FilePath))
                Snapshot.Save(@"D:\Users\Allexx\Documents\Румайн\Проекты\История Румине\ProjectArch 3.0\ПоследниеСсылки.json");

        }
        protected override void InitCommands()
        {
            StartDownloadCommand = new RelayCommand(StartDownload, obj => !IsStarted);
            SaveProgressCommand = new RelayCommand(SaveProgress, obj => IsStarted && !Stopped);
            StopProgressCommand = new RelayCommand(StopProgress, obj => IsStarted && !Stopped);
        }


        //Откуда и куда
        public SiteSnapshot Snapshot { get; private set; }
        public DirectoryInfo FolderWrite => new DirectoryInfo(Snapshot.FolderHtmlSavePath);




        //Загрузка и запись файлов
        public ICommand StartDownloadCommand { get; private set; }
        public bool IsStarted
        {
            get => isStarted;
            private set
            {
                isStarted = value;
                OnPropertyChanged();
                StartDateTime = DateTime.Now;
                OnPropertyChanged(nameof(StartDateTime));
            }
        }
        private bool isStarted = false;
        public DateTime StartDateTime { get; private set; }
        private async void StartDownload(object obj)
        {
            if (!FolderWrite.Exists)
            {
                FolderWrite.Create();
            }

            Task loading = Task.Run(Downloading);
            IsStarted = true;
            OnPropertyChanged(nameof(IsPauseEnabled));
            await loading;
        }


        public TimeSpan FromStart => DateTime.Now - StartDateTime;

        //Прогресс загрузки
        public ObservableCollection<IArchLink> LinksRemaining { get; private set; } = new ObservableCollection<IArchLink>();

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

        public IArchLink LastLink
        {
            get => lastLink;
            private set
            {
                lastLink = value;
                OnPropertyChanged();
            }
        }
        private IArchLink lastLink;


        public double SpeedLinksPerMinute => LinksLoadedCount / FromStart.TotalMinutes > 0 ? LinksLoadedCount / FromStart.TotalMinutes : 10;
        public TimeSpan TimeLeft => TimeSpan.FromMinutes(LinksRemaining.Count / SpeedLinksPerMinute);


        //Настройки загрузки
        public LoadOptions Options { get; private set; }

        //Состояние паузы
        public bool IsPauseEnabled => IsStarted && !Stopped;
        public PauseState PauseState { get; private set; } = new PauseState(true);



        public ObservableCollection<HTMLoader> ActiveRequests { get; private set; } = new ObservableCollection<HTMLoader>();
        public ObservableCollection<HTMLoader> SuccessfullRequests { get; private set; } = new ObservableCollection<HTMLoader>();
        public ObservableCollection<HTMLoader> ErrorRequests { get; private set; } = new ObservableCollection<HTMLoader>();
        private void Downloading()
        {
            var links = LinksRemaining.ToArray();
            for (int i = 0; i < links.Length; )
            {
                Thread.Sleep(Options.LatencyMs);
                bool noRequestLimit = ActiveRequests.Count <= Options.MaxRequestsSimultaneosly;
                if (PauseState.IsPlayed && noRequestLimit)
                {
                    IArchLink link = links[i];

                    Task.Run(() => LoadLinkAsync(link));

                    i++;

                    bool saveCounter = i % Options.SavingLatencyLinks == 0;
                    if (saveCounter)
                    {
                        SaveProgress(null);
                    }
                }
                UpdateSpeed();
            }
            StopProgress(null);
        }
        private async void LoadLinkAsync(IArchLink link)
        {
            var options = new LoadHtmlOptions(FolderWrite.FullName) { SavingHtml = true, LoadingName = true };
            var loader = new ClientHTMLoader(link, options);
            ExeInDispatcher(() => ActiveRequests.Add(loader));;

            bool loadSuccessfull = await loader.LoadHtml();
            LoadState state = loader.State;


            ExeInDispatcher(() => ActiveRequests.Remove(loader));
            if (loadSuccessfull)
            {
                LinksLoadedCount++;
                LinksRemaining.Remove(link);
                LastLink = link;

                ExeInDispatcher(() => SuccessfullRequests.Add(loader));
            }
            else
            {
                ExeInDispatcher(() => ErrorRequests.Add(loader));
            }


            void ExeInDispatcher(Action action)
            {
                Application.Current.Dispatcher.BeginInvoke(action);
            }
        }
        private void UpdateSpeed()
        {
            if (PauseState.IsPlayed && !Stopped)
            {
                OnPropertyChanged(nameof(FromStart));
                OnPropertyChanged(nameof(SpeedLinksPerMinute));
                OnPropertyChanged(nameof(TimeLeft));
            }
        }

        public ICommand SaveProgressCommand { get; private set; }
        private void SaveProgress(object obj)
        {
            Snapshot.Save();
        }

        private bool Stopped { get; set; }
        public ICommand StopProgressCommand { get; private set; }
        private void StopProgress(object obj)
        {
            PauseState.IsPaused = true;
            Stopped = true;
            SaveProgress(null);
            OnPropertyChanged(nameof(IsPauseEnabled));
        }
    }

    //Настройки загрузки
    public class LoadOptions : NotifyObj
    {
        public override string ToString() => $"Настройки загрузки: {LatencyMs}мс, {MaxRequestsSimultaneosly} макс, {SavingLatencyLinks} сохр";

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
        public int MaxRequestsSimultaneosly { get; set; } = 5;
        public int SavingLatencyLinks { get; set; } = 10;
        private int GetSavingLatency(int count)
        {
            int latency = count / 10;
            int min = 10;
            if (latency < min)
                latency = min;
            return latency;
        }

        public LoadOptions() : this(500) { }
        public LoadOptions(int amount)
        {
            SavingLatencyLinks = GetSavingLatency(amount);
        }

    }


    public class PauseState : NotifyObj
    {
        public override string ToString() => $"Пауза: {(IsPaused ? "активна" : "выключена")}";

        public bool IsPaused
        {
            get => isPaused;
            set
            {
                isPaused = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsPlayed));
            }
        }
        private bool isPaused;
        public bool IsPlayed
        {
            get => !isPaused;
            set
            {
                IsPaused = !value;
                OnPropertyChanged();
            }
        }

        public PauseState(bool paused)
        {
            IsPaused = paused;
        }
    }
}
