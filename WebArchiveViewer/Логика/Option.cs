using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchiveViewer
{
    class Option<T> : NotifyObj
    {
        public T Value { get; set; }

        public bool Enabled
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

        }
        public Option(T val, bool enabled = true)
        {
            Value = val;
            Enabled = enabled;
        }
    }

    class OptionCount<T> : Option<T>
    {
        public int Amount { get; set; }
        public OptionCount(T val, bool enabled = true) : base(val, enabled)
        {
            Amount = 1;
        }
    }

}
