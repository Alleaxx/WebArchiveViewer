using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchive.Data
{
    public interface ILink
    {
        string Name { get; set; }
        string Link { get; }
        string HtmlFilePath { get; set; }
    }
    public class UsualLink : ILink
    {
        public string Name { get; set; }
        public string Link { get; set; }

        public string HtmlFilePath { get; set; }

        public UsualLink(string name, string link)
        {
            Name = name;
            Link = link;
        }
    }
}
