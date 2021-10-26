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
        //Ссылка и простые правила
        [TestMethod]
        public void MatchSimpleTest()
        {
            string link = "http://ru-minecraft.ru/forum/showtopic-10000";
            GroupRule rule1 = new GroupRule("Правило true", "forum");
            string result1 = rule1.IsMatched(link);
            Assert.AreEqual("Правило true", result1, "Правило не работает");

            GroupRule rule2 = new GroupRule("Правило false", "fomur");
            string result2 = rule2.IsMatched(link);
            Assert.IsTrue(string.IsNullOrEmpty(result2), "Правило не работает");
        }

        //Ссылка и вложенные правила
        [TestMethod]
        public void MatchInnerTest()
        {
            string link = "http://ru-minecraft.ru/forum/showtopic-10000";
            GroupRule main = new GroupRule("Румайн", "ru-minecraft.ru",
                new GroupRule("Форум", "forum",
                    new GroupRule("Тема", "showtopic")));

            string result1 = main.IsMatched(link);
            Assert.AreEqual("Тема", result1, "Внутреннее правило не работает");

            string result2 = main.IsMatched("http://ru-minecraft.ru/forum");
            Assert.AreEqual("Форум", result2, "Внутреннее правило не работает");
        }

        //Ссылки и контроль правил
        [TestMethod]
        public void CheckCategorisation()
        {
            RulesControl control = new RulesControl();
            control.AddRules(new RumineRules());

            string[] links = new string[]
            {
                "http://ru-minecraft.ru/forum/showtopic-10000",
                "http://ru-minecraft.ru/forum/showtopic-10000",
                "http://ru-minecraft.ru/forum/showtopic-10000",
                "http://ru-minecraft.ru/forum/",
                "http://ru-minecraft.ru/forum/",
                "http://ru-minecraft.ru/",
                "https://vk.com/",
            };

            var Found = new Dictionary<string, int>();
            foreach (var link in links)
            {
                string category = control.CheckLink(link);
                if (!Found.ContainsKey(category))
                    Found.Add(category, 0);
                Found[category]++;
            }

            var Required = new Dictionary<string, int>()
            {
                ["Форум"] = 2,
                ["Форумная тема"] = 3,
                ["Румине"] = 1,
                ["Всё"] = 1,
            };
            foreach (var req in Required)
            {
                if (Found[req.Key] != req.Value)
                    Assert.Fail($"Ошибка категоризации, необходимо '{req.Key}' - {req.Value}, найдено {req.Value}");
            }


        }
    }
}
