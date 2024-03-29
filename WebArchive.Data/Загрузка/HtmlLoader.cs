﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace WebArchive.Data
{
    //Получение ссылок
    public interface IHtmlLoader
    {
        Task<ILinkLoad> LoadHtmlAsync();
        Task<ILinkLoad> LoadHtmlAsync(CancellationToken token);
    }
    public abstract class HTMLoader : IHtmlLoader
    {
        public override string ToString()
        {
            return $"Загрузчик HTML: {State} - {Link.Link}";
        }

        //Опции загрузки
        public ILink Link { get; private set; }
        public LoadHtmlOptions Options { get; private set; }


        //Состояние загрузки
        public string Status { get; private set; }
        public LoadState State { get; private set; }
        protected void SetState(string status, LoadState state)
        {
            Status = status;
            State = state;
        }


        public HTMLoader(ILink link, LoadHtmlOptions options)
        {
            Link = link;
            Options = options;
        }

        //Метод загрузки
        protected virtual ILinkLoad LoadHtml()
        {
            return new LinkLoad(Link, Options).SetFail();
        }
        public virtual async Task<ILinkLoad> LoadHtmlAsync()
        {
            return await Task.Run(() => LoadHtml());
        }
        public virtual async Task<ILinkLoad> LoadHtmlAsync(CancellationToken token)
        {
            return await Task.Run(() => LoadHtml());
        }
    }
}
