using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchive.Data.Loaders
{
    public class SnapshotFileLoader : ISnapshotLoader
    {
        public string FilePath { get; private set; }
        public Snapshot Snapshot { get; private set; }

        public SnapshotFileLoader(string path)
        {
            FilePath = path;
        }

        public event Action<SnapshotLoaderEventArgs> OnStatusChanged;

        public async Task StartLoadProcess()
        {
            await GetSnapshotAsync();
        }
        public Task BreakLoadProcess()
        {
            throw new NotImplementedException();
        }
        public async Task<Snapshot> GetSnapshotAsync()
        {
            var helper = new FileHelper();
            Snapshot = helper.OpenReadJson<Snapshot>(FilePath);
            Snapshot.FilePath = FilePath;
            Snapshot.InitAfterLoad();
            return Snapshot;
        }
    }
}
