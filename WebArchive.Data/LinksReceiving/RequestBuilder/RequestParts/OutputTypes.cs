using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using WebArchive.Data.RequestParts;

namespace WebArchive.Data.RequestParts
{
    public enum OutputType
    {
        JSON,
        Usual
    }
    /// <summary>
    /// Формат вывода ответа
    /// Заполняется двумя предустановленными значениями: json или <пусто>
    /// </summary>
    public class OutputTypes : RequestPart
    {
        public OutputTypes(OutputType type) : base("output", "Вывод", "json")
        {
            switch (type)
            {
                case OutputType.JSON:
                    Value = "json";
                    Name = "JSON";
                    Description = "Возвращает JSON-массив объектов";
                    break;
                case OutputType.Usual:
                    Value = "";
                    Name = "Строки";
                    Description = "Возвращает список строк";
                    break;
            }
        }
    }
}
