using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WebArchiveViewer
{
    //Страница, содержащая определенное число элементов
    public interface IPage<T>
    {
        int Number { get; }
        IEnumerable<T> Elements { get; }
        CollectionViewSource Source { get; }
    }
    public class Page<T> : IPage<T> where T : class
    {
        public int Number { get; private set; }
        public IEnumerable<T> Elements { get; private set; }
        public CollectionViewSource Source { get; private set; }

        public Page(int num, int perPage, IEnumerable<T> source)
        {
            Number = num;
            Elements = source.Skip((num - 1) * perPage).Take(perPage);
            Source = new CollectionViewSource
            {
                Source = Elements
            };
        }
    }
}
