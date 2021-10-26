using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchiveViewer
{
    public class LoadHtmlOptions
    {
        public bool LoadingTitle { get; set; } = true;
        public bool SavingHtml { get; set; } = true;
        public string FolderPath { get; private set; }

        public LoadHtmlOptions(bool loadName, bool loadHtml, string path)
        {
            LoadingTitle = loadName;
            SavingHtml = loadHtml;
            FolderPath = path;
        }
        public LoadHtmlOptions()
        {
            LoadingTitle = true;
            SavingHtml = false;
            FolderPath = null;
        }
    }
}
