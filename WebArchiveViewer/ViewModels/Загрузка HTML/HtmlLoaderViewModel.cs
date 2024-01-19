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

namespace WebArchiveViewer.ViewModels
{
    public class HtmlLoaderViewModel : NotifyObject
    {
        public MainWindowViewModel MainWindowModel { get; private set; }

        public HtmlLoaderViewModel(MainWindowViewModel mainModel)
        {
            MainWindowModel = mainModel;
            SetSnapshot(mainModel.SnapshotView);
        }
        public void SetSnapshot(SnapshotView snapshot)
        {
            SnapshotView = snapshot;
            LoadConfiguration = new HtmlLoadingConfiguration(Snapshot.Links.Any() ? Snapshot.Links.Length : 0);
            LoadConfiguration.SetLinksSource(snapshot);
            ProcessingConfiguration = new LinkProcessingConfiguration()
            {
                LoadingTitle = true,
                SavingHtml = true,
                FolderPath = SavingHtmlFolder.FullName
            };

            InitCollections();
            InitDates();
            PreSave();
        }
        private void InitCollections()
        {
            CurrentTasks = new List<Task>();
            LinksRemainingList = new List<ArchiveLink>();
            LinkProcessingsCurrent = new ObservableCollection<LinkProcessingViewModel>();
            LinkProcessingEventErrors = new ObservableCollection<LinkProcessingEventArgs>();
            LinkProcessingsAll = new ObservableCollection<LinkProcessingViewModel>();
            var remainingLinks = Snapshot.Links.Where(l => l.MimeType == "text/html" && string.IsNullOrEmpty(l.HtmlFilePath));
            //LinksLoadedCount = Snapshot.Links.Length - remainingLinks.Count();
        }
        private void InitDates()
        {
            PauseState = new OperationState(false);
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
                await Task.Run(() => SnapshotView.Save(new SavingConfiguration(null, SaveMode.All)));
            }
        }


        private void RecountLinksForDownload()
        {
            LinksRemainingList.Clear();
            var links = LoadConfiguration.GetFilteredLinks();
            LinksRemainingList = new List<ArchiveLink>(links);
            //рассчитываем количество ссылок для загрузки
        }



        //Загрузка и запись файлов
        public ICommand StartDownloadCommand { get; private set; }
        public ICommand SaveProgressCommand { get; private set; }
        public ICommand StopProgressCommand { get; private set; }

        private void StartDownload(object obj)
        {
            TokenSource = new CancellationTokenSource();
            if (!SavingHtmlFolder.Exists)
            {
                SavingHtmlFolder.Create();
            }
            RecountLinksForDownload();

            Stopped = false;
            IsStarted = true;

            TokenCancel = TokenSource.Token;
            Task.Run(Downloading, TokenCancel);
        }
        private async void SaveProgress(object obj)
        {
            await Task.Run(() => SnapshotView.Save(new SavingConfiguration(null, SaveMode.All)));
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
                OnPropertyChanged(nameof(SavingHtmlFolder));
            }
        }
        private SnapshotView snapshotView;
        public Snapshot Snapshot => SnapshotView.SnapshotModel;
        public DirectoryInfo SavingHtmlFolder => SnapshotView.SavingHtmlFolder;


        //Процессы
        private ICollection<Task> CurrentTasks;
        private CancellationTokenSource TokenSource;
        private CancellationToken TokenCancel;


        //Настройки загрузки
        public HtmlLoadingConfiguration LoadConfiguration
        {
            get => loadConfiguration;
            private set => Set(ref loadConfiguration, value);
        }
        private HtmlLoadingConfiguration loadConfiguration;
        public LinkProcessingConfiguration ProcessingConfiguration
        {
            get => processingConfiguration;
            set => Set(ref processingConfiguration, value);
        }
        private LinkProcessingConfiguration processingConfiguration;




        //Прогресс загрузки
        private List<ArchiveLink> LinksRemainingList;
        public int LinksStartCount
        {
            get => linksStartCount;
            set => Set(ref linksStartCount, value);
        }
        private int linksStartCount;
        public int LinksRemainingCount => LinksRemainingList.Count;
        public int LinksLoadedCount => LinksStartCount - LinksRemainingCount;

        public IList<LinkProcessingViewModel> LinkProcessingsCurrent { get; private set; }
        public IList<LinkProcessingViewModel> LinkProcessingsAll { get; private set; }
        public IList<LinkProcessingEventArgs> LinkProcessingEventErrors { get; private set; }


        public LinkProcessingViewModel LastLinkProcessing
        {
            get => lastLinkProcessing;
            private set => Set(ref lastLinkProcessing, value);
        }
        private LinkProcessingViewModel lastLinkProcessing;






        //Обработка ссылки
        private void Downloading()
        {
            while (LinksRemainingList.Any())
            {
                if (LinkProcessingsCurrent.Count < 25 && PauseState.IsPlayed)
                {
                    var now = LinksRemainingList.LastOrDefault();
                    if (now != null)
                    {
                        Task newTask = Task.Run(() => LinkLoadAsync(now));
                        CurrentTasks.Add(newTask);
                        LinksRemainingList.Remove(now);
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

            DispatcherService.ExeInDispatcher(() => LinkProcessingsAll.Add(linkProcessingModel));
            DispatcherService.ExeInDispatcher(() => LinkProcessingsCurrent.Add(linkProcessingModel));

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
                //LinksLoadedCount++;
                //LastLinkProcessing = linkProcessing.Link as LinkProcessingViewModel;

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
            int progress = 1; //(int)((double)linksLoadedCount / Snapshot.Links.Length * 100);
            DispatcherService.ExeInDispatcher(() => MainWindowModel.SetOperation(new ProcessStatus($"Загрузка HTML страниц: {LinksLoadedCount} / {Snapshot.Links.Length}", progress, true, false)));
            //DispatcherService.ExeInDispatcher(() => LinkProcessingsCurrent.Remove(l => l. linkProcessing));
            linkProcessing.OnStatusChanged -= LinkProcess_OnStatusChanged;

            if (!LinksRemainingList.Any())
            {
                StopDownloading(null);
            }
        }


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
        public TimeSpan TimeLeft => TimeSpan.FromMinutes(LinksRemainingList.Count / SpeedLinksPerMinute);
        private void UpdateSpeed()
        {
            if (PauseState.IsPlayed && !Stopped)
            {
                OnPropertyChanged(nameof(PauseState.FromStart));
                OnPropertyChanged(nameof(SpeedLinksPerMinute));
                OnPropertyChanged(nameof(TimeLeft));
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
        public OperationState PauseState { get; private set; }
        private bool Stopped { get; set; }
    }
}
