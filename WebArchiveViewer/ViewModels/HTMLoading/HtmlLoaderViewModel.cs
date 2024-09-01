using Microsoft.Win32;
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
using WebArchive.WpfUI.Helpers;
using WebArchiveViewer.Services;
using WebArchiveViewer.WpfUI.Commands;

namespace WebArchiveViewer.ViewModels
{
    public class HtmlLoaderViewModel : NotifyObject
    {
        public MainWindowViewModel MainWindowModel { get; private set; }

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
        private DirectoryInfo savingHtmlFOlder;


        private ICollection<Task> CurrentTasks;
        private CancellationTokenSource TokenSource;
        private CancellationToken TokenCancel;


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


        private List<ArchiveLink> LinksRemainingList;
        public int LinksMaxCount => SnapshotView.ListViewInfo.LinksFilteredAmount;
        public int LinksStartCount
        {
            get => linksStartCount;
            set => Set(ref linksStartCount, value);
        }
        private int linksStartCount;
        public int LinksRemainingCount => LinksRemainingList.Count;
        public int LinksLoadedCount => LinksStartCount - LinksRemainingCount;

        public ObservableCollection<LinkProcessingViewModel> LinkProcessingsCurrent { get; private set; }
        public ObservableCollection<LinkProcessingViewModel> LinkProcessingsAll { get; private set; }
        public ObservableCollection<LinkProcessingEventArgs> LinkProcessingEventErrors { get; private set; }


        public LinkProcessingViewModel LastLinkProcessing
        {
            get => lastLinkProcessing;
            private set => Set(ref lastLinkProcessing, value);
        }
        private LinkProcessingViewModel lastLinkProcessing;


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
        public OperationState PauseState
        {
            get => pauseState;
            set => Set(ref pauseState, value);
        }
        private OperationState pauseState;
        private bool Stopped { get; set; }


        public ICommand StartDownloadCommand { get; private set; }
        public ICommand SaveProgressCommand { get; private set; }
        public ICommand StopProgressCommand { get; private set; }
        public ICommand TogglePauseCommand { get; private set; }


        public HtmlLoaderViewModel(MainWindowViewModel mainModel)
        {
            MainWindowModel = mainModel;
            LinksRemainingList = new List<ArchiveLink>();
            LinkProcessingsCurrent = new ObservableCollection<LinkProcessingViewModel>();
            LinkProcessingEventErrors = new ObservableCollection<LinkProcessingEventArgs>();
            LinkProcessingsAll = new ObservableCollection<LinkProcessingViewModel>();

            SetSnapshot(mainModel.SnapshotView);

            StartDownloadCommand = new RelayCommand(StartDownload)
                .SetCondition(() => !IsReady);
            SaveProgressCommand = new RelayCommand(async () => await SaveProgressAsync())
                .SetCondition(() => IsReady);
            StopProgressCommand = new RelayCommand(async () => await StopDownloadingAsync())
                .SetCondition(() => IsReady);
            TogglePauseCommand = new RelayCommand(TogglePause)
                .SetCondition(() => IsReady);
        }
        public void TogglePause()
        {
            PauseState.IsPaused = !PauseState.IsPaused;
        }

        public void SetSnapshot(SnapshotView snapshot)
        {
            SnapshotView = snapshot;
            LoadConfiguration = new HtmlLoadingConfiguration(Snapshot.Links.Any() ? Snapshot.Links.Length : 0);
            LoadConfiguration.SetLinksSource(snapshot);
            LoadConfiguration.PropertyChanged += LoadConfiguration_PropertyChanged;
            SnapshotView.PropertyChanged += LoadConfiguration_PropertyChanged;
            ProcessingConfiguration = new LinkProcessingConfiguration()
            {
                LoadingTitle = true,
                SavingHtml = true,
                FolderPath = SavingHtmlFolder.FullName
            };

            CurrentTasks = new List<Task>();
            LinksRemainingList.Clear();
            var remainingLinks = Snapshot.Links.Where(l => l.MimeType == "text/html" && string.IsNullOrEmpty(l.HtmlFilePath));
            LinkProcessingsCurrent.Clear();
            LinkProcessingEventErrors.Clear();
            LinkProcessingsAll.Clear();

            PauseState = new OperationState(false);
            RecountLinksForDownload();
        }
        private void LoadConfiguration_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ProcessingConfiguration.FolderPath = SavingHtmlFolder.FullName;
            RecountLinksForDownload();
        }

