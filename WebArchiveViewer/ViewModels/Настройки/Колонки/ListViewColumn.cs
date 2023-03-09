using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchiveViewer
{
    public class ListViewColumn : WebArchive.Data.NotifyObject
    {
        public string Name { get; set; }
        public int Order { get; set; }
        public bool Hidden
        {
            get => hidden;
            set
            {
                Set(ref hidden, value);
                OnPropertyChanged(nameof(Shown));
            }
        }
        private bool hidden;

        public bool Shown
        {
            get => !hidden;
            set
            {
                Hidden = !value;
            }
        }


        public ISorting Sorting { get; set; }
        public IGrouping Grouping { get; set; }



    }
}
