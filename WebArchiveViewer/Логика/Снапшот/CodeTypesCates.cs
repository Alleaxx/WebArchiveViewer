using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchiveViewer
{
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
    public interface ICategory
    {
        string Name { get; }
        int ItemsAmount { get; set; }
        bool Enabled { get; set; }
    }


    public class Option : NotifyObj
    {
        public bool Enabled
        {
            get => enabled;
            set
            {
                enabled = value;
                OnPropertyChanged();
            }
        }
        private bool enabled = true;
    }
    public class StatusCode : Option, ICode
    {
        public string Code { get; private set; }
        public StatusCode(string code)
        {
            Code = code;
        }
    }
    public class MimeType : Option, IType
    {
        public string Type { get; private set; }
        public MimeType(string type)
        {
            Type = type;
        }
    }


    public class Category : Option, ICategory
    {
        public string Name { get; private set; }
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


        public Category(string name)
        {
            Name = name;
            ItemsAmount = 1;
            Enabled = true;
        }
    }

}
