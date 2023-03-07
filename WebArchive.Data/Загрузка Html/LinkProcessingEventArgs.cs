using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchive.Data.HtmlLoading
{
    public class LinkProcessingEventArgs : EventArgs
    {
        public LinkProcessing Sender { get; set; }
        public string Message { get; set; }
        public bool Error { get; set; }


        public LinkProcessingEventArgs(LinkProcessing sender, string message, bool error = false)
        {
            Sender = sender;
            Message = message;
            Error = error;
        }
    }
}
