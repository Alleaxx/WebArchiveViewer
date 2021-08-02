using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchiveViewer
{


    public class Option<T, K> : NotifyObj
    {
        public T Value { get; set; }
        public K Info { get; set; }

        public bool Enabled
        {
            get => enabled;
            set
            {
                enabled = value;
                OnPropertyChanged();
            }
        }
        private bool enabled;

        public Option()
        {

        }
        public Option(T val, K info, bool enabled = true)
        {
            Value = val;
            Info = info;
            Enabled = enabled;
        }

    }
    public class Option<T> : Option<T,T>
    {
        public Option()
        {

        }
        public Option(T val, bool enabled = true) : this(val, default, enabled)
        {

        }
        public Option(T val, T info, bool enabled = true) : base(val, info, enabled)
        {

        }
    }

    interface ICode
    {
        string Code { get; }
        bool Enabled { get; set; }
    }
    interface IType
    {
        string Type { get; }
        bool Enabled { get; set; }
    }




    public interface ISorting
    {
        string Name { get; }
        Func<IArchLink, string> KeySelector { get; }
    }
    class Sorting : Option<string,Func<IArchLink,string>>, ISorting
    {
        public string Name => Value;
        public Func<IArchLink, string> KeySelector => Info;


        public Sorting(string name, Func<IArchLink, string> func, bool enabled) : base(name, func, enabled)
        {

        }
    }

    public interface IGrouping
    {
        string Name { get; }
        string Key { get; }
    }
    class Grouping : Option<string,string>, IGrouping
    {
        public string Name => Value;
        public string Key => Info;

        public Grouping(string name, string key, bool enabled) : base(name, key, enabled)
        {

        }
    }


    public interface ICategory
    {
        string Name { get; }
        int ItemsAmount { get; set; }
        bool Enabled { get; set; }
    }
    class Category : Option<string,int>, ICategory
    {
        public string Name => Value;
        public int ItemsAmount
        {
            get => Info;
            set
            {
                Info = value;
            }
        }


        public Category(string name) : base(name, 1, true)
        {

        }
    }
}
