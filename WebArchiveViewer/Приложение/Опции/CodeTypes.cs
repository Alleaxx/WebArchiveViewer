using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchiveViewer
{
    public interface ICode
    {
        string Code { get; }
        bool Enabled { get; set; }
    }
    public interface IType
    {
        string Type { get; }
        bool Enabled { get; set; }
    }



    public class Option : NotifyObj
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
        private bool enabled = true;
    }
    public class StatusCode : Option, ICode
    {
        public string Code { get; private set; }
        public StatusCode(string code)
        {
            Code = code;
        }
    }
    public class MimeType : Option, IType
    {
        public string Type { get; private set; }
        public MimeType(string type)
        {
            Type = type;
        }
    }




}
