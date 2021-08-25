using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WebArchiveViewer
{
    class ClientHTMLoader : HTMLoader
    {
        public ClientHTMLoader(IArchLink link, LoadHtmlOptions options) : base(link, options) { }
        public async override Task<bool> LoadHtml()
        {
            SetState("Ожидание соединения...", LoadState.Waiting);
            WebClient client = new WebClient();
            try
            {
                string html = client.DownloadString(Link.Link);

                SetState("Обработка текста...", LoadState.Loading);
                if (Options.LoadingName)
                {
                    Link.Name = ArchiveLink.GetTitleFromHtml(html);
                }


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
        }
    }
}
