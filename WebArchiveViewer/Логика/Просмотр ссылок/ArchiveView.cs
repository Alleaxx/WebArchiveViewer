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

namespace WebArchiveViewer
{

    //Представление просмотра ссылок с архива
    public class ArchiveView : NotifyObj
    {
        public override string ToString() => $"Просмотр снапшота - {CurrentSnapshot}";

        //Инициализация
        public ArchiveView()
        {
            Receiver = new SnapshotReceiver(this);
        }
        protected override void InitCommands()
        {
            base.InitCommands();
            CloseSnapCommand = new RelayCommand(CloseSnapshot, IsSnapshotOpened);
        }


        //Снапшот и его получение
        public SnapshotReceiver Receiver { get; private set; }
        public SiteSnapshot CurrentSnapshot
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
                    options.OnUpdated += UpdateList;
                    UpdateList();
                }
                if (oldValue != null && oldValue.ViewOptions != null)
                {
                    oldValue.ViewOptions.OnUpdated -= UpdateList;
                }
            }
        }
        private SiteSnapshot currentSnapshot;
        private bool IsSnapshotOpened(object obj) => CurrentSnapshot != null;



        
        //Закрытие
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
                var filteredLinks = options.GetFilteredLinks();
                filteredLinks = options.ListView.SortLinks(filteredLinks);
                options.LinksFilteredAmount = filteredLinks.Count();

                LinksPager = new Pager<ArchiveLink>(filteredLinks, options.ListView.GroupSelected, LinksPager);
            }
        }
    }
}
