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
        public string Message { get; private set; }
        public bool Successfull { get; private set; }
        public bool Ended { get; private set; }
        public DateTime Date { get; private set; }

        public readonly Exception Exception;
        public readonly string PageName;
        public readonly string PageHtmlContent;
        public readonly FileInfo PageFilePath;

        public LinkProcessingEventArgs(LinkProcessing sender, string message, bool successfull = true, string pageName = null, string pageContent = null, FileInfo fileInfo = null)
        {
            Sender = sender;
            Message = message;
            Successfull = successfull;
            Date = DateTime.Now;

            PageName = pageName;
            PageHtmlContent = pageContent;
            PageFilePath = fileInfo;
        }
    }
}
