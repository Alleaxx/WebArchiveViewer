using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WebArchiveViewer.Services
{
    public static class HttpService
    {
        private static readonly HttpClient AppClient;

        static HttpService()
        {
            AppClient = new HttpClient();
            AppClient.Timeout = TimeSpan.FromMinutes(10);
        }

        public static HttpClient GetHttpClient()
        {
            return AppClient;
        }
    }
}
