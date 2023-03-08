using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WebArchive.Data.Loaders
{
    public class SnapshotRequestLoader : ISnapshotLoader
    {
        private readonly string RequestString;
        private readonly HttpClient HttpClient;
        public SnapshotRequestLoader(string request, HttpClient httpClient = null)
        {
            RequestString = request;
            HttpClient = httpClient;
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
            OnStatusChanged?.Invoke(SnapshotLoaderEventArgs.Ok(this, "Загрузка снапшота с архива", 10));

            var client = HttpClient ?? new HttpClient();
            try
            {
                string responceText = await client.GetStringAsync(RequestString);

                OnStatusChanged?.Invoke(SnapshotLoaderEventArgs.Ok(this, "Обработка данных снапшота", 80));
                var snapshot = await Task.Run(() => CreateSnapshotFromJson(responceText));

                OnStatusChanged?.Invoke(SnapshotLoaderEventArgs.Ok(this, "Загрузка снапшота с архива завершена", 100));
                return snapshot;
            }
            catch (WebException ex)
            {
                OnStatusChanged?.Invoke(SnapshotLoaderEventArgs.FinishedWithError(this, $"Ошибка соединения с архивом", ex));
                return Snapshot.GetEmptySnapshot();
            }
            catch (Exception ex)
            {
                OnStatusChanged?.Invoke(SnapshotLoaderEventArgs.FinishedWithError(this, $"Ошибка получения архива", ex));
                return Snapshot.GetEmptySnapshot();
            }
            finally
            {
                if (HttpClient == null)
                {
                    client.Dispose();
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
