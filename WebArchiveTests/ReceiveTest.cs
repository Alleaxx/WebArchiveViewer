using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebArchiveViewer;

namespace WebArchiveTests
{

    [TestClass]
    public class ArchiveReceiveTest
    {
        //Проверить запрос без интернета


        //Проверить доступность загрузки снапшота при нулевом запросе
        [TestMethod]
        public void NullRequestAvailable()
        {
            ArchiveReceiveView receiving = new ArchiveReceiveView();
            receiving.RequestDefaultCreator = new DefaultRequestCreator("");
            bool executeAvailable = receiving.UploadLinksCommand.CanExecute(null);
            Assert.IsFalse(executeAvailable, "Команда загрузки ссылок доступна при нулевом прямом запросе");

            receiving.RequestDefaultCreator = null;
            receiving.RequestArchiveCreator.Site.Value = "";
            executeAvailable = receiving.UploadLinksCommand.CanExecute(null);
            Assert.IsFalse(executeAvailable, "Команда загрузки ссылок доступна при нулевом запросе");
        }

        //Загрузить снапшот из прямой ссылки
        [TestMethod]
        public async Task LoadDirectSnapshot()
        {
            ArchiveReceiveView receiving = new ArchiveReceiveView();
            receiving.RequestDefaultCreator = new DefaultRequestCreator("http://web.archive.org/cdx/search/cdx?url=https://www.noob-club.ru/&matchType=prefix&output=json&limit=59&from=20170209100353&to=20170831025157");

            var command = receiving.UploadLinksCommand;
            bool executeAvailable = command.CanExecute(null);
            Assert.AreEqual(true, executeAvailable, "Команда загрузки ссылок недоступна");

            ISnapshot snapshot = await receiving.UploadLinks();
            Assert.AreEqual(59, snapshot.Links.Length, "Возвращено неверное количество ссылок");
            Assert.AreEqual(2, snapshot.ViewOptions.Codes.Length, "Возвращено неверное количество кодов ответа");
            Assert.AreEqual(1, snapshot.ViewOptions.Types.Length, "Возвращено неверное количество mime-типов");
        }

        //Загрузить снапшот с помощью параметризованного конструктора запросов веб-архива
        [TestMethod]
        public async Task LoadSnapshot()
        {
            ArchiveReceiveView receiving = new ArchiveReceiveView();
            var creator = receiving.RequestArchiveCreator;
            creator.Site.Value = "http://ru-minecraft.ru/forum";
            creator.Limit.Amount = 100;
            creator.Dates.Range.From = new System.DateTime(2012, 7, 27);
            creator.Dates.Range.To = new System.DateTime(2013, 7, 27);

            var command = receiving.UploadLinksCommand;
            bool executeAvailable = command.CanExecute(null);
            Assert.AreEqual(true, executeAvailable, "Команда загрузки ссылок недоступна");

            ISnapshot snapshot = await receiving.UploadLinks();
            Assert.AreEqual(100, snapshot.Links.Length, "Возвращено неверное количество ссылок");
            Assert.AreEqual(2, snapshot.ViewOptions.Codes.Length, "Возвращено неверное количество кодов ответа");
            Assert.AreEqual(1, snapshot.ViewOptions.Types.Length, "Возвращено неверное количество mime-типов");

            var rules = snapshot.RulesControl;
            Assert.AreEqual(true, rules != null && rules.MainRules.Count > 0, "Нет правил формирования категорий");
        }

        //Загрузить пустой снапшот
        [TestMethod]
        public async Task LoadEmpty()
        {
            ArchiveReceiveView receiving = new ArchiveReceiveView();
            var creator = receiving.RequestArchiveCreator;
            creator.Site.Value = "http://ru-minecraft.ru/forum";
            creator.Limit.Amount = 2;
            creator.Codes.FiltersString = "200;502";

            var command = receiving.UploadLinksCommand;
            bool executeAvailable = command.CanExecute(null);
            Assert.AreEqual(true, executeAvailable, "Команда загрузки ссылок недоступна");

            ISnapshot snapshot = await receiving.UploadLinks();
            Assert.AreEqual(0, snapshot.Links.Length, "В пустом снапшоте из архива не 0 ссылок");
            Assert.AreEqual(receiving.SetSnapshotCommand.CanExecute(null), false, "Можно просмотреть пустой возвращенный снапшот");
        }
    }

}
