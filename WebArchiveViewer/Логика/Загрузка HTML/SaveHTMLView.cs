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

namespace WebArchiveViewer
{
    class SaveHTMLView : NotifyObj
    {
        //Инициализация
        public SaveHTMLView()
        {
            InitCommands();
            FolderWrite = new DirectoryInfo(@"D:\Users\Allexx\Documents\Румайн\Проекты\История Румине\ProjectArch 2.0");
            if(AppView.Ex.Archive.CurrentSnapshot != null)
                Snapshot = AppView.Ex.Archive.CurrentSnapshot;
            ActiveLinkRequests = new ObservableCollection<LinkState>();
            ErrorLinkRequests = new ObservableCollection<LinkState>();
            FinishedLinkRequests = new ObservableCollection<LinkState>();
        }
        private void InitCommands()
        {
            SelectSaveFolderCommand = new RelayCommand(SelectSaveFolder, obj => !IsStarted);
            StartDownloadCommand = new RelayCommand(StartDownload, obj =>  !IsStarted);
            SaveProgressCommand = new RelayCommand(SaveProgress, obj => IsStarted);
            StopProgressCommand = new RelayCommand(StopProgress, obj => IsStarted);
        }

        //Задание источника для скачивания
        public DirectoryInfo FolderWrite
        {
            get => folderWrite;
            set
            {
                folderWrite = value;
                OnPropertyChanged();
            }
        }
        private DirectoryInfo folderWrite;
        public SiteSnapshot Snapshot
        {
            get => snapshot;
            set
            {
                snapshot = value;
                OnPropertyChanged();
                LinksRemainingList = new ObservableCollection<ArchiveLink>(value.Links.Where(l => string.IsNullOrEmpty(l.ActualState)));
                LinksLoadedCount = value.Links.Length - linksRemainingList.Count;
            }
        }
        private SiteSnapshot snapshot;


        public RelayCommand SelectSaveFolderCommand { get; set; }
        private void SelectSaveFolder(object obj)
        {

            Ookii.Dialogs.Wpf.VistaFolderBrowserDialog dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            
            if (dialog.ShowDialog() == true)
            {
                FolderWrite = new DirectoryInfo(dialog.SelectedPath);
            }
        }


        //Загрузка и запись файлов
        public bool IsStarted
        {
            get => isStarted;
            set
            {
                isStarted = value;
                OnPropertyChanged();
            }
        }
        private bool isStarted = false;
        public DateTime StartDateTime { get; set; }
        public RelayCommand StartDownloadCommand { get; set; }
        private async void StartDownload(object obj)
        {
            if (!FolderWrite.Exists)
            {
                FolderWrite.Create();
            }

            Task loading = new Task(DownloadingV2);
            loading.Start();
            IsStarted = true;
            StartDateTime = DateTime.Now;
            OnPropertyChanged(nameof(StartDateTime));
            await loading;
        }

        //Прогресс загрузки
        public ObservableCollection<ArchiveLink> LinksRemainingList
        {
            get => linksRemainingList;
            set
            {
                linksRemainingList = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<ArchiveLink> linksRemainingList;

        public int LinksLoadedCount
        {
            get => linksLoadedCount;
            set
            {
                linksLoadedCount = value;
                OnPropertyChanged();
            }
        }
        private int linksLoadedCount;

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

        //Состояние паузы
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
        public PauseState PauseState { get; set; } = new PauseState(false);


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
        public ObservableCollection<LinkState> ActiveLinkRequests { get; set; }
        public ObservableCollection<LinkState> FinishedLinkRequests { get; set; }
        public ObservableCollection<LinkState> ErrorLinkRequests { get; set; }
        private void DownloadingV2()
        {
            ArchiveLink[] links = LinksRemainingList.ToArray();
            for (int i = 0; i < links.Length; )
            {
                Thread.Sleep(LatencyMs);
                if (!IsPaused && ActiveLinkRequests.Count <= 5)
                {
                    ArchiveLink link = links[i];
                    Task currentLinkTask = new Task(() => StartRequest(i,link));
                    currentLinkTask.Start();
                    i++;
                    if (i % 20 == 0)
                        SaveProgress(null);
                }
            }
            StopProgress(null);
        }
        private async void StartRequest(int index,ArchiveLink link)
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
                        string fileName = $"{FolderWrite.FullName}\\{link.Index}.html";
                        using (FileStream fstream = new FileStream(fileName, FileMode.OpenOrCreate))
                        {
                            byte[] array = Encoding.Default.GetBytes(reader.ReadToEnd());
                            await fstream.WriteAsync(array, 0, array.Length);

                            LinksLoadedCount++;
                            LinksRemainingList.Remove(link);
                            state.Status = $"Записан в {fileName}";
                            link.ActualState = "200";
                            await Application.Current.Dispatcher.BeginInvoke(new Action(() => FinishedLinkRequests.Add(state)));
                            await Application.Current.Dispatcher.BeginInvoke(new Action(() => ActiveLinkRequests.Remove(state)));
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                link.ActualState = "404";
                state.Status = ex.Message;
                await Application.Current.Dispatcher.BeginInvoke(new Action(() => ActiveLinkRequests.Remove(state)));
                await Application.Current.Dispatcher.BeginInvoke(new Action(() => ErrorLinkRequests.Add(state)));
            }
        }

        public DateTime LastSaveDate { get; set; }
        public RelayCommand SaveProgressCommand { get; set; }
        private void SaveProgress(object obj)
        {
            Snapshot.Save(FolderWrite.FullName + "\\temp.json");
            LastSaveDate = DateTime.Now;
            OnPropertyChanged(nameof(LastSaveDate));
        }

        private bool Stopped { get; set; }
        public RelayCommand StopProgressCommand { get; set; }
        private void StopProgress(object obj)
        {
            IsPaused = true;
            Stopped = true;
            SaveProgress(null);
        }
    }
    class PauseState
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
    class LinkState : NotifyObj
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
