using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WebArchive.Data;

namespace WebArchiveViewer.ViewModels
{
    public class Option : NotifyObject
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
