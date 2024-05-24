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
}
