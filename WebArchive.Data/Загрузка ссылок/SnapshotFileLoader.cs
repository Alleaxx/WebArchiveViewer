using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebArchive.Data.Loaders
{
    /// <summary>
    /// Загрузчик снапшота ссылок архива из сохраненного файла
    /// </summary>
    public class SnapshotFileLoader : ISnapshotLoader
    {
        public event Action<SnapshotLoaderEventArgs> OnStatusChanged;

        private readonly string FilePath;
        public SnapshotFileLoader(string path)
        {
            FilePath = path;
        }


        public async Task StartLoadProcess(CancellationToken cancellationToken)
        {
            await GetSnapshotAsync(cancellationToken);
        }
        public Task BreakLoadProcess()
        {
            throw new NotImplementedException();
        }
        public async Task<Snapshot> GetSnapshotAsync(CancellationToken cancellationToken)
        {
            var helper = new FileHelper();

            try
            {
                OnStatusChanged?.Invoke(SnapshotLoaderEventArgs.Ok(this, "Открытие файла со снапшотом", 3));
                var snapshot = helper.OpenReadJson<Snapshot>(FilePath);
                OnStatusChanged?.Invoke(SnapshotLoaderEventArgs.Ok(this, "Файл прочитан, идёт обработка", 80));
                snapshot.FilePath = FilePath;
                snapshot.ClearNonExistantFilePathes();
                OnStatusChanged?.Invoke(SnapshotLoaderEventArgs.FinishedSuccessfuly(this, "Снапшот успешно загружен из файла").SetResult(snapshot));
                return snapshot;
            }
            catch (Exception ex)
            {
                var empty = Snapshot.GetEmptySnapshot();
                OnStatusChanged?.Invoke(SnapshotLoaderEventArgs.FinishedWithError(this, $"При загрузке снапшота возникли ошибки", ex).SetResult(empty));
                return empty;
            }
        }
    }
}
