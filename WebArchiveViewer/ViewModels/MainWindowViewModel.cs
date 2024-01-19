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
using WebArchiveViewer.UI;

namespace WebArchiveViewer.ViewModels
{
    //Представление просмотра ссылок с архива
    public class MainWindowViewModel : NotifyObject
    {
        //Ссылки на загрузчик снапшота и html-контента
        public SnapshotLoaderViewModel SnapshotLoader { get; private set; }
        public LinksLoaderViewModel LinksLoader { get; private set; }
        public HtmlLoaderViewModel HtmlLoader { get; private set; }

        public MainWindowViewModel()
        {
            SetNullSnapshot();
            HtmlLoader = new HtmlLoaderViewModel(this);
            SnapshotLoader = new SnapshotLoaderViewModel(this);
            LinksLoader = new LinksLoaderViewModel();

            SetOperation(new ProcessStatus("Ожидание ссылок...", 0));

            CloseSnapshotCommand = new RelayCommand(OnCloseSnapshotCommandExecuted, obj => !SnapshotIsEmptyF);
        }

        /// <summary> Текущая операция, отображается в статусе </summary>
        public ProcessStatus Operation
        {
            get => operation;
            set => Set(ref operation, value);
        }
        private ProcessStatus operation;

        public int SelectedMenuIndex
        {
            get => selectedMenuIndex;
            set => Set(ref selectedMenuIndex, value);
        }
        private int selectedMenuIndex;

        public void SetOperation(ProcessStatus status)
        {
            Operation = status;
        }



        /// <summary> Модель представления текущего снапшота ссылок. Не может быть null </summary>
        public SnapshotView SnapshotView
        {
            get => snapshotView;
            private set {
                Set(ref snapshotView, value);
                OnPropertyChanged(nameof(SnapshotIsEmpty));
                OnPropertyChanged(nameof(SnapshotIsNotEmpty));
            }
        }
        private SnapshotView snapshotView;


        public ICommand CloseSnapshotCommand { get; private set; }

        public bool SnapshotIsEmpty => SnapshotView.SnapshotModel.IsEmpty;
        public bool SnapshotIsNotEmpty => !SnapshotView.SnapshotModel.IsEmpty;


        private bool SnapshotIsEmptyF => SnapshotView.SnapshotModel.IsEmpty;
        public void SetSnapshot(Snapshot value)
        {
            var oldSnapshot = snapshotView;
            if(snapshotView != null)
            {
                snapshotView.ListViewInfo.OnUpdated -= UpdatePagerLinks;
            }

            SnapshotView = new SnapshotView(value);
            if (snapshotView != null && value != null && value.IsNotEmpty)
            {
                SelectedMenuIndex = 1;
                HtmlLoader.SetSnapshot(SnapshotView);
                SnapshotView.ListViewInfo.OnUpdated += UpdatePagerLinks;
                UpdatePagerLinks();
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
            if(SnapshotIsEmptyF)
            {
                return;
            }

            var options = SnapshotView.ListViewInfo;
            var filteredLinks = options.GetFilteredLinks();
            filteredLinks = options.GroupSortsInfo.SortLinks(filteredLinks);
            options.LinksFilteredAmount = filteredLinks.Count();

            LinksPager = new Pager<ArchiveLink>(filteredLinks, options.GroupSortsInfo.GroupSelected, LinksPager);
        }
    }
}
