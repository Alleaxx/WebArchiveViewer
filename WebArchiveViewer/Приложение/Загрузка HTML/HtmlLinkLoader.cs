using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WebArchive.Data;

namespace WebArchiveViewer
{
    public class HtmlLinkLoader
    {
        public HtmlLinkLoader()
        {
            LoadLinkNameCommand = new RelayCommand(LoadNameAsync, IsLoadingLinkNameAvailable);
        }
        public ICommand LoadLinkNameCommand { get; private set; }

        //Загрузка имени страницы
        private List<ArchiveLink> LoadingLinksList { get; set; } = new List<ArchiveLink>();
        private readonly string[] ForbiddenCodes = new string[] { "404", "502", "302" };
        private bool IsLoadingLinkNameAvailable(object obj)
        {
            return obj is ArchiveLink Link && !ForbiddenCodes.Contains(Link.StatusCode) && Link.MimeType == "text/html" && !LoadingLinksList.Contains(Link);
        }

        private async void LoadNameAsync(object obj)
        {
            if (obj is ArchiveLink Link)
            {
                LoadingLinksList.Add(Link);
                using (System.Net.Http.HttpClient client = new System.Net.Http.HttpClient())
                {
                    IHtmlLoader loader = new HttpClientHTMLoader(client, Link, new LoadHtmlOptions());
                    ILinkLoad res = await loader.LoadHtmlAsync();
                    await Task.Run(res.Process);
                }
                LoadingLinksList.Remove(Link);
            }
        }
    }
}