        private void RecountLinksForDownload()
        {
            if(Snapshot.IsEmpty)
            {
                return;
            }

            LinksRemainingList.Clear();

            var links = LoadConfiguration.GetFilteredLinks();
            LinksRemainingList = new List<ArchiveLink>(links);
            LinksStartCount = links.Count();
            OnPropertyChanged(nameof(LinksMaxCount));
            //рассчитываем количество ссылок для загрузки
        }

        private void UpdateSpeed()
        {
            if (PauseState.IsPlayed && !Stopped)
            {
                OnPropertyChanged(nameof(PauseState.FromStart));
                OnPropertyChanged(nameof(SpeedLinksPerMinute));
                OnPropertyChanged(nameof(TimeLeft));
            }
        }
        private void UpdateProperties()
        {
            OnPropertyChanged(nameof(LinksRemainingCount));
            OnPropertyChanged(nameof(LinksLoadedCount));
        }


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
        private async Task SaveProgressAsync()
        {
            try
            {
                await SnapshotView.Save(new SavingConfiguration(null, SaveMode.All) { UseDefaultPath = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        private async Task StopDownloadingAsync()
        {
            if(TokenSource == null)
            {
                return;
            }
            PauseState.IsPaused = true;
            Stopped = true;
            await Task.Run(TokenSource.Cancel);
            await Task.Run(SaveProgressAsync);
            OnPropertyChanged(nameof(IsPauseEnabled));
            MainWindowModel.SetOperation(new Operation($"Загрузка HTML страниц завершена", 100, true, true));
        }

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

            DispatcherHelper.ExeInDispatcher(() =>
            {
                LinkProcessingsAll.Add(linkProcessingModel);
                LinkProcessingsCurrent.Add(linkProcessingModel);
                UpdateSpeed();
            });

            linkProcess.OnStatusChanged += LinkProcess_OnStatusChanged;
            await linkProcess.StartProcessing();
        }

        private void LinkProcess_OnStatusChanged(LinkProcessingEventArgs obj)
        {
            if (!obj.State.IsSuccessfull)
            {
                DispatcherHelper.ExeInDispatcher(() => LinkProcessingEventErrors.Insert(0, obj));
            }

            if (obj.State.IsEnded)
            {
                LinkProcessingFinished(obj);
            }
        }
        private async void LinkProcessingFinished(LinkProcessingEventArgs eventArgs)
        {
            var linkProcessing = eventArgs.Sender;
            bool success = eventArgs.State.IsSuccessfull;

            var vm = GetVm(eventArgs);
            DispatcherHelper.ExeInDispatcher(() => LinkProcessingsCurrent.Remove(vm));
            LastLinkProcessing = vm;
            int progress = (int)((double)LinksLoadedCount / Snapshot.Links.Length * 100);
            DispatcherHelper.ExeInDispatcher(() =>
            {
                MainWindowModel.SetOperation(new Operation($"Загрузка HTML страниц: {LinksLoadedCount} / {Snapshot.Links.Length}", progress, true, false));
                UpdateProperties();
                LinkProcessingsCurrent.Remove(vm);
            });

            linkProcessing.OnStatusChanged -= LinkProcess_OnStatusChanged;

            if (!LinksRemainingList.Any())
            {
                await StopDownloadingAsync();
            }
        }

        private LinkProcessingViewModel GetVm(LinkProcessingEventArgs eventArgs)
        {
            return LinkProcessingsCurrent.ToArray().FirstOrDefault(l => l.Link == eventArgs.Sender.Link);
        }
    }
}
