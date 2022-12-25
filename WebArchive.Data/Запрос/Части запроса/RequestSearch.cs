using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchive.Data.RequestParts
{
    public class RequestSearch : RequestFilter
    {
        public RequestSearch() : base("urlkey")
        {

        }
    }
}
