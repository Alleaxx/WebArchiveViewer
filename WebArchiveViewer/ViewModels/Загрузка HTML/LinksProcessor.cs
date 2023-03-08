using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WebArchive.Data;
using WebArchive.Data.HtmlLoading;
using WebArchiveViewer.Services;

namespace WebArchiveViewer
{
    //Контрол на загрузку ссылок
    public class LinksProcessor
    {
        public LinksProcessor()
        {
            LoadingLinksList = new List<ArchiveLink>();
            LoadLinkNameCommand = new RelayCommand(LoadNameAsync, IsLoadingLinkNameAvailable);
        }

        //Загрузка имени страницы
        private readonly List<ArchiveLink> LoadingLinksList;
        private readonly string[] ForbiddenCodes = new string[] { "404", "502", "302" };

        public ICommand LoadLinkNameCommand { get; private set; }
        private bool IsLoadingLinkNameAvailable(object obj)
        {
            return obj is ArchiveLink Link && !ForbiddenCodes.Contains(Link.StatusCode) && Link.MimeType == "text/html" && !LoadingLinksList.Contains(Link);
        }
        private async void LoadNameAsync(object obj)
        {
            if (obj is ArchiveLink Link)
            {
                LoadingLinksList.Add(Link);
                Link.Name = await LinkProcessingHelper.GetNameFromURIAsync(Link.Link, HttpService.GetHttpClient(), default);
                LoadingLinksList.Remove(Link);
            }
        }
    }
}
