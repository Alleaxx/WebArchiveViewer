using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchive.Data
{
    public class Option : NotifyObject
    {
        public virtual bool Enabled
        {
            get => enabled;
            set => Set(ref enabled, value);
        }
        private bool enabled = true;
    }
}
