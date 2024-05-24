using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace WebArchive.Data.Loaders
{
    public class SnapshotRequestLoader : ISnapshotLoader
    {
        public event Action<SnapshotLoaderEventArgs> OnStatusChanged;

        private readonly string RequestString;
        private readonly HttpClient HttpClient;

        public SnapshotRequestLoader(string request, HttpClient httpClient = null)
        {
            RequestString = request;
            HttpClient = httpClient;
        }


        public async Task StartLoadingProcessAsync(CancellationToken cancellationToken)
        {
            await GetSnapshotAsync(cancellationToken);
        }
        public Task BreakLoadingProcessAsync()
        {
            throw new NotImplementedException();
        }
        public async Task<Snapshot> GetSnapshotAsync(CancellationToken cancellationToken)
        {
            var client = HttpClient ?? new HttpClient();
            try
            {
                OnStatusChanged?.Invoke(SnapshotLoaderEventArgs.Ok(this, "Отправлен запрос к архиву", 10));
                var responce = await client.GetAsync(RequestString, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                if (!responce.IsSuccessStatusCode)
                {
                    OnStatusChanged?.Invoke(SnapshotLoaderEventArgs.FinishedWithError(this, $"Ошибка соединения с архивом", new Exception($"Статус ответа: {responce.StatusCode}")));
                    return Snapshot.GetEmptySnapshot();
                }


                StringBuilder sb = new StringBuilder();
                try
                {
                    using (var stream = await responce.Content.ReadAsStreamAsync())
                    {
                        OnStatusChanged?.Invoke(SnapshotLoaderEventArgs.Ok(this, $"Ответ положительный, чтение контента", 30));
                        using (var reader = new StreamReader(stream))
                        {
                            int lines = 0;
                            while (!reader.EndOfStream)
                            {
                                var line = await reader.ReadLineAsync();
                                sb.AppendLine(line);
                                lines++;
                                
                                //уведомление о продолжающейся загрузке
                                if(lines % 5000 == 0)
                                {
                                    OnStatusChanged?.Invoke(SnapshotLoaderEventArgs.Ok(this, $"Прочитано {lines} строк ответа...", 30));
                                }

                                if (cancellationToken.IsCancellationRequested)
                                {
                                    OnStatusChanged?.Invoke(SnapshotLoaderEventArgs.FinishedSuccessfuly(this, $"Запрошено завершение чтения"));
                                    return Snapshot.GetEmptySnapshot();
                                }
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    OnStatusChanged?.Invoke(SnapshotLoaderEventArgs.FinishedWithError(this, $"Ошибка чтения потока", ex));
                    return Snapshot.GetEmptySnapshot();
                }

                string responceText = sb.ToString();

                OnStatusChanged?.Invoke(SnapshotLoaderEventArgs.Ok(this, "Обработка полученных данных", 80));
                var snapshot = CreateSnapshotFromJson(responceText);

                OnStatusChanged?.Invoke(SnapshotLoaderEventArgs.FinishedSuccessfuly(this, "Загрузка снапшота с архива успешно завершена"));
                return snapshot;
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


        /// <summary>
        /// Создание снапшота из полученных данных
        /// </summary>
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
            newSnap.Posprocessing();
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
