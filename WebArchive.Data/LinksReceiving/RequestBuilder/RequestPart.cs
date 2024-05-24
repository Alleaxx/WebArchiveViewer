using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchive.Data.RequestParts
{
    public class RequestPart : NotifyObject
    {
        public override string ToString()
        {
            return $"Часть запроса: {Name}";
        }

        protected virtual string PrefixChar => Inverted ? "&!" : "&";
        public string Prefix { get; protected set; }
        public virtual string Value { get; set; }


        public string Name { get; protected set; }
        public string Description { get; protected set; }

        public bool Inverted
        {
            get => inverted;
            set => Set(ref inverted, value);
        }
        private bool inverted;
        public bool Enabled
        {
            get => !Inverted;
            set
            {
                Set(ref inverted, !value);
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
        public RequestPart(string name)
        {
            Name = name;
        }
    }
}
