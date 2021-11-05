using WebArchiveViewer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using WebArchive.Data;
namespace WebArchive.Tests
{    
    [TestClass]
    public class RulesTest
    {
        //Простые правила
        [TestMethod]
        public void MatchTrue_IsTrue()
        {
            string link = "http://ru-minecraft.ru/forum/showtopic-10000";
            GroupRule rule1 = new GroupRule("Правило true", "forum");

            string result1 = rule1.CheckLink(link);

            Assert.AreEqual(rule1.GroupName, result1, "Правило не работает");

        }

        [TestMethod]
        public void MatchFalse_IsFalse()
        {
            string link = "http://ru-minecraft.ru/forum/showtopic-10000";
            GroupRule rule2 = new GroupRule("Правило false", "fomur");

            string result2 = rule2.CheckLink(link);

            Assert.IsTrue(string.IsNullOrEmpty(result2), "Правило не работает");
        }


        //Вложенные правила
        [TestMethod]
        public void MatchInner_Check()
        {
            string link = "http://ru-minecraft.ru/forum/showtopic-10000";
            GroupRule main = new GroupRule("Румайн", "ru-minecraft.ru",
                new GroupRule("Форум", "forum",
                    new GroupRule("Тема", "showtopic")));

            string result1 = main.CheckLink(link);
            Assert.AreEqual("Тема", result1, "Внутреннее правило не работает");
        }
        [TestMethod]
        public void MatchInner2_Check()
        {
            string link = "http://ru-minecraft.ru/forum";
            GroupRule main = new GroupRule("Румайн", "ru-minecraft.ru",
                new GroupRule("Форум", "forum",
                    new GroupRule("Тема", "showtopic")));

            string result2 = main.CheckLink(link);

            Assert.AreEqual("Форум", result2, "Внутреннее правило не работает");
        }


        [TestMethod]
        [DataRow("Форум", 2)]
        [DataRow("Форумная тема", 3)]
        [DataRow("Румине", 1)]
        [DataRow("Всё", 1)]
        public void CategoryAssign_Check(string category, int amount)
        {
            GroupRule control = CreateRule();
            var links = CreateLinks();

            var found = links.ToLookup(l => control.CheckLink(l));
            int foundCount = found[category].Count();

            Assert.AreEqual(amount, foundCount, $"Ожидается [{category}] - {amount}, найдено {foundCount}");
        }


        private GroupRule CreateRule()
        {
            GroupRule rule = new GroupRule();
            rule.AddInner(RulesStorage.Rumine());
            return rule;
        }
        private IEnumerable<string> CreateLinks()
        {
            return new string[]
            {
                "http://ru-minecraft.ru/forum/showtopic-10000",
                "http://ru-minecraft.ru/forum/showtopic-10000",
                "http://ru-minecraft.ru/forum/showtopic-10000",
                "http://ru-minecraft.ru/forum/",
                "http://ru-minecraft.ru/forum/",
                "http://ru-minecraft.ru/",
                "https://vk.com/",
            };
        }
    }
}
