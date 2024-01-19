using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchiveViewer.ViewModels
{
    public class LinkProcessingPreparation
    {
        public HtmlLoaderViewModel Loader { get; private set; }

        public LinkProcessingPreparation(HtmlLoaderViewModel loader)
        {
            Loader = loader;
        }
    }
}
