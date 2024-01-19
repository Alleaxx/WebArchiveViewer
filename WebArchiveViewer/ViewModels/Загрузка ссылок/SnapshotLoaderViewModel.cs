using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms.Design;
using System.Windows.Input;
using WebArchive.Data;
using WebArchive.Data.Loaders;
using WebArchive.Data.RequestParts;
using WebArchiveViewer.Services;

namespace WebArchiveViewer.ViewModels
{
    public class SnapshotLoaderViewModel : NotifyObject
    {
        #region Ссылки

        public MainWindowViewModel MainModel { get; private set; }
        public SnapshotView SnapshotView => MainModel.SnapshotView;

        #endregion

        #region Сервисы

        public FileDialog FileDialog { get; private set; }
        public ArchiveRequestBuilder RequestBuilder { get; private set; }
        public RequestFiltersViewModel RequestFilters { get; private set; }

        private CancellationTokenSource CancellationTokenSource;

        #endregion

        public SnapshotLoaderViewModel() : this(null)
        {

        }
        public SnapshotLoaderViewModel(MainWindowViewModel mainModel)
        {
            MainModel = mainModel;

            CopyRequestCommand = new RelayCommand(OnCopyRequestCommandExecuted);
            LoadFromRequestBuilderCommand = new RelayCommand(OnLoadFromRequestBuilderCommandExecuted, IsUploadingAvailable);
            LoadFromFileCommand = new RelayCommand(OnLoadFromFileCommandExecuted);
            BreakRequestCommand = new RelayCommand(OnBreakRequestCommandExecuted, IsBreakingAvailable);

            FileDialog = new FileDialog();
            RequestBuilder = new ArchiveRequestBuilder();
            RequestFilters = new RequestFiltersViewModel(RequestBuilder);

            LoadingEventsList = new ObservableCollection<SnapshotLoaderEventArgs>();

            Status = new ProcessStatus("Ожидание начала загрузки", 0);
        }

        public ProcessStatus Status
        {
            get => status;
            set => Set(ref status, value);
        }
        private ProcessStatus status;

        public bool IsProcessing
        {
            get => isProcessing;
            set => Set(ref isProcessing, value);
        }
        private bool isProcessing;

        public IList<SnapshotLoaderEventArgs> LoadingEventsList { get; private set; }

        public string RequestString => requestString ?? RequestBuilder.GetRequest();
        private string requestString;


        #region Команды

        public ICommand BreakRequestCommand { get; private set; }
        public ICommand CopyRequestCommand { get; private set; }
        public ICommand LoadFromRequestBuilderCommand { get; private set; }
        public ICommand LoadFromFileCommand { get; private set; }


        //Условия
        private bool IsUploadingAvailable(object obj)
        {
            return !string.IsNullOrEmpty(RequestBuilder.GetRequest()) && !IsProcessing;
        }
        private bool IsBreakingAvailable(object obj)
        {
            return CancellationTokenSource != null && IsProcessing;
        }

        //Действия
        private async void OnBreakRequestCommandExecuted(object obj)
        {
            await Task.Run(() => CancellationTokenSource.Cancel());
        }
        private void OnCopyRequestCommandExecuted(object o)
        {
            OnPropertyChanged(nameof(RequestString));
            System.Windows.Clipboard.SetText(RequestBuilder.GetRequest());
        }
        private async void OnLoadFromRequestBuilderCommandExecuted(object o)
        {
            var request = RequestBuilder.GetRequest();
            OnPropertyChanged(nameof(RequestString));
            var snapshot = await LoadFromRequestString(request);
            SendSnapshot(snapshot);
        }
        private async void OnLoadFromFileCommandExecuted(object o)
        {
            var file = FileDialog.Open();
            if (file == null || !file.Exists)
            {
                return;
            }

            var snapshot = await LoadFromFile(file.FullName);            
            SendSnapshot(snapshot);
        }

        private void ReadyCleanup()
        {
            IsProcessing = true;
            LoadingEventsList.Clear();
            CancellationTokenSource = new CancellationTokenSource();
        }
        #endregion

        public async Task<Snapshot> LoadFromRequestString(string request)
        {
            var requestLoader = new SnapshotRequestLoader(request, HttpService.GetHttpClient());
            ReadyCleanup();
            requestLoader.OnStatusChanged += Loader_OnStatusChanged;
            var receivedSnap = await requestLoader.GetSnapshotAsync(CancellationTokenSource.Token);
            receivedSnap.SourceURI = RequestBuilder.Site.Value;
            var snapshot = receivedSnap;
            requestLoader.OnStatusChanged -= Loader_OnStatusChanged;
            return snapshot;
        }
        public async Task<Snapshot> LoadFromFile(string path)
        {
            var fileLoader = new SnapshotFileLoader(path);
            ReadyCleanup();
            fileLoader.OnStatusChanged += Loader_OnStatusChanged;
            var snapshot = await fileLoader.GetSnapshotAsync(CancellationTokenSource.Token);
            fileLoader.OnStatusChanged -= Loader_OnStatusChanged;
            return snapshot;
        }

        private void Loader_OnStatusChanged(SnapshotLoaderEventArgs obj)
        {
            LoadingEventsList.Insert(0, obj);
            Status = obj.State;
            MainModel.SetOperation(obj.State);
        }
        private void SendSnapshot(Snapshot snapshot)
        {
            if(MainModel == null)
            {
                return;
            }
            if (snapshot.IsEmpty)
            {
                Status = new ProcessStatus("Загрузка не удалась или ответ вернул 0 ссылок", 100, true, true);
                IsProcessing = false;
                return;
            }

            CancellationTokenSource = null;
            IsProcessing = false;

            MainModel.SetSnapshot(snapshot);
            OnPropertyChanged(nameof(SnapshotView));
        }
    }
}
