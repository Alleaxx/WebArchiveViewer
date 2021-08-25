using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchiveViewer
{
    public interface ICategory
    {
        string Name { get; }
        int ItemsAmount { get; set; }
        int ItemsTotal { get; }
        int ItemsInner { get; }
        bool Enabled { get; set; }

        ICategory FindCategory(string name);
        void RemoveNullInnerCates();
        ICategory[] InnerCates { get; }
        IEnumerable<ICategory> GetAllInnerCates();
    }
    public class Category : Option, ICategory
    {
        public override string ToString() => $"Категория {Name}: {ItemsAmount} | {ItemsInner} | {ItemsTotal}";

        public string Name { get; private set; }

        public override bool Enabled
        {
            get => base.Enabled;
            set
            {
                base.Enabled = value;
                foreach (var cate in InnerCates)
                {
                    cate.Enabled = value;
                }
            }
        }


        //Элементов конкретно в этой категории
        public int ItemsAmount
        {
            get => itemsAmount;
            set
            {
                itemsAmount = value;
                OnPropertyChanged();
            }
        }
        private int itemsAmount;
        //Элементов в категории и во всех подкатегориях
        public int ItemsTotal => ItemsAmount + ItemsInner;
        //Элементов в под-категориях
        public int ItemsInner => InnerCates.Sum(c => c.ItemsTotal);


        public ICategory[] InnerCates { get; private set; } = new ICategory[0];


        public Category()
        {

        }
        public Category(string name)
        {
            Name = name;
            ItemsAmount = 1;
            Enabled = true;
        }
        public Category(GroupRule rule)
        {
            Name = rule.GroupName;
            ItemsAmount = 0;
            Enabled = true;
            InnerCates = rule.Rules.Select(r => new Category(r)).ToArray();
        }

        public ICategory FindCategory(string name)
        {
            var cate = InnerCates.Where(c => c.Name == name).FirstOrDefault();
            if (cate == null)
            {
                foreach (var cateInner in InnerCates)
                {
                    var res = cateInner.FindCategory(name);
                    if (res != null)
                        return res;
                }
            }
            return cate;
        }
        public void RemoveNullInnerCates()
        {
            InnerCates = InnerCates.Where(c => c.ItemsTotal > 0).ToArray();
            foreach (var innerCate in InnerCates)
            {
                innerCate.RemoveNullInnerCates();
            }
        }


        public IEnumerable<ICategory> GetAllInnerCates()
        {
            List<ICategory> cates = new List<ICategory>();
            foreach (var cate in InnerCates)
            {
                var inner = cate.GetAllInnerCates();
                cates.Add(cate);
                cates.AddRange(inner);
            }
            return cates;
        }

        public static Dictionary<string, ICategory> GetDictionary(IEnumerable<ICategory> cates)
        {
            List<ICategory> categories = new List<ICategory>();
            foreach (var cate in cates)
            {
                categories.Add(cate);
                categories.AddRange(cate.GetAllInnerCates());
            }

            Dictionary<string, ICategory> dictinary = new Dictionary<string, ICategory>();
            foreach (var rule in categories)
            {
                if (!dictinary.ContainsKey(rule.Name))
                    dictinary.Add(rule.Name, rule);
            }
            return dictinary;
        }
    }
}
