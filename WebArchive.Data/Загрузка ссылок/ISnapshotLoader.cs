using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebArchive.Data.Loaders
{
    /// <summary>
    /// Общий интерфейс загрузки архивного снапшота ссылок
    /// </summary>
    public interface ISnapshotLoader
    {
        event Action<SnapshotLoaderEventArgs> OnStatusChanged;

        Task StartLoadProcess(CancellationToken cancellationToken);
        Task BreakLoadProcess();
        Task<Snapshot> GetSnapshotAsync(CancellationToken cancellationToken);
    }
}
