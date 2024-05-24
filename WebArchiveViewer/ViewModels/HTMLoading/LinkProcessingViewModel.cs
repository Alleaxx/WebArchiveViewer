using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebArchive.Data;
using WebArchive.Data.HtmlLoading;
using WebArchive.WpfUI.Helpers;
using WebArchiveViewer.Services;

namespace WebArchiveViewer.ViewModels
{
    public class LinkProcessingViewModel : NotifyObject
    {
        public ILink Link { get; private set; }

        public IList<Operation> Logs { get; private set; }

        public Operation LastProgress
        {
            get => lastProgress;
            set => Set(ref lastProgress, value);
        }
        private Operation lastProgress;

        public LinkProcessingViewModel(LinkProcessing processing)
        {
            Link = processing.Link;
            Logs = new ObservableCollection<Operation>();
            processing.OnStatusChanged += Processing_OnStatusChanged;
        }

        private async void Processing_OnStatusChanged(LinkProcessingEventArgs obj)
        {
            await DispatcherHelper.ExeInDispatcherAsync(() =>
            {
                Logs.Add(obj.State);
                LastProgress = obj.State;
            });
            if (obj.State.IsEnded)
            {
                obj.Sender.OnStatusChanged -= Processing_OnStatusChanged;
            }
        }
    }
}
