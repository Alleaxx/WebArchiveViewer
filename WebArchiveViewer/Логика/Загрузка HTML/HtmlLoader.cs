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
        public override string ToString() => $"Загрузчик HTML: {State} - {Link.Link}";

        public IArchLink Link { get; private set; }

        public string Status { get; private set; }
        public LoadState State { get; private set; }
        protected void SetState(string status, LoadState state)
        {
            Status = status;
            State = state;
        }


        public LoadHtmlOptions Options { get; private set; }


        public HTMLoader(IArchLink link, LoadHtmlOptions options)
        {
            Link = link;
            Options = options;
        }
        public abstract Task<bool> LoadHtml();
        protected static string GetSaveFileName(IArchLink link)
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
    public class LoadHtmlOptions
    {
        public bool LoadingName { get; set; } = true;
        public bool SavingHtml { get; set; } = true;
        public string FolderPath { get; set; }

        public LoadHtmlOptions(string folder)
        {
            FolderPath = folder;
        }
    }
}
