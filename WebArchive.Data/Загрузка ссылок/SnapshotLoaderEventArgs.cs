using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchive.Data.Loaders
{
    public class SnapshotLoaderEventArgs : EventArgs
    {
        public readonly string Message;
        public readonly double ReadyPersentage;
        public readonly bool IsError;

        private SnapshotLoaderEventArgs(string message, double readyPersentage, bool error = false)
        {
            Message = message;
            ReadyPersentage = readyPersentage;
            this.IsError = error;
        }

        public static SnapshotLoaderEventArgs Ok(string message, double ready)
        {
            return new SnapshotLoaderEventArgs(message, ready, false);
        }
        public static SnapshotLoaderEventArgs Error(string message, double ready)
        {
            return new SnapshotLoaderEventArgs(message, ready, true);
        }
    }
}
