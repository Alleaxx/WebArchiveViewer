using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchive.Data
{
    //Запрос из заданной строки
    public class DefaultRequestBuilder : NotifyObject, IRequestBuilder
    {
        public string GetRequest()
        {
            return Request;
        }

        public string Request
        {
            get => request;
            set => Set(ref request, value);
        }
        private string request;

        public DefaultRequestBuilder(string request)
        {
            Request = request;
        }
    }
}
