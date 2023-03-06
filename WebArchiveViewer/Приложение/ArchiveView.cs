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
namespace WebArchiveViewer
{
    //Представление просмотра ссылок с архива
    public class ArchiveView : NotifyObject
    {
        public override string ToString()
        {
            return $"Представление снапшота: {snapshotView}";
        }

        public event Action<ArchiveView, SnapshotView> OnSnapshotOpened;
        public event Action<ArchiveView, SnapshotView> OnSnapshotClosed;


        private SnapshotView snapshotView;
        private IPager<ArchiveLink> linksPager;

        public ArchiveView()
        {
            SetSnapshot(null);
            Receiver = new SnapshotImporter(this);
            CloseSnapCommand = new RelayCommand(CloseSnapshot, obj => !IsEmptySnapshot);
        }

        public ICommand CloseSnapCommand { get; private set; }

        //Получение снапшота
        public SnapshotImporter Receiver { get; private set; }


        //Открытый снапшот
        public SnapshotView SnapshotView
        {
            get => snapshotView;
            private set
            {
                snapshotView = value;
                OnPropertyChanged();
            }
        }
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
                OnSnapshotOpened?.Invoke(this, SnapshotView);
            }
            else
            {
                OnSnapshotClosed?.Invoke(this, oldSnapshot);
            }
        }
        private void CloseSnapshot(object obj)
        {
            SetSnapshot(null);
            LinksPager = null;
        }
        private bool IsEmptySnapshot => SnapshotView.CurrentSnapshot == null;


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
        public void UpdatePagerLinks()
        {
            if(IsEmptySnapshot)
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
