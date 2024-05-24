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
        
        private bool IsBreaked { get; set; }
        public SnapshotFileLoader(string path)
        {
            FilePath = path;
        }


        public async Task StartLoadingProcessAsync(CancellationToken cancellationToken)
        {
            await Task.Run(() => GetSnapshotAsync(cancellationToken));
        }
        public async Task BreakLoadingProcessAsync()
        {
            IsBreaked = true;
        }
        public async Task<Snapshot> GetSnapshotAsync(CancellationToken cancellationToken)
        {
            var helper = new FileHelper();

            try
            {
                OnStatusChanged?.Invoke(SnapshotLoaderEventArgs.Ok(this, "Открытие файла со снапшотом", 3));
                await Task.Delay(50);
                var snapshot = helper.OpenReadJson<Snapshot>(FilePath);
                OnStatusChanged?.Invoke(SnapshotLoaderEventArgs.Ok(this, "Файл прочитан, идёт обработка", 80));
                await Task.Delay(50);
                snapshot.FilePath = FilePath;
                snapshot.Posprocessing();
                snapshot.ClearNonExistantFilePathes();
                OnStatusChanged?.Invoke(SnapshotLoaderEventArgs.FinishedSuccessfuly(this, "Снапшот успешно загружен из файла")
                    .SetResult(snapshot));
                await Task.Delay(50);
                if (IsBreaked)
                {
                    var empty = Snapshot.GetEmptySnapshot();
                    OnStatusChanged?.Invoke(SnapshotLoaderEventArgs.FinishedSuccessfuly(this, $"Загрузка снапшота из файла была отменена")
                        .SetResult(empty));
                }
                return snapshot;
            }
            catch (Exception ex)
            {
                var empty = Snapshot.GetEmptySnapshot();
                OnStatusChanged?.Invoke(SnapshotLoaderEventArgs.FinishedWithError(this, $"При загрузке снапшота возникли ошибки", ex)
                    .SetResult(empty));
                return empty;
            }
        }
    }
}
