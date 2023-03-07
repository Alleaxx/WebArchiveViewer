using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebArchive.Data.HtmlLoading
{
    //1) Загрузка Html
    //2) Обработка Html (выделение имени)
    //3) Сохранение в файл
    //4) Завершение
    public class LinkProcessing
    {
        public event Action<LinkProcessing> OnStarted;
        public event Action<LinkProcessing, string> OnHtmlLoaded;
        public event Action<LinkProcessing, string> OnNameLoaded;
        public event Action<LinkProcessing, FileInfo> OnFileSaved;
        public event Action<LinkProcessingEventArgs> OnStatusChanged;
        public event Action<LinkProcessingEventArgs, string, string, FileInfo> OnEnded;

        public ILink Link { get; private set; }
        public int Index { get; private set; }

        private HttpClient HttpClient;
        private CancellationToken CancellationToken;
        private readonly LinkProcessingConfiguration Configuration;

        public LinkProcessing(ILink link, LinkProcessingConfiguration config)
        {
            Link = link;
            Configuration = config;
        }
        public void SetClient(HttpClient client, CancellationToken cancellationToken = default)
        {
            HttpClient = client;
            CancellationToken = cancellationToken;
        }
        public void SetIndex(int index)
        {
            Index = index;
        }
        public async Task StartProcessing()
        {
            FileInfo file = null;
            OnStarted?.Invoke(this);
            if(!Configuration.SavingHtml && !Configuration.LoadingTitle)
            {
                OnEnded?.Invoke(new LinkProcessingEventArgs(this, "Старт обработки"), null, null, null);
                return;
            }

            string htmlContent = await LinkProcessingHelper.GetHtmlAsync(Link.Link, HttpClient, CancellationToken);
            OnHtmlLoaded?.Invoke(this, htmlContent);

            if (Configuration.LoadingTitle)
            {
                Link.Name = await LinkProcessingHelper.GetNameFromHtmlAsync(htmlContent);
                OnNameLoaded?.Invoke(this, Link.Name);
            }
            if (Configuration.SavingHtml && !string.IsNullOrEmpty(Configuration.FolderPath))
            {
                file = await LinkProcessingHelper.SaveToFileAsync(Link, Configuration.FolderPath, htmlContent);
                OnFileSaved?.Invoke(this, null);
            }

            OnEnded?.Invoke(new LinkProcessingEventArgs(this, "Конец обработки"), Link.Name, htmlContent, file);
        }
    }
}
