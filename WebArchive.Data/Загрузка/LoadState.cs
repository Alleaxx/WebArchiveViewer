using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchive.Data
{
    public enum LoadState
    {
        Waiting,
        Loading,
        Success,
        WebFail,
        FileFail
    }
}
