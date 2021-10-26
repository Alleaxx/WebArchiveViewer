using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WebArchive.Data;
namespace WebArchiveViewer
{


    public interface ISorting
    {
        string Name { get; }
        Func<ArchiveLink, string> KeySelector { get; }
    }
    public class Sorting : ISorting
    {
        public string Name { get; private set; }
        public Func<ArchiveLink, string> KeySelector { get; private set; }


        public Sorting(string name, Func<ArchiveLink, string> func, bool enabled)
        {
            Name = name;
            KeySelector = func;

        }
    }
}
