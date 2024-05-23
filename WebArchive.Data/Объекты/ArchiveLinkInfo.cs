using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchive.Data
{
    public class ArchiveLinkInfo
    {
        public List<ArchiveLink> AllLinks { get; set; }
        public int Count => AllLinks.Count;
        public ArchiveLink ExampleLink { get; private set; }
        public ArchiveLink FirstLink { get; private set; }
        public ArchiveLink LastLink { get; private set; }

        public ArchiveLinkInfo(ArchiveLink link)
        {
            ExampleLink = link;
            AllLinks = new List<ArchiveLink>();
            AddLink(link);
            link.IsUniq = true;
        }

        public void AddLink(ArchiveLink link)
        {
            AllLinks.Add(link);
            if(FirstLink == null || FirstLink.Date > link.Date)
            {
                FirstLink = link;
            }
            if (LastLink == null || LastLink.Date < link.Date)
            {
                LastLink = link;
            }
            link.Information = this;
        }
    }
}
