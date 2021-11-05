using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebArchiveViewer;

using WebArchive.Data;
namespace WebArchive.Tests
{

    [TestClass]
    public class ArchiveReceiveTest
    {
        //Проверить доступность загрузки снапшота при нулевом запросе
        [TestMethod]
        public void EmptyRequestDefaultAvailable_Check()
        {
            ArchiveSnapLoader receiving = new ArchiveSnapLoader();
            receiving.RequestDefaultCreator = new DefaultRequestCreator("");
            bool executeAvailable = receiving.UploadLinksCommand.CanExecute(null);
            Assert.IsFalse(executeAvailable, "Команда загрузки ссылок доступна при нулевом прямом запросе");
        }
        [TestMethod]
        public void EmptyRequestArhiveAvailable_Check()
        {
            ArchiveSnapLoader receiving = new ArchiveSnapLoader();
            receiving.RequestArchiveCreator.Site.Value = "";
            bool executeAvailable = receiving.UploadLinksCommand.CanExecute(null);
            Assert.IsFalse(executeAvailable, "Команда загрузки ссылок доступна при нулевом запросе");
        }



        //Загрузить снапшот из прямой ссылки
        [TestMethod]
        public async Task LoadDirectSnapshot_Check()
        {
            ArchiveSnapLoader receiving = new ArchiveSnapLoader
            {
                RequestDefaultCreator = new DefaultRequestCreator("http://web.archive.org/cdx/search/cdx?url=https://www.noob-club.ru/&matchType=prefix&output=json&limit=59&from=20170209100353&to=20170831025157")
            };

            Snapshot snapshot = await receiving.UploadLinks();
            int linksCount = snapshot.Links.Length;

            Assert.AreEqual(59, linksCount, "Возвращено неверное количество ссылок");
        }

        //Загрузить снапшот с помощью параметризованного конструктора запросов веб-архива
        [TestMethod]
        public async Task LoadSnapshot_Check()
        {
            ArchiveSnapLoader receiving = new ArchiveSnapLoader();
            var creator = receiving.RequestArchiveCreator;
            creator.Site.Value = "http://ru-minecraft.ru/forum";
            creator.Limit.Amount = 100;
            creator.Dates.Range.From = new DateTime(2012, 7, 27);
            creator.Dates.Range.To = new DateTime(2013, 7, 27);

            Snapshot snapshot = await receiving.UploadLinks();
            int linksCount = snapshot.Links.Length;

            Assert.AreEqual(100, linksCount, "Возвращено неверное количество ссылок");
        }

        //Загрузить пустой снапшот
        [TestMethod]
        public async Task LoadEmptySnapshot_Check()
        {
            ArchiveSnapLoader receiving = new ArchiveSnapLoader();
            var creator = receiving.RequestArchiveCreator;
            creator.Site.Value = "http://ru-minecraft.ru/forum";
            creator.Limit.Amount = 2;
            creator.Codes.FiltersString = "200;502";

            Snapshot snapshot = await receiving.UploadLinks();
            int linksCount = snapshot.Links.Length;

            Assert.AreEqual(linksCount, snapshot.Links.Length, "В пустом снапшоте из архива не 0 ссылок");
        }
    }

}
