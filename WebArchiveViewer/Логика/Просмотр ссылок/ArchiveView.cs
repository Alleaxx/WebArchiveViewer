using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

using Newtonsoft.Json;

namespace WebArchiveViewer
{
    public enum SaveMode
    {
        allShowed,
        allFiltered,
        allNotFiltered,
        all,
        alldefaultpath,
    }

    //Представление просмотра ссылок с архива
    public class ArchiveView : NotifyObj
    {
        private IFileDialog FileDialog { get; set; }

        //Инициализация
        public ArchiveView()
        {
            FileDialog = new FileDialog(".json", "JSON-файл (*.json) |*.json");
        }
        protected override void InitCommands()
        {
            base.InitCommands();
            OpenArchiveLinksCommand = new RelayCommand(LoadLinks, RelayCommand.IsTrue);
            OpenSnapshotCommand = new RelayCommand(OpenSnapshotFile, RelayCommand.IsTrue);
            SaveSnapFileCommand = new RelayCommand(SaveSnapFile, IsSnapshotOpened);
            CloseSnapCommand = new RelayCommand(CloseSnapshot, IsSnapshotOpened);
        }


        public ISnapshot CurrentSnapshot
        {
            get => currentSnapshot;
            set
            {
                var oldValue = currentSnapshot;
                currentSnapshot = value;
                OnPropertyChanged();

                if (value != null)
                {
                    var options = value.ViewOptions;
                    options.LoadCategories(currentSnapshot);
                    options.Updated += UpdateList;
                    UpdateList();
                }
                if (oldValue != null && oldValue.ViewOptions != null)
                {
                    oldValue.ViewOptions.Updated -= UpdateList;
                }
            }
        }
        private ISnapshot currentSnapshot;
        private bool IsSnapshotOpened(object obj) => CurrentSnapshot != null;


        //Окна загрузки с веб-архива ссылок и HTML
        public ICommand OpenArchiveLinksCommand { get; private set; }
        private void LoadLinks(object obj)
        {
            LoadWindow window = new LoadWindow();
            window.ShowDialog();
        }

        //Открытие файла со ссылками
        public ICommand OpenSnapshotCommand { get; private set; }
        private void OpenSnapshotFile(object obj)
        {
            var file = FileDialog.Open();
            if (file != null && file.Exists)
            {
                string path = file.FullName;
                var snapshot = FileDialog.OpenReadJson<SiteSnapshot>(path);
                snapshot.FilePath = path;
                snapshot.ViewOptions.UpdateForSnapshot(snapshot);
                CurrentSnapshot = snapshot;
            }
        }


        //Сохранение ссылок в файл
        public ICommand SaveSnapFileCommand { get; private set; }
        private void SaveSnapFile(object obj)
        {
            SaveMode saveMode = GetSaveMode(obj);
            string path = GetSavingPath(saveMode);

            if(path != null)
            {
                CurrentSnapshot.Save(saveMode, path);
            }
        }

        private SaveMode GetSaveMode(object obj)
        {
            SaveMode saveMode = SaveMode.allShowed;
            if (obj != null && Enum.TryParse(obj.ToString(), out SaveMode mode))
                saveMode = mode;
            return saveMode;

        }
        private string GetSavingPath(SaveMode mode)
        {
            if (mode == SaveMode.alldefaultpath)
            {
                return CurrentSnapshot.File.FullName;
            }
            else
            {
                int linksCount = currentSnapshot.Links.Length;
                string link = currentSnapshot.SourceURI;

                var file = FileDialog.Save($"{linksCount} - {link}");
                if (file != null && file.Exists)
                    return file.FullName;
                else
                    return null;
            }
        }

        
        //Закрыть снапшот
        public ICommand CloseSnapCommand { get; private set; }
        private void CloseSnapshot(object obj)
        {
            CurrentSnapshot = null;
            LinksPager = null;
        }


        //Список отображаемых ссылок
        public IPager<ArchiveLink> LinksPager
        {
            get => linksPager;
            private set
            {
                linksPager = value;
                OnPropertyChanged();
            }
        }
        private IPager<ArchiveLink> linksPager;
        public void UpdateList()
        {
            if(currentSnapshot != null)
            {
                var options = CurrentSnapshot.ViewOptions;
                var links = options.GetFilteredLinks(currentSnapshot);
                links = options.ListView.SortLinks(currentSnapshot);
                options.LinksFilteredAmount = links.Count();

                LinksPager = new Pager<ArchiveLink>(links, options.ListView.GroupSelected);
            }
        }
    }
}
