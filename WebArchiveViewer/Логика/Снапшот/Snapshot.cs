using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchiveViewer
{
    //Сохраняемый снапшот (список ссылок полученных из запроса)
    public class Snapshot
    {
        //Сведения о файлах
        public string FolderHtmlSavePath { get; set; }
        public string FilePath { get; set; }


        //Сведения о запросе
        public string Request { get; set; }
        public string SourceURI { get; set; }
        public DateTime ReceivingDate { get; set; }


        public RulesControl RulesControl { get; set; }
        public ArchiveLink[] Links { get; set; }




        //Из сохранения
        public Snapshot()
        {

        }
        //Копия для сохранения
        public Snapshot(Snapshot snapshot, IEnumerable<ArchiveLink> links)
        {
            FolderHtmlSavePath = snapshot.FolderHtmlSavePath;
            FilePath = snapshot.FilePath;
            Request = snapshot.Request;
            SourceURI = snapshot.SourceURI;
            ReceivingDate = snapshot.ReceivingDate;
            RulesControl = snapshot.RulesControl;
            Links = links.ToArray();
        }
        //Из архива
        public Snapshot(string request, string sourceLink, IEnumerable<ArchiveLink> links)
        {
            FolderHtmlSavePath = $"{System.IO.Directory.GetCurrentDirectory()}\\Ссылки";
            ReceivingDate = DateTime.Now;
            Request = request;
            SourceURI = sourceLink;
            Links = links.ToArray();

            CreateRulesIfNull();
        }

        //Категории, их обновление
        private void UpdateLinkCategories()
        {
            foreach (ArchiveLink link in Links)
            {
                string category = RulesControl.CheckLink(link);
                link.Category = category;
            }
        }
        public CategoriesInfo GetCategories()
        {
            CreateRulesIfNull();
            UpdateLinkCategories();

            var mainRule = RulesControl.Rule;
            var cates = RulesControl.Rules.Select(r => new Category(r)).ToList();
            var cateDictionary = Category.GetDictionary(cates);

            CountCategoriesElements(cates, cateDictionary);
            foreach (var cate in cates)
            {
                cate.RemoveNullInnerCates();
            }

            return new CategoriesInfo(cates, cateDictionary);
        }
        private void CountCategoriesElements(List<Category> categories, Dictionary<string, ICategory> dictionary)
        {
            foreach (ArchiveLink link in Links)
            {
                string cateName = link.Category;

                if (!dictionary.ContainsKey(cateName))
                {
                    var newCate = new Category(cateName);
                    dictionary.Add(newCate.Name, newCate);
                    categories.Add(newCate);
                }
                dictionary[cateName].ItemsAmount++;
            }
        }


        private void CreateRulesIfNull()
        {
            if (RulesControl == null)
            {
                RulesControl = new RulesControl();
                RulesControl.AddRules(new RumineRules());
            }
        }
    }
}