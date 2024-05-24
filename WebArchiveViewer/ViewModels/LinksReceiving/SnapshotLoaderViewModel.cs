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
using WebArchive.WpfUI.Helpers;
using WebArchiveViewer.Services;
using WebArchiveViewer.WpfUI.Commands;

namespace WebArchiveViewer.ViewModels
{
    public class SnapshotLoaderViewModel : NotifyObject
    {
        public MainWindowViewModel MainModel { get; private set; }
        public SnapshotView SnapshotView => MainModel.SnapshotView;


        public FileDialog FileDialog { get; private set; }
        public ArchiveRequestBuilder RequestBuilder { get; private set; }
        public RequestFiltersViewModel RequestFilters { get; private set; }

        private CancellationTokenSource CancellationTokenSource;

        public Operation Status
        {
            get => status;
            set => Set(ref status, value);
        }
        private Operation status;

        public bool IsProcessing
        {
            get => isProcessing;
            set => Set(ref isProcessing, value);
        }
        private bool isProcessing;

        public IList<SnapshotLoaderEventArgs> LoadingEventsList { get; private set; }

        public string RequestString => requestString ?? RequestBuilder.GetRequest();
        private string requestString;


        public ICommand BreakRequestCommand { get; private set; }
        public ICommand CopyRequestCommand { get; private set; }
        public ICommand LoadFromRequestBuilderCommand { get; private set; }
        public ICommand LoadFromFileCommand { get; private set; }
        public ICommand LoadCategoriesFromFileCommand { get; private set; }


        public SnapshotLoaderViewModel() : this(null)
        {

        }
        public SnapshotLoaderViewModel(MainWindowViewModel mainModel)
        {
            MainModel = mainModel;

            CopyRequestCommand = new RelayCommand(CopyRequest);
            LoadFromRequestBuilderCommand = new RelayCommand(LoadFromRequestBuilder)
                .SetCondition(IsUploadingAvailable);
            LoadFromFileCommand = new RelayCommand(LoadFromFile);
            LoadCategoriesFromFileCommand = new RelayCommand(OnLoadCategoriesFromFileCommandExecuted);
            BreakRequestCommand = new RelayCommand(OnBreakRequestCommandExecuted)
                .SetCondition(IsBreakingAvailable);

            FileDialog = new FileDialog();
            RequestBuilder = new ArchiveRequestBuilder();
            RequestFilters = new RequestFiltersViewModel(RequestBuilder);

            LoadingEventsList = new ObservableCollection<SnapshotLoaderEventArgs>();

            Status = new Operation("Ожидание начала загрузки", 0);
        }

        private bool IsUploadingAvailable()
        {
            return !string.IsNullOrEmpty(RequestBuilder.GetRequest()) && !IsProcessing;
        }
        private bool IsBreakingAvailable()
        {
            return CancellationTokenSource != null && IsProcessing;
        }

        private async void OnBreakRequestCommandExecuted(object obj)
        {
            await Task.Run(() => CancellationTokenSource.Cancel());
        }
        private void CopyRequest(object o)
        {
            OnPropertyChanged(nameof(RequestString));
            System.Windows.Clipboard.SetText(RequestBuilder.GetRequest());
        }
        private async void LoadFromRequestBuilder()
        {
            var request = RequestBuilder.GetRequest();
            OnPropertyChanged(nameof(RequestString));
            var snapshot = await LoadFromRequestString(request);
            SendSnapshot(snapshot);
        }
        private async void LoadFromFile()
        {
            var file = FileDialog.Open();
            if (file == null || !file.Exists)
            {
                return;
            }

            var snapshot = await LoadFromFile(file.FullName);
            SendSnapshot(snapshot);
        }
        private async void OnLoadCategoriesFromFileCommandExecuted(object o)
        {
            var file = FileDialog.Open();
            if (file == null || !file.Exists)
            {
                return;
            }

            var snapshot = await LoadFromFile(file.FullName);
            MainModel.SnapshotView.ReplaceRulesWith(snapshot.RulesControl);
        }

        private void ReadyCleanup()
        {
            IsProcessing = true;
            LoadingEventsList.Clear();
            CancellationTokenSource = new CancellationTokenSource();
        }

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
            var snapshot = await Task.Run(() => fileLoader.GetSnapshotAsync(CancellationTokenSource.Token));
            fileLoader.OnStatusChanged -= Loader_OnStatusChanged;
            return snapshot;
        }

        private void Loader_OnStatusChanged(SnapshotLoaderEventArgs obj)
        {
            DispatcherHelper.ExeInDispatcher(() =>
            {
                LoadingEventsList.Insert(0, obj);
                Status = obj.State;
                MainModel.SetOperation(obj.State);
            });
        }
        private void SendSnapshot(Snapshot snapshot)
        {
            if(MainModel == null)
            {
                return;
            }
            if (snapshot.IsEmpty)
            {
                Status = new Operation("Загрузка не удалась или ответ вернул 0 ссылок", 100, true, true);
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
