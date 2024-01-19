using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using WebArchive.Data;

namespace WebArchiveViewer.ViewModels
{
    public class SnapshotView : NotifyObject
    {
        public string Status
        {
            get => status;
            set => Set(ref status, value);
        }
        private string status;

        public FileInfo SavingFile
        {
            get => string.IsNullOrEmpty(SnapshotModel?.FilePath) ? null : new FileInfo(SnapshotModel.FilePath);
        }
        public DateTime LastSavingDate => SavingFile != null ? SavingFile.LastWriteTime : new DateTime();

        public Snapshot SnapshotModel { get; private set; }
        public SnapshotDateStatistics DatesStatistics { get; private set; }
        public RulesInfoViewModel RulesInfoView { get; private set; }
        public ListViewInfo ListViewInfo { get; private set; }
        public DirectoryInfo SavingHtmlFolder => new DirectoryInfo(SnapshotModel.FolderHtmlSavePath);

        public SnapshotView(Snapshot snap)
        {
            SnapshotModel = snap;
            RulesInfoView = new RulesInfoViewModel(this);
            ListViewInfo = new ListViewInfo(SnapshotModel);
            DatesStatistics = new SnapshotDateStatistics(SnapshotModel);

            Status = snap.IsEmpty ? "нет" : "есть";

            CreateCommands();
        }

        #region Команды
        private void CreateCommands()
        {
            SelectSaveFolderCommand = new RelayCommand(OnSelectSaveFolderExecuted, IsNotEmptySnapshot);
            SaveSnapshotFileCommand = new RelayCommand(Save, IsNotEmptySnapshot);

            UpdateCategoriesCommand = new RelayCommand(OnUpdateCategoriesExecuted, IsNotEmptySnapshot);
            ClearProgressCommand = new RelayCommand(OnClearProgressExecuted, IsNotEmptySnapshot);
        }
        public ICommand SelectSaveFolderCommand { get; private set; }
        public ICommand UpdateCategoriesCommand { get; private set; }
        public ICommand SaveSnapshotFileCommand { get; private set; }
        public ICommand ClearProgressCommand { get; private set; }

        //Условия
        private bool IsNotEmptySnapshot(object obj)
        {
            return !SnapshotModel.IsEmpty;
        }

        //Команды
        public void Save(SavingConfiguration config)
        {
            Save(config as object);
        }
        private async void Save(object obj)
        {
            IFileDialog fileDialog = new FileDialog();
            if(!(obj is SavingConfiguration saveConfig))
            {
                return;
            }
            Snapshot saveCopy = SnapshotModel.CloneThis(ListViewInfo.GetFilteredLinks(saveConfig.Mode));

            string filePath = null;
            if (!string.IsNullOrEmpty(SnapshotModel.FilePath) && SavingFile.Exists && saveConfig.UseDefaultPath)
            {
                filePath = SnapshotModel.FilePath;
            }
            else
            {
                int linksCount = saveCopy.Links.Count();
                var file = new FileDialog().Save($"{SnapshotModel.ReceivingDate:yyyy-MM-dd} снапшот - {linksCount} ссылок");
                if(file != null)
                {
                    filePath = file.FullName;
                }
            }
            await Save();


            async Task Save()
            {
                if (!string.IsNullOrEmpty(filePath))
                {
                    await Task.Run(() => fileDialog.SaveFile(filePath, saveCopy));
                    SaveComplete(filePath);
                }
            }
            void SaveComplete(string path)
            {
                SnapshotModel.FilePath = path;
                UpdateSaveFileInfo();
                OnPropertyChanged(nameof(SavingFile));
            }
        }
        private void OnSelectSaveFolderExecuted(object obj)
        {
            IFileDialog dialog = new FileDialog();
            var folder = dialog.SelectFolder();
            if (folder != null && folder.Exists)
            {
                SnapshotModel.FolderHtmlSavePath = folder.FullName;
                OnPropertyChanged(nameof(SavingHtmlFolder));
            }
        }
        private void OnClearProgressExecuted(object obj)
        {
            foreach (var link in SnapshotModel.Links)
            {
                link.HtmlFilePath = null;
            }
        }
        public void OnUpdateCategoriesExecuted(object obj)
        {
            ListViewInfo.LoadCategories(SnapshotModel);
        }


        private void UpdateSaveFileInfo()
        {
            OnPropertyChanged(nameof(SavingFile));
            OnPropertyChanged(nameof(LastSavingDate));
        }

        #endregion
    }
}
