using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WebArchive.Data;
namespace WebArchiveViewer
{
    public class Option : NotifyObject
    {
        public virtual bool Enabled
        {
            get => enabled;
            set
            {
                enabled = value;
                OnPropertyChanged();
            }
        }
        private bool enabled;

        public Option()
        {
            enabled = true;
        }
    }
}
