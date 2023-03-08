using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebArchive.Data;
using WebArchive.Data.HtmlLoading;
using WebArchiveViewer.Services;

namespace WebArchiveViewer.ViewModels
{
    public class LinkProcessingViewModel : NotifyObject
    {
        public ILink Link { get; private set; }

        public IList<ProcessStatus> Logs { get; private set; }

        public ProcessStatus LastProgress
        {
            get => lastProgress;
            set => Set(ref lastProgress, value);
        }
        private ProcessStatus lastProgress;

        public LinkProcessingViewModel(LinkProcessing processing)
        {
            Link = processing.Link;
            Logs = new ObservableCollection<ProcessStatus>();
            processing.OnStatusChanged += Processing_OnStatusChanged;
        }

        private async void Processing_OnStatusChanged(LinkProcessingEventArgs obj)
        {
            await DispatcherService.ExeInDispatcherAsync(() =>
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
