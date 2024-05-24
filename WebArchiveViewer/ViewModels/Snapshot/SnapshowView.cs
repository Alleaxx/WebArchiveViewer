using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using WebArchive.Data;
using WebArchive.WpfUI.Helpers;
using WebArchiveViewer.WpfUI.Commands;

namespace WebArchiveViewer.ViewModels
{
    public class SnapshotView : NotifyObject
    {
        public MainWindowViewModel MainWindowViewModel { get; private set; }

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

        public ICommand SelectSaveFolderCommand { get; private set; }
        public ICommand UpdateCategoriesCommand { get; private set; }
        public ICommand SaveSnapshotFileCommand { get; private set; }
        public ICommand ClearProgressCommand { get; private set; }


        public SnapshotView(MainWindowViewModel mainViewModel, Snapshot snap)
        {
            MainWindowViewModel = mainViewModel;
            SnapshotModel = snap;
            RulesInfoView = new RulesInfoViewModel(this);
            ListViewInfo = new ListViewInfo(mainViewModel, SnapshotModel);
            DatesStatistics = new SnapshotDateStatistics(SnapshotModel);

            Status = snap.IsEmpty ? "нет" : "есть";

            SaveSnapshotFileCommand = new RelayCommand(async (obj) => await Save(obj));

            SelectSaveFolderCommand = new RelayCommand(SelectSaveFolder)
                .SetCondition(IsNotEmptySnapshot);
            UpdateCategoriesCommand = new RelayCommand(UpdateCategories)
                .SetCondition(IsNotEmptySnapshot);
            ClearProgressCommand = new RelayCommand(ClearProgress)
                .SetCondition(IsNotEmptySnapshot);
        }

        public void ReplaceRulesWith(GroupRule rule)
        {
            SnapshotModel.RulesControl = rule;
            RulesInfoView = new RulesInfoViewModel(this);
            UpdateCategories(null);
            OnPropertyChanged(nameof(RulesInfoView));
        }
        private bool IsNotEmptySnapshot(object obj)
        {
            return !SnapshotModel.IsEmpty;
        }

        private async Task Save(object obj)
        {
            if (obj is SavingConfiguration config)
            {
                await Save(config);
            }
        }
        public async Task Save(SavingConfiguration config)
        {
            IFileDialog fileDialog = new FileDialog();
            var links = ListViewInfo.GetFilteredLinks(config.Mode)
                .Where(l => l.Tag != ArchiveLink.RemoveTag);
            Snapshot saveCopy = SnapshotModel.CloneThis(links);

            string filePath;
            if (!string.IsNullOrEmpty(SnapshotModel.FilePath) && SavingFile.Exists && config.UseDefaultPath)
            {
                filePath = SnapshotModel.FilePath;
            }
            else
            {
                int linksCount = saveCopy.Links.Count();
                var file = new FileDialog().Save($"Ссылки {SnapshotModel.ReceivingDate:yyyy-MM-dd} (всего {linksCount})");
                if (file != null)
                {
                    filePath = file.FullName;
                }
                else
                {
                    return;
                }
            }

            await Task.Run(() => fileDialog.SaveFile(filePath, saveCopy));
            SnapshotModel.FilePath = filePath;
            UpdateFileProperties();
            OnPropertyChanged(nameof(SavingFile));
        }
        private void SelectSaveFolder(object obj)
        {
            IFileDialog dialog = new FileDialog();
            var folder = dialog.SelectFolder();
            if (folder != null && folder.Exists)
            {
                SnapshotModel.FolderHtmlSavePath = folder.FullName;
                OnPropertyChanged(nameof(SavingHtmlFolder));
            }
        }
        private void ClearProgress(object obj)
        {
            foreach (var link in SnapshotModel.Links)
            {
                link.HtmlFilePath = null;
            }
        }
        public void UpdateCategories(object obj)
        {
            ListViewInfo.LoadCategories(SnapshotModel);
        }

        private void UpdateFileProperties()
        {
            OnPropertyChanged(nameof(SavingFile));
            OnPropertyChanged(nameof(LastSavingDate));
        }
    }
}
