using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchiveViewer.WpfUI.Collections
{
    public interface IGrouping
    {
        GroupsEnum Type { get; }
        string Name { get; }
        string Key { get; }
    }
    public class Grouping : IGrouping
    {
        public GroupsEnum Type { get; private set; }
        public string Name { get; private set; }
        public string Key { get; private set; }

        public Grouping(GroupsEnum type, string name, string key, bool enabled)
        {
            Type = type;
            Name = name;
            Key = key;
        }
    }
}
