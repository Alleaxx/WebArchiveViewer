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

namespace WebArchiveViewer
{
    public class SaveHtmlWindowViewModel : NotifyObject
    {
        public SaveHtmlWindowViewModel(SnapshotView snapshot) : base()
        {
            if(snapshot == null || snapshot.CurrentSnapshot == null)
            {
                throw new ArgumentNullException();
            }

            SnapshotView = snapshot;
            Options = new LoadOptions(Snapshot.Links.Length);
            ProcessingConfiguration = new LinkProcessingConfiguration()
            {
                LoadingTitle = true,
                SavingHtml = true,
                FolderPath = FolderWrite.FullName
            };

            InitCollections();
            InitDates();
            PreSave();

            void InitCollections()
            {
                CurrentRequests = new ObservableCollection<LinkProcessing>();
                SuccessfullRequests = new ObservableCollection<LinkProcessing>();
                ErrorRequests = new ObservableCollection<LinkProcessing>();
                var remainingLinks = Snapshot.Links.Where(l => l.MimeType == "text/html" && string.IsNullOrEmpty(l.HtmlFilePath));
                LinksRemaining = new ObservableCollection<ArchiveLink>(remainingLinks);
                LinksLoadedCount = Snapshot.Links.Length - remainingLinks.Count();
            }
            void InitDates()
            {
                PauseState = new PauseState(false);
            }
            async void PreSave()
            {
                if (!string.IsNullOrEmpty(Snapshot.FilePath))
                {
                    await Task.Run(() => snapshot.Save(SaveMode.AllDefaultPath));
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

        private void StartDownload(object obj)
        {
            Client = new HttpClient();
            TokenSource = new CancellationTokenSource();
            Client.Timeout = Options.Timeout;
            if (!FolderWrite.Exists)
            {
                FolderWrite.Create();
            }
            ErrorRequests.Clear();

            Stopped = false;
            IsStarted = true;

            TokenCancel = TokenSource.Token;
            Task.Run(Downloading, TokenCancel);
        }
        private async void SaveProgress(object obj)
        {
            await Task.Run(() => SnapshotView.Save(SaveMode.AllDefaultPath));
        }
        private async void StopProgress(object obj)
        {
            if (TokenSource != null)
            {
                PauseState.IsPaused = true;
                Stopped = true;
                await Task.Run(TokenSource.Cancel);
                if (Client != null)
                {
                    Client.Dispose();
                }
                SaveProgress(null);
                OnPropertyChanged(nameof(IsPauseEnabled));
            }
        }


        //Откуда и куда
        public Snapshot Snapshot => SnapshotView.CurrentSnapshot;
        public SnapshotView SnapshotView { get; private set; }
        public DirectoryInfo FolderWrite => SnapshotView.SavingFolderHtmlContent;
        private HttpClient Client { get; set; }
        private CancellationTokenSource TokenSource { get; set; }
        private CancellationToken TokenCancel { get; set; }

        //Настройки загрузки
        public LoadOptions Options { get; private set; }
        public LinkProcessingConfiguration ProcessingConfiguration { get; private set; }



        private void Downloading()
        {
            List<Task> tasks = new List<Task>();
            while (LinksRemaining.Any())
            {
                if (CurrentRequests.Count < 25 && PauseState.IsPlayed)
                {
                    var now = LinksRemaining.LastOrDefault();
                    if(now != null)
                    {
                        Task newTask = Task.Run(() => LinkLoadAsync(now));
                        tasks.Add(newTask);
                        LinksRemaining.Remove(now);
                    }
                }
                if (TokenCancel.IsCancellationRequested)
                {
                    break;
                }
                Thread.Sleep(250);
            }
            Task.WaitAll(tasks.ToArray());
        }


        //Прогресс загрузки
        public ObservableCollection<ArchiveLink> LinksRemaining { get; private set; }

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

        public ObservableCollection<LinkProcessing> CurrentRequests { get; private set; }
        public ObservableCollection<LinkProcessing> SuccessfullRequests { get; private set; }
        public ObservableCollection<LinkProcessing> ErrorRequests { get; private set; }


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
        private async Task LinkLoadAsync(ArchiveLink link)
        {
            var linkProcess = new LinkProcessing(link, ProcessingConfiguration);
            linkProcess.SetClient(Client);
            linkProcess.SetIndex(SuccessfullRequests.Count + CurrentRequests.Count);

            ExeInDispatcher(() => CurrentRequests.Add(linkProcess));

            linkProcess.OnEnded += LinkProcess_OnEnded;
            await linkProcess.StartProcessing();
        }
        private async void LinkProcess_OnEnded(LinkProcessingEventArgs arg1, string arg2, string arg3, FileInfo arg4)
        {
            var linkProcessing = arg1.Sender;
            bool success = !arg1.Error;

            if (success)
            {
                ProcessSuccess(linkProcessing);
            }
            else
            {
                ProcessFail(linkProcessing);
            }

            ExeInDispatcher(() => CurrentRequests.Remove(linkProcessing));
        }

        private void ProcessSuccess(LinkProcessing linkProcessing)
        {
            LinksLoadedCount++;
            LastLink = linkProcessing.Link as ArchiveLink;
            ExeInDispatcher(() => SuccessfullRequests.Add(linkProcessing));
            bool saveCounter = linkProcessing.Index % Options.SavingLatencyLinks == 0;
            if (saveCounter)
            {
                SaveProgress(null);
                UpdateSpeed();
            }
        }
        private void ProcessFail(LinkProcessing linkProcessing)
        {
            ExeInDispatcher(() => ErrorRequests.Add(linkProcessing));
        }
        
        private void ExeInDispatcher(Action action)
        {
            Application.Current.Dispatcher.BeginInvoke(action);
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
