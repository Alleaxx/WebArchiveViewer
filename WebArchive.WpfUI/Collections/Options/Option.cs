using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WebArchive.WpfUI;

namespace WebArchiveViewer.WpfUI.Collections
{
    public class Option : NotifyWpfObject
    {
        public virtual bool Enabled
        {
            get => enabled;
            set => Set(ref enabled, value);
        }
        private bool enabled;

        public Option()
        {
            enabled = true;
        }
    }
}
