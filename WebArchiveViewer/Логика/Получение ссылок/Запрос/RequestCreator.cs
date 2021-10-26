using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchiveViewer
{
    //Построитель запроса
    public interface IRequestCreator
    {
        string GetRequest();
        string Request { get; }
    }

    //Запрос из заданной строки
    public class DefaultRequestCreator : NotifyObj, IRequestCreator
    {
        public override string ToString() => $"Построитель запроса: {Request}";
        public string GetRequest() => Request;
        public string Request
        {
            get => request;
            set
            {
                request = value;
                OnPropertyChanged();
            }
        }
        private string request;
        public DefaultRequestCreator(string request)
        {
            Request = request;
        }
    }
}
