using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchiveViewer
{

    public interface IRequestPart
    {
        string Name { get; }
        string Value { get; set; }
        string RequestString { get; }
        bool Inverted { get; set; }
    }
    public class RequestPart : NotifyObj, IRequestPart
    {
        public override string ToString() => $"Часть запроса: {Name}";

        protected virtual string PrefixChar => Inverted ? "&!" : "&";
        public string Prefix { get; protected set; }
        public virtual string Value { get; set; }


        public string Name { get; protected set; }
        public string Description { get; protected set; }

        public bool Inverted
        {
            get => inverted;
            set
            {
                inverted = value;
                OnPropertyChanged();
            }
        }
        private bool inverted;
        public bool Enabled
        {
            get => !Inverted;
            set
            {
                Inverted = !value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Inverted));
            }
        }

        public virtual string RequestString => string.IsNullOrEmpty(Value) ? "" : $"{PrefixChar}{Prefix}={Value}";

        protected RequestPart(string prefix, string name, string defaultValue)
        {
            Prefix = prefix;
            Name = name;
            Value = defaultValue;
        }
    }
}
