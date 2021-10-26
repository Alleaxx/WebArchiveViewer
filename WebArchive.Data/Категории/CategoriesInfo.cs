using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchive.Data
{
    public class CategoriesInfo
    {
        public readonly IEnumerable<ICategory> Collection;
        public readonly Dictionary<string, ICategory> Dictionary;

        public CategoriesInfo(IEnumerable<ICategory> cates, Dictionary<string, ICategory> dict)
        {
            Collection = cates;
            Dictionary = dict;
        }
    }
}
