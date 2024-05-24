using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchive.Data.Loaders
{
    /// <summary>
    /// Информация о состоянии загрузки снапшота
    /// </summary>
    public class SnapshotLoaderEventArgs : EventArgs
    {
        public ISnapshotLoader Loader { get; private set; }
        public Operation State { get; private set; }
        public Snapshot Result { get; private set; }


        private SnapshotLoaderEventArgs(ISnapshotLoader loader, Operation state)
        {
            Loader = loader;
            State = state;
        }
        public SnapshotLoaderEventArgs SetResult(Snapshot snapshot)
        {
            this.Result = snapshot;
            return this;
        }


        public static SnapshotLoaderEventArgs Ok(ISnapshotLoader loader, string message, int ready)
        {
            return new SnapshotLoaderEventArgs(loader, new Operation(message, ready, true));
        }
        public static SnapshotLoaderEventArgs Error(ISnapshotLoader loader, string message, int ready, Exception ex)
        {
            return new SnapshotLoaderEventArgs(loader, new Operation(message, ready, false, false, ex));
        }
        
        public static SnapshotLoaderEventArgs FinishedSuccessfuly(ISnapshotLoader loader, string message)
        {
            return new SnapshotLoaderEventArgs(loader, new Operation(message, 100, true, true));
        }
        public static SnapshotLoaderEventArgs FinishedWithError(ISnapshotLoader loader, string message, Exception ex)
        {
            return new SnapshotLoaderEventArgs(loader, new Operation(message, 100, false, true, ex));
        }
    }
}
