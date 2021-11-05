using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using WebArchive.Data;
namespace WebArchive.Tests
{
    [TestClass]
    public class CategoriesTest
    {
        [TestMethod]
        public void CategoryInfo_Check()
        {
            CategoriesInfo info = CreateInfo();

            int count = info.Dictionary.Count();

            Assert.AreEqual(3, count, "Число найденных категорий не соответствует ожидаемому");
        }
        [TestMethod]
        public void CategoryInfoEmpty_Check()
        {
            CategoriesInfo info = CreateInfo();

            bool noEmpty = !info.Dictionary.Any(p => p.Value.ItemsTotal == 0);

            Assert.IsTrue(noEmpty, "В словаре есть пустые категории");
        }
        [TestMethod]
        public void CategoryInfoMatch_Check()
        {
            CategoriesInfo info = CreateInfo();

            var cates = info.MainCategory.AllInnerCatesSelf().ToList();
            var catesDictionary = info.Dictionary.Values.ToList();

            CollectionAssert.AreEquivalent(cates, catesDictionary, "Словарь и список категорий не совпадают");
        }


        [TestMethod]
        [DataRow("Всё", 7, 3, 4)]
        [DataRow("Румине", 4, 2, 2)]
        [DataRow("Форум", 2, 2, 0)]
        public void CategoryInfoAmount_Check(string category, int total, int amount, int inner)
        {
            CategoriesInfo info = CreateInfo();

            ICategory cate = info.Dictionary[category];

            Assert.AreEqual(total, cate.ItemsTotal, "Число всех элементов не соответствует ожидаемому");
            Assert.AreEqual(amount, cate.ItemsAmount, "Число элементов категории не соответствует ожидаемому");
            Assert.AreEqual(inner, cate.ItemsInner, "Число внутренних элементов не соответствует ожидаемому");
        }


        private CategoriesInfo CreateInfo()
        {
            GroupRule control = new GroupRule();
            control.AddInner(RulesStorage.Rumine());
            var cates = new string[] { "Всё", "Форум", "Румине", "Румине", "Форум", "Всё", "Всё" };
            var info = new CategoriesInfo(control, cates);
            return info;
        }
    }
}
