using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using WebArchiveViewer;

using WebArchive.Data;
using WebArchive.Data.RequestParts;
namespace WebArchive.Tests
{
    [TestClass]
    public class RequestCreatorTest
    {
        //Проверка типичного запроса
        [TestMethod]
        public void RequestBuild1_Check()
        {
            string expected = "http://web.archive.org/cdx/search/cdx?url=https://www.noob-club.ru/&matchType=prefix&output=json&limit=10";
            ArchiveRequestCreator creator = new ArchiveRequestCreator();
            creator.Site.Value = "https://www.noob-club.ru/";
            creator.Dates.Enabled = false;
            creator.Limit.Amount = 10;

            string request = creator.GetRequest();

            Assert.AreEqual(expected, request, "Запрос 1 построен неверно");
        }

        //Проверка на лимит и область поиска
        [TestMethod]
        public void RequestBuild2_Check()
        {
            string expected = "http://web.archive.org/cdx/search/cdx?url=https://ru-minecraft.ru/forum&matchType=exact&output=json&limit=70&from=20110727120000&to=20120727120000";
            ArchiveRequestCreator creator = new ArchiveRequestCreator();
            creator.Site.Value = "https://ru-minecraft.ru/forum";
            creator.MatchType.Selected = new LinkMatchType(MatchType.exact);
            creator.Limit.Amount = 70;

            string request = creator.GetRequest();

            Assert.AreEqual(expected, request, "Запрос 2 построен неверно");
        }

        //Проверка дат и фильтров по типу, коду
        [TestMethod]
        public void RequestBuild3_Check()
        {
            string expected = "http://web.archive.org/cdx/search/cdx?url=https://www.noob-club.ru/&matchType=prefix&output=json&filter=!statuscode:200&filter=!statuscode:502&filter=mimetype:text/html";
            ArchiveRequestCreator creator = new ArchiveRequestCreator();
            creator.Site.Value = "https://www.noob-club.ru/";
            creator.Dates.Enabled = false;
            creator.Codes.FiltersString = "!200; !502";
            creator.Types.FiltersString = "text/html;";

            string request = creator.GetRequest();

            Assert.AreEqual(expected, request, "Запрос 3 построен неверно");
        }


        [TestMethod]
        public void RequestDefault_Check()
        {
            string expected = "https://test.ru";
            DefaultRequestCreator creator = new DefaultRequestCreator(expected);

            string request = creator.GetRequest();

            Assert.AreEqual(expected, request, "Стандартный запрос определяется неверно");
        }
    }

}
