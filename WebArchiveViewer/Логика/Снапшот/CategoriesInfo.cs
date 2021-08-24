using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchiveViewer
{
    public class CategoriesInfo
    {
        public IEnumerable<ICategory> Collection { get; private set; }
        public Dictionary<string, ICategory> Dictionary { get; private set; }

        public CategoriesInfo()
        {

        }
        public CategoriesInfo(IEnumerable<ICategory> cates, Dictionary<string, ICategory> dict)
        {
            Collection = cates;
            Dictionary = dict;
        }
    }
}
