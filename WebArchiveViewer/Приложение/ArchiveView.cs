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
    public class ArchiveView : NotifyObj
    {
        public override string ToString()
        {
            return $"Просмотр снапшота - {snapshotView}";
        }

        public ArchiveView()
        {
            SetSnapshot(null);
            Receiver = new SnapshotReceiver(this);
        }
        protected override void InitCommands()
        {
            base.InitCommands();
            CloseSnapCommand = new RelayCommand(CloseSnapshot, IsSnapshotOpened);
        }



        //Получение снапшота
        public SnapshotReceiver Receiver { get; private set; }


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
        private SnapshotView snapshotView;

        public void SetSnapshot(Snapshot value)
        {
            if(snapshotView != null)
            {
                snapshotView.ViewOptions.OnUpdated -= UpdateShowedLinks;
            }

            SnapshotView = new SnapshotView(value);
            if (value != null)
            {
                SnapshotView.ViewOptions.OnUpdated += UpdateShowedLinks;
                UpdateShowedLinks();
            }
        }


        private bool IsSnapshotOpened(object obj)
        {
            return SnapshotView.Current != null;
        }
        public ICommand CloseSnapCommand { get; private set; }
        private void CloseSnapshot(object obj)
        {
            SetSnapshot(null);
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
        public void UpdateShowedLinks()
        {
            if(SnapshotView.Current != null)
            {
                var options = SnapshotView.ViewOptions;
                var filteredLinks = options.GetFilteredLinks();
                filteredLinks = options.ListView.SortLinks(filteredLinks);
                options.LinksFilteredAmount = filteredLinks.Count();

                LinksPager = new Pager<ArchiveLink>(filteredLinks, options.ListView.GroupSelected, LinksPager);
            }
        }
    }

}
