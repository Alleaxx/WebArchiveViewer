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

using WebArchive.Data;
using WebArchive.Data.HtmlLoading;
using WebArchiveViewer.Services;
using WebArchiveViewer.ViewModels;

namespace WebArchiveViewer
{
    public class HtmlLoaderViewModel : NotifyObject
    {
        public MainWindowViewModel MainWindowModel { get; private set; }


        public HtmlLoaderViewModel(MainWindowViewModel mainModel)
        {
            MainWindowModel = mainModel;
        }
        public void SetSnapshot(SnapshotView snapshot)
        {
            SnapshotView = snapshot;
            LoadConfiguration = new LoadConfiguration(Snapshot.Links.Length);
            ProcessingConfiguration = new LinkProcessingConfiguration()
            {
                LoadingTitle = true,
                SavingHtml = true,
                FolderPath = FolderWrite.FullName
            };

            InitCollections();
            InitDates();
            PreSave();
        }
        private void InitCollections()
        {
            CurrentRequests = new ObservableCollection<LinkProcessing>();
            CurrentTasks = new List<Task>();
            LogError = new ObservableCollection<LinkProcessingEventArgs>();
            LinkProcessings = new ObservableCollection<LinkProcessingViewModel>();
            var remainingLinks = Snapshot.Links.Where(l => l.MimeType == "text/html" && string.IsNullOrEmpty(l.HtmlFilePath));
            LinksRemaining = new ObservableCollection<ArchiveLink>(remainingLinks);
            LinksLoadedCount = Snapshot.Links.Length - remainingLinks.Count();
        }
        private void InitDates()
        {
            PauseState = new PauseState(false);
        }
        protected override void InitCommands()
        {
            StartDownloadCommand = new RelayCommand(StartDownload, obj => !IsReady);
            SaveProgressCommand = new RelayCommand(SaveProgress, obj => IsReady);
            StopProgressCommand = new RelayCommand(StopDownloading, obj => IsReady);
        }
        private async void PreSave()
        {
            if (!string.IsNullOrEmpty(Snapshot.FilePath))
            {
                await Task.Run(() => SnapshotView.Save(SaveMode.AllDefaultPath));
            }
        }



        //Загрузка и запись файлов
        public ICommand StartDownloadCommand { get; private set; }
        public ICommand SaveProgressCommand { get; private set; }
        public ICommand StopProgressCommand { get; private set; }

        private void StartDownload(object obj)
        {
            TokenSource = new CancellationTokenSource();
            if (!FolderWrite.Exists)
            {
                FolderWrite.Create();
            }

            Stopped = false;
            IsStarted = true;

            TokenCancel = TokenSource.Token;
            Task.Run(Downloading, TokenCancel);
        }
        private async void SaveProgress(object obj)
        {
            await Task.Run(() => SnapshotView.Save(SaveMode.AllDefaultPath));
        }
        private async void StopDownloading(object obj)
        {
            if(TokenSource == null)
            {
                return;
            }
            PauseState.IsPaused = true;
            Stopped = true;
            await Task.Run(TokenSource.Cancel);
            SaveProgress(null);
            OnPropertyChanged(nameof(IsPauseEnabled));
            MainWindowModel.SetOperation(new ProcessStatus($"Загрузка HTML страниц завершена", 100, true, true));
        }


        //Откуда и куда
        public SnapshotView SnapshotView
        {
            get => snapshotView;
            set
            {
                Set(ref snapshotView, value);
                OnPropertyChanged(nameof(Snapshot));
                OnPropertyChanged(nameof(FolderWrite));
            }
        }
        private SnapshotView snapshotView;
        public Snapshot Snapshot => SnapshotView.CurrentSnapshot;
        public DirectoryInfo FolderWrite => SnapshotView.SavingFolderHtmlContent;

        //Процессы
        private ICollection<Task> CurrentTasks;
        private CancellationTokenSource TokenSource;
        private CancellationToken TokenCancel;


        //Настройки загрузки
        public LoadConfiguration LoadConfiguration
        {
            get => loadConfiguration;
            private set => Set(ref loadConfiguration, value);
        }
        private LoadConfiguration loadConfiguration;
        public LinkProcessingConfiguration ProcessingConfiguration
        {
            get => processingConfiguration;
            set => Set(ref processingConfiguration, value);
        }
        private LinkProcessingConfiguration processingConfiguration;




        //Прогресс загрузки
        public ObservableCollection<ArchiveLink> LinksRemaining
        {
            get => linksRemaining;
            private set => Set(ref linksRemaining, value);
        }
        private ObservableCollection<ArchiveLink> linksRemaining;

