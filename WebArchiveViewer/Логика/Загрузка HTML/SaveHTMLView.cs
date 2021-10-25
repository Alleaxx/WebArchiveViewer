using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
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

        public SaveHTMLView() : this(AppView.Ex.Archive.SnapshotView)
        {

        }
        public SaveHTMLView(SnapshotView snapshot) : base()
        {
            if(snapshot == null || snapshot.Current == null)
            {
                throw new ArgumentNullException();
            }

            SnapshotView = snapshot;
            Options = new LoadOptions(Snapshot.Links.Length);
            Dialog = new FileDialog();

            ProcessOptions = new LoadHtmlOptions(FolderWrite.FullName)
            {
                SavingHtml = true,
                LoadingName = true
            };

            InitCollections();
            InitDates();
            PreSave();

            void InitCollections()
            {
                ActiveRequests = new ObservableCollection<HTMLoader>();
                SuccessfullRequests = new ObservableCollection<HTMLoader>();
                ErrorRequests = new ObservableCollection<HTMLoader>();
                var remainingLinks = Snapshot.Links.Where(l => string.IsNullOrEmpty(l.HtmlFilePath));
                LinksRemaining = new ObservableCollection<ArchiveLink>(remainingLinks);
                LinksLoadedCount = Snapshot.Links.Length - remainingLinks.Count();
            }
            void InitDates()
            {
                PauseState = new PauseState(false);
                StartDateTime = DateTime.Now;
            }
            async void PreSave()
            {
                if (!string.IsNullOrEmpty(Snapshot.FilePath))
                {
                    snapshot.Save(SaveMode.AllDefaultPath);
                }
            }
        }
        protected override void InitCommands()
        {
            StartDownloadCommand = new RelayCommand(StartDownload, obj => !IsReady);
            SaveProgressCommand = new RelayCommand(SaveProgress, obj => IsReady);
            StopProgressCommand = new RelayCommand(StopProgress, obj => IsReady);
        }

        //Загрузка и запись файлов
        public ICommand StartDownloadCommand { get; private set; }
        public ICommand SaveProgressCommand { get; private set; }
        public ICommand StopProgressCommand { get; private set; }


        //Откуда и куда
        public Snapshot Snapshot => SnapshotView.Current;
        public SnapshotView SnapshotView { get; private set; }
        public DirectoryInfo FolderWrite => SnapshotView.FolderHtmlSavePath;
        private IFileDialog Dialog { get; set; }
        private HttpClient Client { get; set; }




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
        private bool isStarted;
        public DateTime StartDateTime { get; private set; }
        private async void StartDownload(object obj)
        {
            Client = new HttpClient();
            if (!FolderWrite.Exists)
            {
                FolderWrite.Create();
            }

            Task loading = Task.Run(Downloading);
            IsStarted = true;
            Stopped = false;
            OnPropertyChanged(nameof(IsPauseEnabled));
            await loading;
        }


        public bool IsReady => IsStarted && !Stopped;

        public TimeSpan FromStart => DateTime.Now - StartDateTime;

        //Прогресс загрузки
        public ObservableCollection<ArchiveLink> LinksRemaining { get; private set; } = new ObservableCollection<ArchiveLink>();

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


        public double SpeedLinksPerMinute
        {
            get
            {
                if(LinksLoadedCount / FromStart.TotalMinutes > 0)
                {
                    return LinksLoadedCount / FromStart.TotalMinutes;
                }
                else
                {
                    return 10;
                }
            }
        }
        public TimeSpan TimeLeft => TimeSpan.FromMinutes(LinksRemaining.Count / SpeedLinksPerMinute);


        //Настройки загрузки
        public LoadOptions Options { get; private set; }
        public LoadHtmlOptions ProcessOptions { get; private set; }

        //Состояние паузы
        public bool IsPauseEnabled => IsReady;
        public PauseState PauseState { get; private set; }


        //Процесс загрузки
        public ObservableCollection<HTMLoader> ActiveRequests { get; private set; }
        public ObservableCollection<HTMLoader> SuccessfullRequests { get; private set; }
        public ObservableCollection<HTMLoader> ErrorRequests { get; private set; }
        private void Downloading()
        {
            var links = LinksRemaining.ToArray();
            for (int i = 0; i < links.Length; )
            {
                Thread.Sleep(Options.LatencyMs);
                bool noRequestLimit = ActiveRequests.Count <= Options.MaxRequestsSimultaneosly;
                if (PauseState.IsPlayed && noRequestLimit)
                {
                    ArchiveLink link = links[i];

                    Task.Run(() => ProcessLinkAsync(link));

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

            void UpdateSpeed()
            {
                if (PauseState.IsPlayed && !Stopped)
                {
                    OnPropertyChanged(nameof(FromStart));
                    OnPropertyChanged(nameof(SpeedLinksPerMinute));
                    OnPropertyChanged(nameof(TimeLeft));
                }
            }
        }
        private async void ProcessLinkAsync(ArchiveLink link)
        {
            var loader = new HttpClientHTMLoader(Client, link, ProcessOptions);
            ExeInDispatcher(() => ActiveRequests.Add(loader));;

            bool loadSuccessfull = await loader.LoadHtmlAsync();

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



        //Сохранение
        private async void SaveProgress(object obj)
        {
            SnapshotView.Save(SaveMode.AllDefaultPath);
        }


        private bool Stopped { get; set; }
        private void StopProgress(object obj)
        {
            if(Client != null)
            {
                Client.Dispose();
            }
            PauseState.IsPaused = true;
            Stopped = true;
            SaveProgress(null);
            OnPropertyChanged(nameof(IsPauseEnabled));
        }
    }




}
