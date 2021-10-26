using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebArchiveViewer
{
    //Проверка загрузки ссылки
    //Проверка длины загруженного HTML
    //Проверка наличия имени


    public class HttpClientHTMLoader : HTMLoader
    {
        private readonly HttpClient Client;
        private readonly CancellationToken Token;
        public HttpClientHTMLoader(HttpClient client, ArchiveLink link, LoadHtmlOptions options, CancellationToken token = default) : base(link, options)
        {
            Client = client;
            Token = token;
            if(token == default)
            {
                Token = new CancellationTokenSource().Token;
            }
        }

        public async override Task<ILinkLoad> LoadHtmlAsync()
        {
            ILinkLoad loaded = new LinkLoad(Link, Options);
            SetState("Ожидание соединения...", LoadState.Waiting);
            try
            {
                string html = await LoadHtmlFromUri();
                if (Token.IsCancellationRequested)
                    return loaded.SetFail();
                string name = Options.LoadingTitle ? LoadNameFromHtml(html) : ArchiveLink.DefaultName;

                SetState($"Успешно загружена - {name}", LoadState.Success);
                return loaded.SetResult(name, html);
            }
            catch(TaskCanceledException ex)
            {
                SetState($"Истекло время подключения или операция была отменена - {ex.Message}", LoadState.WebFail);
                return loaded.SetFail();
            }
            catch (NotSupportedException ex)
            {
                SetState($"Неподдерживаемая кодировка - {ex.Message}", LoadState.WebFail);
                return loaded.SetFail();
            }
            catch (HttpRequestException ex)
            {
                SetState($"Веб-ошибка - {ex.Message}", LoadState.WebFail);
                return loaded.SetFail();
            }
        }
        //Загрузка кода страницы
        private async Task<string> LoadHtmlFromUri()
        {
            //return await Client.GetStringAsync(Link.Link);
            HttpResponseMessage responce = await Client.GetAsync(Link.Link, Token);
            if (Token.IsCancellationRequested)
                return null;
            var buffer = await responce.Content.ReadAsByteArrayAsync();
            var byteArray = buffer.ToArray();
            string win1251 = Encoding.GetEncoding(1251).GetString(byteArray, 0, byteArray.Length);
            return win1251;
        }

        //Обработка содержимого
        private string LoadNameFromHtml(string html)
        {
            SetState("Обработка текста...", LoadState.Loading);
            return GetTitleFromHtml(html);
        }

        public static string GetTitleFromHtml(string html)
        {
            string name = System.Text.RegularExpressions.Regex.Match(html, @"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\</title\>",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase).Groups["Title"].Value;
            return name;
        }
    }
}