        public int LinksLoadedCount
        {
            get => linksLoadedCount;
            private set => Set(ref linksLoadedCount, value);
        }
        private int linksLoadedCount;

        public ArchiveLink LastLink
        {
            get => lastLink;
            private set => Set(ref lastLink, value);
        }
        private ArchiveLink lastLink;

        public IList<LinkProcessing> CurrentRequests { get; private set; }
        public IList<LinkProcessingEventArgs> LogError { get; private set; }
        public IList<LinkProcessingViewModel> LinkProcessings { get; private set; }


        //Скорость загрузки
        public double SpeedLinksPerMinute
        {
            get
            {
                if (LinksLoadedCount / PauseState.FromStart.TotalMinutes > 0)
                {
                    return LinksLoadedCount / PauseState.FromStart.TotalMinutes;
                }
                else
                {
                    return 10;
                }
            }
        }
        public TimeSpan TimeLeft => TimeSpan.FromMinutes(LinksRemaining.Count / SpeedLinksPerMinute);
        private void UpdateSpeed()
        {
            if (PauseState.IsPlayed && !Stopped)
            {
                OnPropertyChanged(nameof(PauseState.FromStart));
                OnPropertyChanged(nameof(SpeedLinksPerMinute));
                OnPropertyChanged(nameof(TimeLeft));
            }
        }



        //Обработка ссылки
        private void Downloading()
        {
            while (LinksRemaining.Any())
            {
                if (CurrentRequests.Count < 25 && PauseState.IsPlayed)
                {
                    var now = LinksRemaining.LastOrDefault();
                    if (now != null)
                    {
                        Task newTask = Task.Run(() => LinkLoadAsync(now));
                        CurrentTasks.Add(newTask);
                        LinksRemaining.Remove(now);
                    }
                }
                if (TokenCancel.IsCancellationRequested)
                {
                    break;
                }
                Thread.Sleep(250);
            }
        }
        private async Task LinkLoadAsync(ArchiveLink link)
        {
            var linkProcess = new LinkProcessing(link, ProcessingConfiguration);
            var linkProcessingModel = new LinkProcessingViewModel(linkProcess);
            linkProcess.SetClient(HttpService.GetHttpClient());

            DispatcherService.ExeInDispatcher(() => LinkProcessings.Add(linkProcessingModel));
            DispatcherService.ExeInDispatcher(() => CurrentRequests.Add(linkProcess));

            linkProcess.OnStatusChanged += LinkProcess_OnStatusChanged;
            await linkProcess.StartProcessing();
        }

        private void LinkProcess_OnStatusChanged(LinkProcessingEventArgs obj)
        {
            //if (!obj.State.IsSuccessfull)
            //{
            //    DispatcherService.ExeInDispatcher(() => LogError.Insert(0, obj));
            //}
            //DispatcherService.ExeInDispatcher(() => Log.Insert(0, obj));

            if (obj.State.IsEnded)
            {
                LinkProcessingFinished(obj);
            }
        }
        private async void LinkProcessingFinished(LinkProcessingEventArgs eventArgs)
        {
            var linkProcessing = eventArgs.Sender;
            bool success = eventArgs.State.IsSuccessfull;

            if (success)
            {
                LinksLoadedCount++;
                LastLink = linkProcessing.Link as ArchiveLink;

                //bool saveCounter = linkProcessing.Index % Options.SavingLatencyLinks == 0;
                //if (saveCounter)
                //{
                //    SaveProgress(null);
                //    UpdateSpeed();
                //}
            }
            else
            {
                //
            }
            int progress = (int)((double)linksLoadedCount / Snapshot.Links.Length * 100);
            DispatcherService.ExeInDispatcher(() => MainWindowModel.SetOperation(new ProcessStatus($"Загрузка HTML страниц: {linksLoadedCount} / {Snapshot.Links.Length}", progress, true, false)));
            DispatcherService.ExeInDispatcher(() => CurrentRequests.Remove(linkProcessing));
            linkProcessing.OnStatusChanged -= LinkProcess_OnStatusChanged;

            if (!LinksRemaining.Any())
            {
                StopDownloading(null);
            }
        }


        //Состояние паузы
        public bool IsStarted
        {
            get => isStarted;
            private set
            {
                Set(ref isStarted, value);
                PauseState.StartDateTime = DateTime.Now;
                OnPropertyChanged(nameof(IsPauseEnabled));
            }
        }
        private bool isStarted;
        public bool IsReady => IsStarted && !Stopped;
        public bool IsPauseEnabled => IsReady;
        public PauseState PauseState { get; private set; }
        private bool Stopped { get; set; }
    }
}
