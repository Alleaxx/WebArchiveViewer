using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WebArchiveViewer
{
    //Получение ссылок
    public enum LoadState { Waiting, Loading, Success, WebFail, FileFail }
    public abstract class HTMLoader
    {
        public override string ToString() => $"Загрузчик HTML-разметки - {Link.Link}";

        public IArchLink Link { get; private set; }

        public string Status { get; private set; }
        public LoadState State { get; private set; }
        protected void SetState(string status, LoadState state)
        {
            Status = status;
            State = state;
        }

        protected bool LoadPageName { get; set; } = true;
        protected string PageName { get; set; }
        protected bool SaveFile { get; set; } = true;
        protected string FolderWrite { get; private set; }


        public HTMLoader(string writefolder, IArchLink link)
        {
            FolderWrite = writefolder;
            Link = link;
        }
        public abstract Task<bool> LoadHtml();
        protected static string GetSaveName(IArchLink link)
        {
            string name = link.Name == ArchiveLink.DefaultName ? $"{link.TimeStamp} - {link.Index}" : $"{link.TimeStamp} - {link.Index} - {link.Name}";
            foreach (var invalidChar in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(invalidChar, '_');
            }
            return name;
        }
    }



    //class ResponceHTMLoader : HTMLoader
    //{
    //    public override string ToString() => $"Загрузчик HTML-разметки - {Link.Link}";
    //    public ResponceHTMLoader(string writefolder, IArchLink link) : base(writefolder, link) { }


    //    public async override Task<bool> LoadHtml()
    //    {
    //        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Link.Link);
    //        try
    //        {
    //            SetState("Ожидание соединения...", LoadState.Waiting);
    //            HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();
    //            using (Stream responceStream = response.GetResponseStream())
    //            {
    //                SetState("Соединение установлено...", LoadState.Loading);
    //                using (StreamReader reader = new StreamReader(responceStream, Encoding.GetEncoding(1251)))
    //                {
    //                    string html = reader.ReadToEnd();
    //                    Link.Name = ArchiveLink.GetTitleFromHtml(html);

    //                    string name = GetSaveName(Link);
    //                    string filePath = $"{FolderWrite}\\{name}.html";
    //                    using (FileStream fstream = new FileStream(filePath, FileMode.OpenOrCreate))
    //                    {
    //                        byte[] htmlByte = Encoding.Default.GetBytes(html);
    //                        fstream.Write(htmlByte, 0, htmlByte.Length);
    //                        Link.HtmlFilePath = filePath;
    //                        SetState($"Записан в \"{name}\"", LoadState.Success);
    //                    }
    //                }
    //            }
    //        }
    //        catch (WebException ex)
    //        {
    //            Link.HtmlFilePath = null;
    //            SetState($"Сетевая ошибка - {ex.Message}", LoadState.WebFail);
    //            return false;
    //        }
    //        catch (IOException ex)
    //        {
    //            SetState($"Файловая ошибка - {ex.Message}", LoadState.FileFail);
    //            return false;
    //        }
    //        return true;
    //    }
    //}

}
