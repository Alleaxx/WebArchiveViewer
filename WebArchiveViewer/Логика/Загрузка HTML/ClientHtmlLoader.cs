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
        public ClientHTMLoader(string writefolder, IArchLink link) : base(writefolder, link) { }
        public async override Task<bool> LoadHtml()
        {
            SetState("Ожидание соединения...", LoadState.Waiting);
            WebClient client = new WebClient();
            try
            {
                string html = client.DownloadString(Link.Link);

                SetState("Обработка текста...", LoadState.Loading);
                if (LoadPageName)
                {
                    Link.Name = ArchiveLink.GetTitleFromHtml(html);
                }


                if (SaveFile)
                {
                    SetState("Запись в файл...", LoadState.Loading);
                    string fileName = GetSaveName(Link);
                    string filePath = $"{FolderWrite}\\{fileName}.html";

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
