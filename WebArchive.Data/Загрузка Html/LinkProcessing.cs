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
            OnStatusChanged?.Invoke(LinkProcessingEventArgs.Ok(this, "Обработка ссылки начинается", 3));
            if(!Configuration.SavingHtml && !Configuration.LoadingTitle)
            {
                OnStatusChanged?.Invoke(LinkProcessingEventArgs.EndedSuccessfuly(this, "Конец обработки ссылки: обработка не указана"));
                return;
            }


            string htmlContent = "";
            try
            {
                htmlContent = await LinkProcessingHelper.GetHtmlAsync(Link.Link, HttpClient, CancellationToken);
            }
            catch (Exception ex)
            {
                OnStatusChanged?.Invoke(LinkProcessingEventArgs.EndedWithErrors(this, "Обработка ссылки завершена с ошибкой", ex));
                return;
            }
            OnStatusChanged?.Invoke(LinkProcessingEventArgs.Ok(this, "Получен html-контент", 50).SetResult(pageContent: htmlContent));


            if (Configuration.LoadingTitle)
            {
                Link.Name = LinkProcessingHelper.GetNameFromHtmlAsync(htmlContent);
                OnStatusChanged?.Invoke(LinkProcessingEventArgs.Ok(this, $"Загружено имя страницы", 65).SetResult(pageName:Link.Name));
            }
            if (Configuration.SavingHtml && !string.IsNullOrEmpty(Configuration.FolderPath))
            {
                try
                {
                    file = LinkProcessingHelper.SaveToFileAsync(Link, Configuration.FolderPath, htmlContent);
                    OnStatusChanged?.Invoke(LinkProcessingEventArgs.Ok(this, $"Содержимое страницы сохранено в файл", 100).SetResult(fileInfo: file));
                }
                catch (Exception ex)
                {
                    OnStatusChanged?.Invoke(LinkProcessingEventArgs.Error(this, "Обработка ссылки завершена с ошибкой", 100, ex));
                }
            }

            OnStatusChanged?.Invoke(LinkProcessingEventArgs.EndedSuccessfuly(this, "Обработка ссылки завершена").SetResult(Link.Name, htmlContent, file));
        }
    }
}
