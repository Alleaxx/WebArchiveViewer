using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace WebArchiveViewer
{
    //Сайт со ссылками из архива на указанную дату
    [Serializable]
    class SiteSnapshot
    {
        public string FilePath { get; set; }
        public string FileName { get; set; }


        public string SourceURI { get; set; }
        public DateTime Date { get; set; }
        public ViewOptionsGetLinks ViewOptions { get; set; }
        public ArchiveLink[] Links { get; set; }

        public SiteSnapshot()
        {

        }
        public void Save(string path)
        {
            FilePath = path;
            FileName = new FileInfo(path).Name;

            IFileDialog fileDialog = new FileDialog();
            fileDialog.SaveFile(path, this);
        }


        public List<OptionCount<string>> GetCategories(IRulesControl rulesControl)
        {
            var categoriesFound = new List<OptionCount<string>>();
            foreach (ArchiveLink link in Links)
            {
                string category = rulesControl.CheckLink(link);
                link.Category = category;
                OptionCount<string> cate = categoriesFound.Find(o => o.Value == category);

                if (cate == null)
                    categoriesFound.Add(new OptionCount<string>(category, true));
                else if (cate != null)
                    cate.Amount++;
            }
            return categoriesFound;
        }
    }
}
