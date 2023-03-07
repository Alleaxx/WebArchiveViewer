using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchive.Data
{
    //Построитель запроса
    public interface IRequestBuilder
    {
        string GetRequest();
        string Request { get; }
    }
}
