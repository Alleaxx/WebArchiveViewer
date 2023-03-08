using Newtonsoft.Json.Linq;
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
using WebArchiveViewer.ViewModels;

namespace WebArchiveViewer
{
    //Представление просмотра ссылок с архива
    public class MainWindowViewModel : NotifyObject
    {
        //Получение снапшота
        public SnapshotLoaderViewModel SnapshotLoader { get; private set; }
        public LinksProcessor LinkLoader { get; private set; }

        /// <summary> Текущая операция, отображается в статусе </summary>
        public ProcessStatus Operation
        {
            get => operation;
            set => Set(ref operation, value);
        }
        private ProcessStatus operation;

        public MainWindowViewModel()
        {
            LoadHtmlView = new HtmlLoaderViewModel(this);
            SetNullSnapshot();
            SnapshotLoader = new SnapshotLoaderViewModel(this);
            LinkLoader = new LinksProcessor();

            SetOperation(new ProcessStatus("Ожидание ссылок...", 0));

            CloseSnapCommand = new RelayCommand(OnCloseSnapshotCommandExecuted, obj => !SnapshotIsNull);
        }

        public ICommand CloseSnapCommand { get; private set; }

        //Открытый снапшот
        public SnapshotView SnapshotView
        {
            get => snapshotView;
            private set => Set(ref snapshotView, value);
        }
        private SnapshotView snapshotView;

        public HtmlLoaderViewModel LoadHtmlView { get; private set; }

        private bool SnapshotIsNull => SnapshotView.CurrentSnapshot.IsEmpty;
        public void SetSnapshot(Snapshot value)
        {
            var oldSnapshot = snapshotView;
            if(snapshotView != null)
            {
                snapshotView.ViewOptions.OnUpdated -= UpdatePagerLinks;
            }

            SnapshotView = new SnapshotView(value);
            if (value != null)
            {
                LoadHtmlView.SetSnapshot(SnapshotView);
                SnapshotView.ViewOptions.OnUpdated += UpdatePagerLinks;
                UpdatePagerLinks();
            }
        }
        public void SetNullSnapshot()
        {
            SetSnapshot(Snapshot.GetEmptySnapshot());
        }

        public void SetOperation(ProcessStatus status)
        {
            Operation = status;
        }

        private void OnCloseSnapshotCommandExecuted(object obj)
        {
            SetNullSnapshot();
            LinksPager = null;
            SetOperation(new ProcessStatus("Снапшот закрыт", 0));
        }


        //Список отображаемых ссылок
        public IPager<ArchiveLink> LinksPager
        {
            get => linksPager;
            private set => Set(ref linksPager, value);
        }
        private IPager<ArchiveLink> linksPager;
        public void UpdatePagerLinks()
        {
            if(SnapshotIsNull)
            {
                return;
            }

            var options = SnapshotView.ViewOptions;
            var filteredLinks = options.GetFilteredLinks();
            filteredLinks = options.ListView.SortLinks(filteredLinks);
            options.LinksFilteredAmount = filteredLinks.Count();

            LinksPager = new Pager<ArchiveLink>(filteredLinks, options.ListView.GroupSelected, LinksPager);
        }
    }
}
