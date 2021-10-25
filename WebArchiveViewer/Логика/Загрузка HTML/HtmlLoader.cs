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
        public override string ToString()
        {
            return $"Загрузчик HTML: {State} - {Link.Link}";
        }

        //Опции загрузки
        public ArchiveLink Link { get; private set; }
        public LoadHtmlOptions Options { get; private set; }


        //Состояние загрузки
        public string Status { get; private set; }
        public LoadState State { get; private set; }
        protected void SetState(string status, LoadState state)
        {
            Status = status;
            State = state;
        }


        public HTMLoader(ArchiveLink link, LoadHtmlOptions options)
        {
            Link = link;
            Options = options;
        }

        //Метод загрузки
        protected virtual bool LoadHtml()
        {
            return false;
        }
        public virtual async Task<bool> LoadHtmlAsync()
        {
            return await Task.Run(() => LoadHtml());
        }


        //Формирование имени сохраняемого файла
        protected static string GetSaveFileName(ArchiveLink link)
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
