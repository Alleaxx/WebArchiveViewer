using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchive.Data.HtmlLoading
{
    public class LinkProcessingConfiguration
    {
        public bool LoadingTitle { get; set; } = true;
        public bool SavingHtml { get; set; } = true;
        public string FolderPath { get; set; }
    }
}
