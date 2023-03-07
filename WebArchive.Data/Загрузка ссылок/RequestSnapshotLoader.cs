using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WebArchive.Data.Loaders
{
    public class RequestSnapshotLoader : ISnapshotLoader
    {
        public readonly string RequestString;
        public Snapshot Snapshot { get; private set; }
        public RequestSnapshotLoader(string request)
        {
            RequestString = request;
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
            OnStatusChanged?.Invoke(SnapshotLoaderEventArgs.Ok("Загрузка данных с сервера...", 3));
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string responceText = await client.GetStringAsync(RequestString);

                    OnStatusChanged?.Invoke(SnapshotLoaderEventArgs.Ok("Обработка загруженных данных...", 7));
                    Snapshot = await Task.Run(() => CreateSnapshotFromJson(responceText));

                    OnStatusChanged?.Invoke(SnapshotLoaderEventArgs.Ok("Загрузка завершена...", 100));
                    return Snapshot;
                }
                catch (WebException ex)
                {
                    OnStatusChanged?.Invoke(SnapshotLoaderEventArgs.Error($"Ошибка соединения: {ex.Message}", 100));
                    return null;
                }
                catch (Exception ex)
                {
                    OnStatusChanged?.Invoke(SnapshotLoaderEventArgs.Error($"Неопределенная ошибка: {ex.Message}", 100));
                    return null;
                }
            }
        }


        //Создание снапшота из полученных данных
        private Snapshot CreateSnapshotFromJson(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return new Snapshot(RequestString, "", Array.Empty<ArchiveLink>());
            }
            List<List<string>> jsonArr = Newtonsoft.Json.JsonConvert.DeserializeObject<List<List<string>>>(json);

            ArchiveLink[] allLinks = jsonArr.Skip(1).AsParallel().Select((arr, index) =>
            {
                return CreateLinkFromJsonString(index, arr);
            }).ToArray();

            Snapshot newSnap = new Snapshot(RequestString, "", allLinks);
            return newSnap;
        }
        private ArchiveLink CreateLinkFromJsonString(int counter, List<string> stringList)
        {
            ArchiveLink newEntry = new ArchiveLink()
            {
                TimeStamp = stringList[1],
                LinkSource = stringList[2],
                MimeType = stringList[3],
                StatusCode = stringList[4],
                Index = counter
            };
            newEntry.Date = new DateTime(
                Convert.ToInt32(newEntry.TimeStamp.Substring(0, 4)),
                Convert.ToInt32(newEntry.TimeStamp.Substring(4, 2)),
                Convert.ToInt32(newEntry.TimeStamp.Substring(6, 2)),
                Convert.ToInt32(newEntry.TimeStamp.Substring(8, 2)),
                Convert.ToInt32(newEntry.TimeStamp.Substring(10, 2)),
                Convert.ToInt32(newEntry.TimeStamp.Substring(12, 2)));
            return newEntry;
        }
    }
}
