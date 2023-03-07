using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebArchive.Data.HtmlLoading
{
    public static class LinkProcessingHelper
    {
        //1 - Получить HTML страницы из URL-адреса
        public static async Task<string> GetHtmlAsync(string link, HttpClient HttpClient = null, CancellationToken cancellationToken = default)
        {
            var client = HttpClient ?? new HttpClient();

            HttpResponseMessage responce = await client.GetAsync(link, cancellationToken);
            if (cancellationToken.IsCancellationRequested)
            {
                return null;
            }
            if (HttpClient == null)
            {
                client.Dispose();
            }
            var buffer = await responce.Content.ReadAsByteArrayAsync();
            var byteArray = buffer.ToArray();


            string win1251 = Encoding.GetEncoding(1251).GetString(byteArray, 0, byteArray.Length);
            string utf = Encoding.UTF8.GetString(byteArray, 0, byteArray.Length);
            string shorten = utf.Substring(0, 2000);



            if (shorten.Contains("windows-1251"))
            {
                return win1251;
            }
            else
            {
                return utf;
            }
        }

        //2 - Получить имя из HTML страницы
        public static async Task<string> GetNameFromHtmlAsync(string htmlContent)
        {
            if (string.IsNullOrEmpty(htmlContent))
            {
                return null;
            }

            string name = System.Text.RegularExpressions.Regex.Match(htmlContent, @"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\</title\>",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase).Groups["Title"].Value;
            return name;
        }
        public static async Task<string> GetNameFromURIAsync(string URI, HttpClient client = null, CancellationToken token = default)
        {
            string html = await GetHtmlAsync(URI, client, token);
            string name = await GetNameFromHtmlAsync(html);
            return name;
        }

        //3 - Сохранить HTML-контент по пути с именем
        public static async Task<FileInfo> SaveToFileAsync(string fileName, string folderPath, string htmlContent)
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
        public static async Task<FileInfo> SaveToFileAsync(ILink Link, string folderPath, string htmlContent)
        {
            if (string.IsNullOrEmpty(htmlContent))
            {
                return null;
            }

            string fileName = CreateSaveFileName(Link as ArchiveLink);
            string filePath = $"{folderPath}\\{fileName}.html";

            File.WriteAllText(filePath, htmlContent);
            Link.HtmlFilePath = filePath;
            return new FileInfo(filePath);
        }
        private static string CreateSaveFileName(ArchiveLink link)
        {
            bool noName = link.Name == ArchiveLink.DefaultName;
            string withNameText = $"{link.TimeStamp} - {link.Index} - {link.Name}";
            string withoutNameText = $"{link.TimeStamp} - {link.Index}";

            StringBuilder nameText = noName ? new StringBuilder(withoutNameText) : new StringBuilder(withNameText);
            char[] invalidChars = Path.GetInvalidFileNameChars();
            foreach (var invChar in invalidChars)
            {
                nameText = nameText.Replace(invChar, '_');
            }
            return nameText.ToString();
        }
    }
}
