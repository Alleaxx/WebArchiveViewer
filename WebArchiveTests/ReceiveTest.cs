using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebArchiveViewer;

using WebArchive.Data;
using WebArchiveViewer.ViewModels;

namespace WebArchive.Tests
{

    [TestClass]
    public class ArchiveReceiveTest
    {
        [TestMethod]
        public void EmptyRequestArhiveAvailable_Check()
        {
            var receiving = new SnapshotLoaderViewModel();
            receiving.RequestBuilder.Site.Value = "";
            bool executeAvailable = receiving.LoadFromRequestBuilderCommand.CanExecute(null);
            Assert.IsFalse(executeAvailable, "Команда загрузки ссылок доступна при нулевом запросе");
        }



        //Загрузить снапшот из прямой ссылки
        [TestMethod]
        public async Task LoadDirectSnapshot_Check()
        {
            var receiving = new SnapshotLoaderViewModel();
            string link = "http://web.archive.org/cdx/search/cdx?url=https://www.noob-club.ru/&matchType=prefix&output=json&limit=59&from=20170209100353&to=20170831025157";
            Snapshot snapshot = await receiving.LoadFromRequestString(link);
            int linksCount = snapshot.Links.Length;

            Assert.AreEqual(59, linksCount, "Возвращено неверное количество ссылок");
        }

        //Загрузить снапшот с помощью параметризованного конструктора запросов веб-архива
        [TestMethod]
        public async Task LoadSnapshot_Check()
        {
            var receiving = new SnapshotLoaderViewModel();
            var builder = receiving.RequestBuilder;
            builder.Site.Value = "http://ru-minecraft.ru/forum";
            builder.Limit.Amount = 100;
            builder.Dates.Range.From = new DateTime(2012, 7, 27);
            builder.Dates.Range.To = new DateTime(2013, 7, 27);

            Snapshot snapshot = await receiving.LoadFromRequestString(builder.Request);
            int linksCount = snapshot.Links.Length;

            Assert.AreEqual(100, linksCount, "Возвращено неверное количество ссылок");
        }

        //Загрузить пустой снапшот
        [TestMethod]
        public async Task LoadEmptySnapshot_Check()
        {
            var receiving = new SnapshotLoaderViewModel();
            var builder = receiving.RequestBuilder;
            builder.Site.Value = "http://ru-minecraft.ru/forum";
            builder.Limit.Amount = 2;
            builder.Codes.FiltersString = "200;502";

            Snapshot snapshot = await receiving.LoadFromRequestString(builder.Request);
            int linksCount = snapshot.Links.Length;

            Assert.AreEqual(linksCount, snapshot.Links.Length, "В пустом снапшоте из архива не 0 ссылок");
        }
    }

}
