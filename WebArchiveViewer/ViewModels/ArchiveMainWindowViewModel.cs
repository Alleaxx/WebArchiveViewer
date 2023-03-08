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
    public class ArchiveMainWindowViewModel : NotifyObject
    {
        //Получение снапшота
        public SnapshotLoaderViewModel SnapshotLoader { get; private set; }

        public ArchiveMainWindowViewModel()
        {
            SetNullSnapshot();
            SnapshotLoader = new SnapshotLoaderViewModel(this);


            CloseSnapCommand = new RelayCommand(OnCloseSnapshotCommandExecuted, obj => !SnapshotIsNull);
            OpenLoadHtmlWindowCommand = new RelayCommand(OnOpenLoadHtmlWindowCommandExecuted, obj => !SnapshotIsNull);
        }

        public ICommand OpenLoadHtmlWindowCommand { get; private set; }
        public ICommand CloseSnapCommand { get; private set; }

        //Открытый снапшот
        public SnapshotView SnapshotView
        {
            get => snapshotView;
            private set => Set(ref snapshotView, value);
        }
        private SnapshotView snapshotView;

        private bool SnapshotIsNull => SnapshotView.CurrentSnapshot == null;
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
                SnapshotView.ViewOptions.OnUpdated += UpdatePagerLinks;
                UpdatePagerLinks();
            }
        }
        public void SetNullSnapshot()
        {
            SetSnapshot(null);
        }

        private void OnCloseSnapshotCommandExecuted(object obj)
        {
            SetNullSnapshot();
            LinksPager = null;
        }
        private void OnOpenLoadHtmlWindowCommandExecuted(object obj)
        {
            LoadHtmlWindowViewModel saveHTMLView = new LoadHtmlWindowViewModel(SnapshotView);
            SaveHTMLWindow w = new SaveHTMLWindow(saveHTMLView);
            w.Show();
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
