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

using WebArchive.Data;
using WebArchive.WpfUI.Helpers;
using WebArchiveViewer.UI;
using WebArchiveViewer.WpfUI.Collections.Paged;
using WebArchiveViewer.WpfUI.Commands;
using static System.Windows.Forms.Design.AxImporter;

namespace WebArchiveViewer.ViewModels
{
    /// <summary>
    /// Представление просмотра ссылок с архива
    /// </summary>
    public class MainWindowViewModel : NotifyObject
    {
        public SnapshotLoaderViewModel SnapshotLoader { get; private set; }
        public LinksLoaderViewModel LinksLoader { get; private set; }
        public HtmlLoaderViewModel HtmlLoader { get; private set; }

        public int SelectedMenuIndex
        {
            get => selectedMenuIndex;
            set => Set(ref selectedMenuIndex, value);
        }
        private int selectedMenuIndex;
        /// <summary>
        /// Текущая операция, отображается в статусе
        /// </summary>
        public Operation Operation
        {
            get => operation;
            set => Set(ref operation, value);
        }
        private Operation operation;

        /// <summary>
        /// Модель представления текущего снапшота ссылок. Не может быть null
        /// </summary>
        public SnapshotView SnapshotView
        {
            get => snapshotView;
            private set
            {
                Set(ref snapshotView, value);
                OnPropertyChanged(nameof(SnapshotIsEmpty));
                OnPropertyChanged(nameof(SnapshotIsNotEmpty));
            }
        }
        private SnapshotView snapshotView;

        public IPager<ArchiveLink> LinksPager
        {
            get => linksPager;
            private set => Set(ref linksPager, value);
        }
        private IPager<ArchiveLink> linksPager;

        public bool SnapshotIsEmpty => SnapshotView?.SnapshotModel.IsEmpty ?? true;
        public bool SnapshotIsNotEmpty => !SnapshotView?.SnapshotModel.IsEmpty ?? false;

        public ICommand CloseSnapshotCommand { get; private set; }

        public MainWindowViewModel()
        {
            SnapshotView = new SnapshotView(this, Snapshot.GetEmptySnapshot());
            SetNullSnapshot();
            HtmlLoader = new HtmlLoaderViewModel(this);
            SnapshotLoader = new SnapshotLoaderViewModel(this);
            LinksLoader = new LinksLoaderViewModel();
            LinksPager = new Pager<ArchiveLink>();

            SetOperation(new Operation("Ожидание ссылок...", 0));

            CloseSnapshotCommand = new RelayCommand(CloseSnapshot)
                .SetCondition(() => !SnapshotIsEmpty);
        }

        public void SetOperation(Operation status)
        {
            Operation = status;
        }
        public async void SetSnapshot(Snapshot value)
        {
            SnapshotView = new SnapshotView(this, value);
            if (value.IsNotEmpty)
            {
                SelectedMenuIndex = 1;
                HtmlLoader.SetSnapshot(SnapshotView);
                await UpdatePagerLinks();
            }
            else
            {
                SelectedMenuIndex = 0;
            }
        }
        public void SetNullSnapshot()
        {
            SetSnapshot(Snapshot.GetEmptySnapshot());
        }

        private void CloseSnapshot(object obj)
        {
            SetNullSnapshot();
            SetOperation(new Operation("Снапшот закрыт", 0));
        }

        public async Task UpdatePagerLinks()
        {
            if(SnapshotIsEmpty)
            {
                return;
            }

            var filteredLinks = await Task.Run(GetCurrentLinks);

            DispatcherHelper.ExeInDispatcher(() =>
            {
                LinksPager.UpdateCollection(filteredLinks);
            });
        }
        private IEnumerable<ArchiveLink> GetCurrentLinks()
        {
            var options = SnapshotView.ListViewInfo;
            var filteredLinks = options.GetFilteredLinks();
            filteredLinks = options.GroupSortsInfo.SortLinks(filteredLinks);
            options.LinksFilteredAmount = filteredLinks.Count();
            return filteredLinks;
        }
    }
}
