using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Design;
using System.Windows.Input;
using WebArchive.Data;
using WebArchive.Data.Loaders;

namespace WebArchiveViewer.ViewModels
{
    public class SnapshotLoaderViewModel : NotifyObject
    {
        #region Ссылки

        public ArchiveMainWindowViewModel MainModel { get; private set; }

        #endregion

        #region Сервисы

        public FileDialog FileDialog { get; private set; }
        public ArchiveRequestBuilder RequestBuilder { get; private set; }

        #endregion

        public SnapshotLoaderViewModel() : this(null)
        {

        }
        public SnapshotLoaderViewModel(ArchiveMainWindowViewModel mainModel)
        {
            MainModel = mainModel;

            OpenRequestWindowCommand = new RelayCommand(OnOpenRequestWindowCommandExecuted);
            CopyRequestCommand = new RelayCommand(OnCopyRequestCommandExecuted);
            LoadFromRequestBuilderCommand = new RelayCommand(OnLoadFromRequestBuilderCommandExecuted, IsUploadingAvailable);
            LoadFromFileCommand = new RelayCommand(OnLoadFromFileCommandExecuted);
            SetSnapshotCommand = new RelayCommand(OnSetSnapshotCommandExecuted, IsNotEmptySnapshotReceived);

            FileDialog = new FileDialog();
            RequestBuilder= new ArchiveRequestBuilder();

            UploadingStatus = new ProcessProgress("Ожидание старта загрузки", 10);
        }

        //Результат загрузки
        public Snapshot Snapshot
        {
            get => snapshot;
            set => Set(ref snapshot, value);
        }
        private Snapshot snapshot;
        public ProcessProgress UploadingStatus { get; private set; }

        public string RequestString { get; set; }

        #region Команды

        public ICommand OpenRequestWindowCommand { get; private set; }
        public ICommand CopyRequestCommand { get; private set; }
        public ICommand LoadFromRequestBuilderCommand { get; private set; }
        public ICommand LoadFromFileCommand { get; private set; }
        public ICommand SetSnapshotCommand { get; private set; }

        //Условия
        private bool IsUploadingAvailable(object obj)
        {
            return (!string.IsNullOrEmpty(RequestBuilder.GetRequest())) && !UploadingStatus.InProgress;
        }
        private bool IsNotEmptySnapshotReceived(object obj)
        {
            return Snapshot != null && Snapshot.Links.Length > 0;
        }

        //Действия
        private void OnOpenRequestWindowCommandExecuted(object o)
        {
            LoadWindow window = new LoadWindow(this);
            window.Show();
        }
        private void OnCopyRequestCommandExecuted(object o)
        {
            System.Windows.Clipboard.SetText(RequestBuilder.GetRequest());
        }
        private async void OnLoadFromRequestBuilderCommandExecuted(object o)
        {
            var request = RequestBuilder.GetRequest();
            await LoadFromRequestString(request);
        }
        private async void OnLoadFromFileCommandExecuted(object o)
        {
            var file = FileDialog.Open();
            if (file == null || !file.Exists)
            {
                return;
            }
            string path = file.FullName;

            var fileLoader = new FileSnapshotLoader(path);
            fileLoader.OnStatusChanged += Loader_OnStatusChanged;
            Snapshot = await fileLoader.GetSnapshotAsync();
            fileLoader.OnStatusChanged -= Loader_OnStatusChanged;
            SendSnapshot();
        }
        private void OnSetSnapshotCommandExecuted(object o)
        {
            SendSnapshot();
        }


        public async Task<Snapshot> LoadFromRequestString(string request)
        {
            var requestLoader = new RequestSnapshotLoader(request);

            Snapshot = null;

            requestLoader.OnStatusChanged += Loader_OnStatusChanged;
            Snapshot = await requestLoader.GetSnapshotAsync();
            Snapshot.SourceURI = RequestBuilder.Site.Value;
            requestLoader.OnStatusChanged -= Loader_OnStatusChanged;
            return Snapshot;
        }

        #endregion


        private void Loader_OnStatusChanged(SnapshotLoaderEventArgs obj)
        {
            UploadingStatus.SetStatus(obj.Message, (int)obj.ReadyPersentage);
        }
        private void SendSnapshot()
        {
            if(MainModel == null)
            {
                return;
            }

            MainModel.SetSnapshot(Snapshot);
        }
    }
}
