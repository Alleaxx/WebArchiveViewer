using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchiveViewer
{
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
