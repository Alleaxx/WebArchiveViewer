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
        string Name { get; }
        string Content { get; }

        bool Success { get; }
        void Process();
        ILinkLoad SetFail();
        ILinkLoad SetResult(string name, string html);
    }
    public class LinkLoad : ILinkLoad
    {
        private readonly LoadHtmlOptions Options;
        private readonly ILink Link;


        public string Name { get; private set; }
        public string Content { get; private set; }
        public bool Success { get; private set; }


        public LinkLoad(ILink link, LoadHtmlOptions options)
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
            if (Options.SavingHtml && !string.IsNullOrEmpty(Options.FolderPath))
            {
                string fileName = CreateSaveFileName(Link as ArchiveLink);
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
