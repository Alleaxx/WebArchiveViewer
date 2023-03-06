using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchiveViewer
{
    public interface IType
    {
        string Type { get; }
        bool Enabled { get; set; }
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
