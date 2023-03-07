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
    public class StatusCode : Option, ICode
    {
        public string Code { get; private set; }
        public StatusCode(string code)
        {
            Code = code;
        }
    }
}
