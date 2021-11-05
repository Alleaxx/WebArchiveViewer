using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchive.Data
{
    public class CategoriesInfo
    {
        public readonly ICategory MainCategory;

        //Все категории, но с ключом доступа
        public readonly Dictionary<string, ICategory> Dictionary;


        public CategoriesInfo(Snapshot snapshot) : this(snapshot.RulesControl, snapshot.Links.Select(l => l.Category))
        {

        }
        //Сформировать иерархию категорий на основе правил и ссылок
        public CategoriesInfo(GroupRule rule, IEnumerable<string> categories)
        {
            MainCategory = new Category(rule);
            Dictionary<string, ICategory> dictionary = MainCategory
                .AllInnerCatesSelf()
                .GroupBy(c => c.Name)
                .Select(g => g.First())
                .ToDictionary(c => c.Name);

            UpdateWithActual();
            MainCategory.RemoveNullInnerCates();
            var notEmptyCates = dictionary.Values.Where(p => p.ItemsTotal > 0);
            Dictionary = notEmptyCates.ToDictionary(p => p.Name);

            void UpdateWithActual()
            {
                var catesLinks = categories.GroupBy(l => l);
                foreach (var group in catesLinks)
                {
                    if (dictionary.ContainsKey(group.Key))
                    {
                        dictionary[group.Key].ItemsAmount = group.Count();
                    }
                }
            }
        }
    }
}
