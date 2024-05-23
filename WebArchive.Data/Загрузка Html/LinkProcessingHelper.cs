using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebArchive.Data.HtmlLoading
{
    public static class LinkProcessingHelper
    {
        //1 - Получить HTML страницы из URL-адреса
        public static async Task<string> GetHtmlAsync(string URI, HttpClient receivedClient = null, CancellationToken token = default)
        {
            var usedClient = receivedClient ?? new HttpClient();

            var responce = await usedClient.GetAsync(URI, HttpCompletionOption.ResponseHeadersRead, token);
            if (!responce.IsSuccessStatusCode)
            {
                return null;
            }
            var encoding = GetGuessedEncoding(responce, Encoding.UTF8);

            StringBuilder html = new StringBuilder();
            using (var stream = await responce.Content.ReadAsStreamAsync())
            {
                using (var streamReader = new StreamReader(stream, encoding))
                {
                    while (!streamReader.EndOfStream)
                    {
                        var line = await streamReader.ReadLineAsync();
                        html.Append(line);
                    }
                }
            }
            if (receivedClient == null)
            {
                usedClient.Dispose();
            }
            return html.ToString();
        }

        //2 - Получить имя из HTML страницы
        public static string GetNameFromHtmlAsync(string htmlContent)
        {
            if (string.IsNullOrEmpty(htmlContent))
            {
                return null;
            }

            string name = System.Text.RegularExpressions.Regex.Match(htmlContent, @"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\</title\>",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase).Groups["Title"].Value;
            return name;
        }

        public static async Task<string> GetNameFromURIAsync(string URI, HttpClient receivedClient = null, CancellationToken token = default)
        {
            var usedClient = receivedClient ?? new HttpClient();

            var responce = await usedClient.GetAsync(URI, HttpCompletionOption.ResponseHeadersRead, token);

            if (!responce.IsSuccessStatusCode)
            {
                return null;
            }
            var encoding = GetGuessedEncoding(responce, Encoding.UTF8);

            string html = "";
            using (var stream = await responce.Content.ReadAsStreamAsync())
            {
                StringBuilder sb = new StringBuilder();
                using(var streamReader = new StreamReader(stream, encoding))
                {
                    bool foundTitleStart = false;
                    bool foundTitleEnd = false;
                    while (!(foundTitleEnd && foundTitleStart) && !streamReader.EndOfStream)
                    {
                        bool added = false;
                        var line = await streamReader.ReadLineAsync();
                        if (line.Contains("<title>"))
                        {
                            sb.Append(line);
                            foundTitleStart = true;
                            added = true;
                        }
                        if (line.Contains("</title>"))
                        {
                            foundTitleEnd = true;
                            if (!added)
                            {
                                sb.AppendLine(line);
                            }
                        }
                    }
                    bool foundAll = foundTitleStart && foundTitleEnd;
                    //Не нашли
                    if(!foundAll)
                    {
                        return null;
                    }
                    //Нашли!
                    else
                    {
                        html = sb.ToString();
                    }
                }
            }
            if(receivedClient == null)
            {
                usedClient.Dispose();
            }
            var name = GetNameFromHtmlAsync(html);
            return name;
        }

        //3 - Сохранить HTML-контент по пути с именем
        public static FileInfo SaveToFileAsync(string fileName, string folderPath, string htmlContent)
        {
            if (string.IsNullOrEmpty(htmlContent))
            {
                return null;
            }

            string filePath = $"{folderPath}\\{fileName}.html";

            File.WriteAllText(filePath, htmlContent);
            return new FileInfo(filePath);
        }

        //4 - Сохранить HTML-контент с автогенерацией имени
        public static FileInfo SaveToFileAsync(ILink Link, string folderPath, string htmlContent)
        {
            if (string.IsNullOrEmpty(htmlContent))
            {
                return null;
            }

            string fileName = CreateFileName(Link as ArchiveLink);
            string filePath = $"{folderPath}\\{fileName}.html";

            File.WriteAllText(filePath, htmlContent);
            Link.HtmlFilePath = filePath;
            return new FileInfo(filePath);
        }
        private static string CreateFileName(ArchiveLink link)
        {
            bool noName = link.Name == ArchiveLink.DefaultName;
            string withNameText = $"{link.TimeStamp} - {link.Index} - {link.Name}";
            string withoutNameText = $"{link.TimeStamp} - {link.Index}";
            return withoutNameText;
            StringBuilder nameText = noName ? new StringBuilder(withoutNameText) : new StringBuilder(withNameText);
            char[] invalidChars = Path.GetInvalidFileNameChars();
            foreach (var invChar in invalidChars)
            {
                nameText = nameText.Replace(invChar, '_');
            }
            return nameText.ToString();
        }


        private static Encoding GetGuessedEncoding(HttpResponseMessage responce, Encoding defaultEncoding)
        {
            string charset = null;
            var contentType = responce.Headers.TryGetValues("x-archive-guessed-charset", out var guessedCharsets);
            if (guessedCharsets != null && guessedCharsets.Any())
            {
                charset = guessedCharsets.First();
            }
            var encoding = Encoding.GetEncodings().FirstOrDefault(c => c.Name.Equals(charset, StringComparison.OrdinalIgnoreCase))?.GetEncoding();

            return encoding ?? defaultEncoding;
        }
    }
}
