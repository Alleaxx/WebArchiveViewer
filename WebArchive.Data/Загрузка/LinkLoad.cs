using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace WebArchive.Data
{
    public interface ILinkLoad
    {
        bool Success { get; }
        void Process();
        ILinkLoad SetFail();
        ILinkLoad SetResult(string name, string html);
    }
    public class LinkLoad : ILinkLoad
    {
        private readonly LoadHtmlOptions Options;
        private readonly ArchiveLink Link;


        public string Name { get; private set; }
        public string Content { get; private set; }
        public bool Success { get; private set; }


        public LinkLoad(ArchiveLink link, LoadHtmlOptions options)
        {
            Link = link;
            Options = options;
            Success = false;
        }
        public ILinkLoad SetResult(string name, string content)
        {
            Success = true;
            Name = name;
            Content = content;
            return this;
        }
        public ILinkLoad SetFail()
        {
            return this;
        }

        public void Process()
        {
            if (Options.LoadingTitle)
            {
                Link.Name = Name;
            }
            if (Options.SavingHtml)
            {
                string fileName = CreateSaveFileName(Link);
                string filePath = $"{Options.FolderPath}\\{fileName}.html";

                File.WriteAllText(filePath, Content);
                Link.HtmlFilePath = filePath;
            }
        }

        //Формирование имени сохраняемого файла
        public static string CreateSaveFileName(ArchiveLink link)
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
