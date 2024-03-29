﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchive.Data
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
        IEnumerable<ICategory> AllInnerCates();
        IEnumerable<ICategory> AllInnerCatesSelf();
    }
    public class Category : Option, ICategory
    {
        public override string ToString()
        {
            return $"Категория {Name}: {ItemsAmount} | {ItemsInner} | {ItemsTotal}";
        }

        public string Name { get; private set; }

        public override bool Enabled
        {
            get => base.Enabled;
            set
            {
                base.Enabled = value;
                foreach (ICategory cate in InnerCates)
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
            foreach (ICategory cateInner in InnerCates)
            {
                if (cateInner.Name.Equals(name))
                {
                    return cateInner;
                }
                if (cateInner.FindCategory(name) is ICategory found)
                {
                    return found;
                }
            }
            return null;
        }
        public void RemoveNullInnerCates()
        {
            InnerCates = InnerCates.Where(c => c.ItemsTotal > 0).ToArray();
            foreach (var innerCate in InnerCates)
            {
                innerCate.RemoveNullInnerCates();
            }
        }


        public IEnumerable<ICategory> AllInnerCates()
        {
            List<ICategory> cates = new List<ICategory>();
            foreach (ICategory cate in InnerCates)
            {
                var inner = cate.AllInnerCates();
                cates.Add(cate);
                cates.AddRange(inner);
            }
            return cates;
        }
        public IEnumerable<ICategory> AllInnerCatesSelf()
        {
            var inner = AllInnerCates();
            List<ICategory> cates = new List<ICategory>(inner);
            cates.Add(this);
            return cates;
        }
    }
}
