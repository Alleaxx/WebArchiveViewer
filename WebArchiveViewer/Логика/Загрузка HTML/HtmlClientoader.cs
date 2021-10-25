using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WebArchiveViewer
{
    class HttpClientHTMLoader : HTMLoader
    {
        private readonly HttpClient Client;
        public HttpClientHTMLoader(HttpClient client,ArchiveLink link, LoadHtmlOptions options) : base(link, options)
        {
            Client = client;
        }

        public async override Task<bool> LoadHtmlAsync()
        {

            SetState("Ожидание соединения...", LoadState.Waiting);
            try
            {
                string html = await LoadHtmlFromUri();
                ProcessFileContent(html);
                SaveFileHtml(html);
                return true;
            }
            catch (WebException ex)
            {
                Link.HtmlFilePath = null;
                SetState($"Сетевая ошибка - {ex.Message}", LoadState.WebFail);
                return false;
            }
            catch (IOException ex)
            {
                SetState($"Файловая ошибка - {ex.Message}", LoadState.FileFail);
                return false;
            }
            catch (Exception ex)
            {
                SetState($"Ошибка - {ex.Message}", LoadState.FileFail);
                return false;
            }
        }

        //Загрузка кода страницы
        private async Task<string> LoadHtmlFromUri()
        {
            HttpResponseMessage responce = await Client.GetAsync(Link.Link);
            var buffer = await responce.Content.ReadAsByteArrayAsync();
            var byteArray = buffer.ToArray();
            return Encoding.GetEncoding(1251).GetString(byteArray, 0, byteArray.Length);
        }

        //Обработка содержимого
        private void ProcessFileContent(string html)
        {
            SetState("Обработка текста...", LoadState.Loading);
            if (Options.LoadingName)
            {
                Link.Name = LoadLink.GetTitleFromHtml(html);
            }
        }

        //Сохранение содержимого в файл
        private void SaveFileHtml(string html)
        {
            if (Options.SavingHtml)
            {
                SetState("Запись в файл...", LoadState.Loading);
                string fileName = GetSaveFileName(Link);
                string filePath = $"{Options.FolderPath}\\{fileName}.html";

                File.WriteAllText(filePath, html);
                Link.HtmlFilePath = filePath;
                SetState($"Записан в \"{fileName}\"", LoadState.Success);
            }
            else
            {
                SetState($"Загружен и прочитан", LoadState.Success);
            }

        }
    }
}
