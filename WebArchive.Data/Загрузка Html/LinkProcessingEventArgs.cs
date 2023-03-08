using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchive.Data.HtmlLoading
{
    /// <summary>
    /// Обновление состояния обработки ссылки
    /// </summary>
    public class LinkProcessingEventArgs : EventArgs
    {
        public LinkProcessing Sender { get; private set; }
        public ProcessStatus State { get; private set; }

        public string PageName { get; private set; }
        public string PageHtmlContent { get; private set; }
        public FileInfo PageFilePath { get; private set; }

        private LinkProcessingEventArgs(LinkProcessing sender, string message, bool successfull = true, string pageName = null, string pageContent = null, FileInfo fileInfo = null)
        {
            Sender = sender;
            State = new ProcessStatus(message, 0, successfull, false);

            PageName = pageName;
            PageHtmlContent = pageContent;
            PageFilePath = fileInfo;
        }
        private LinkProcessingEventArgs(LinkProcessing sender, ProcessStatus state, string pageName = null, string pageContent = null, FileInfo fileInfo = null)
        {
            Sender = sender;
            State = state;

            PageName = pageName;
            PageHtmlContent = pageContent;
            PageFilePath = fileInfo;
        }

        public LinkProcessingEventArgs SetResult(string pageName = null, string pageContent = null, FileInfo fileInfo = null)
        {
            PageName = pageName;
            PageHtmlContent = pageContent;
            PageFilePath = fileInfo;
            return this;
        }


        public static LinkProcessingEventArgs Ok(LinkProcessing sender, string message, int ready)
        {
            return new LinkProcessingEventArgs(sender, new ProcessStatus(message, ready, true, false));
        }
        public static LinkProcessingEventArgs Error(LinkProcessing sender, string message, int ready, Exception ex)
        {
            return new LinkProcessingEventArgs(sender, new ProcessStatus(message, ready, false, false, ex));
        }
        public static LinkProcessingEventArgs EndedSuccessfuly(LinkProcessing sender, string message)
        {
            return new LinkProcessingEventArgs(sender, new ProcessStatus(message, 100, true, true));
        }
        public static LinkProcessingEventArgs EndedWithErrors(LinkProcessing sender, string message, Exception ex)
        {
            return new LinkProcessingEventArgs(sender, new ProcessStatus(message, 100, false, true, ex));
        }
    }
}
