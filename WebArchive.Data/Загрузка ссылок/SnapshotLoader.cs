using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchive.Data.Loaders
{
    public interface ISnapshotLoader
    {
        event Action<SnapshotLoaderEventArgs> OnStatusChanged;

        Task StartLoadProcess();
        Task BreakLoadProcess();
        Task<Snapshot> GetSnapshotAsync();
    }
}
