using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using WebArchiveViewer;

namespace WebArchiveTests
{
    //Сформировать запрос с заданными параметрами, проверить корректность строки
    //+ с датами
    //+ без дат
    //+ с фильтром по коду ответа
    //+ с отрицательным фильтром по коду ответа и фильтром по mime-типу
    [TestClass]
    public class RequestCreatorTest
    {
        [TestMethod]
        public void RequestCreate1Test()
        {
            ArchiveRequestCreator creator = new ArchiveRequestCreator();
            creator.Site.Value = "https://www.noob-club.ru/";
            creator.Dates.Enabled = false;
            creator.Limit.Amount = 10;
            string request = creator.GetRequest();
            string correctRequest = "http://web.archive.org/cdx/search/cdx?url=https://www.noob-club.ru/&matchType=prefix&output=json&limit=10";

            Assert.AreEqual(correctRequest, request, "Запрос 1 построен неверно");
        }
        [TestMethod]
        public void RequestCreate2Test()
        {
            ArchiveRequestCreator creator = new ArchiveRequestCreator();
            creator.Site.Value = "https://ru-minecraft.ru/forum";
            creator.MatchType.Selected = new LinkMatchType(MatchType.exact);
            creator.Limit.Amount = 70;
            string request = creator.GetRequest();
            string correctRequest = "http://web.archive.org/cdx/search/cdx?url=https://ru-minecraft.ru/forum&matchType=exact&output=json&limit=70&from=20110727120000&to=20120727120000";

            Assert.AreEqual(correctRequest, request, "Запрос 2 построен неверно");
        }
        [TestMethod]
        public void RequestCreate3Test()
        {
            ArchiveRequestCreator creator = new ArchiveRequestCreator();
            creator.Site.Value = "https://www.noob-club.ru/";
            creator.Dates.Enabled = false;
            creator.Codes.FiltersString = "!200; !502";
            creator.Types.FiltersString = "text/html;";
            string request = creator.GetRequest();
            string correctRequest = "http://web.archive.org/cdx/search/cdx?url=https://www.noob-club.ru/&matchType=prefix&output=json&filter=!statuscode:200&filter=!statuscode:502&filter=mimetype:text/html";

            Assert.AreEqual(correctRequest, request, "Запрос 3 построен неверно");
        }

    }

}
