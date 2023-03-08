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
        public event Action<LinkProcessingEventArgs> OnStatusChanged;
        public event Action<LinkProcessingEventArgs> OnEnded;

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
            OnStatusChanged?.Invoke(new LinkProcessingEventArgs(this, "Обработка ссылки начинается"));
            if(!Configuration.SavingHtml && !Configuration.LoadingTitle)
            {
                var endEventArgs = new LinkProcessingEventArgs(this, "Конец обработки ссылки: обработка не указана");
                OnStatusChanged?.Invoke(endEventArgs);
                OnEnded?.Invoke(endEventArgs);
                return;
            }


            string htmlContent = "";
            try
            {
                htmlContent = await LinkProcessingHelper.GetHtmlAsync(Link.Link, HttpClient, CancellationToken);
            }
            catch (Exception ex)
            {
                OnEnded?.Invoke(new LinkProcessingEventArgs(this, "Обработка ссылки завершена с ошибкой", true));
                return;
            }
            OnStatusChanged?.Invoke(new LinkProcessingEventArgs(this, "Получен html-контент", pageContent: htmlContent));


            if (Configuration.LoadingTitle)
            {
                Link.Name = LinkProcessingHelper.GetNameFromHtmlAsync(htmlContent);
                OnStatusChanged?.Invoke(new LinkProcessingEventArgs(this, $"Загружено имя страницы", pageName:Link.Name));
            }
            if (Configuration.SavingHtml && !string.IsNullOrEmpty(Configuration.FolderPath))
            {
                try
                {
                    file = LinkProcessingHelper.SaveToFileAsync(Link, Configuration.FolderPath, htmlContent);
                    OnStatusChanged?.Invoke(new LinkProcessingEventArgs(this, $"Содержимое страницы сохранено в файл", fileInfo: file));
                }
                catch (Exception ex)
                {
                    OnStatusChanged?.Invoke(new LinkProcessingEventArgs(this, "Обработка ссылки завершена с ошибкой", true));
                }
            }

            var lastEventArgs = new LinkProcessingEventArgs(this, "Обработка ссылки завершена", true, Link.Name, htmlContent, file);
            OnStatusChanged?.Invoke(lastEventArgs);
            OnEnded?.Invoke(lastEventArgs);
        }
    }
}
