using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchiveViewer
{
    public interface IGrouping
    {
        string Name { get; }
        string Key { get; }
    }
    public class Grouping : IGrouping
    {
        public string Name { get; private set; }
        public string Key { get; private set; }

        public Grouping(string name, string key, bool enabled)
        {
            Name = name;
            Key = key;
        }
    }
}
