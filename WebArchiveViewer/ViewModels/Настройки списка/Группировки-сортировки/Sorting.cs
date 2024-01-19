using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WebArchive.Data;

namespace WebArchiveViewer.ViewModels
{
    public interface ISorting
    {
        SortsEnum Type { get; }
        string Name { get; }
        Func<ArchiveLink, string> KeySelector { get; }
        bool Ascending { get; }
        void ToggleOrder();
    }
    public class Sorting : NotifyObject, ISorting
    {
        public SortsEnum Type { get; private set; }
        public string Name { get; private set; }
        public Func<ArchiveLink, string> KeySelector { get; private set; }
        public bool Ascending
        {
            get => ascending;
            set => Set(ref ascending, value);
        }
        private bool ascending;

        public Sorting(SortsEnum type, string name, Func<ArchiveLink, string> func, bool enabled, bool ascending = true)
        {
            Type = type;
            Name = name;
            KeySelector = func;
            this.Ascending = ascending;
        }

        public void ToggleOrder()
        {
            Ascending = !Ascending;
        }
    }
}
