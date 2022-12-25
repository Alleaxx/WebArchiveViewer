using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchive.Data.RequestParts
{
    //Формат вывода ответа
    //Заполняется двумя предустановленными значениями: json или <пусто>
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
    public enum OutputType
    {
        JSON,
        Usual
    }
    public class RequestOutput : RequestPart
    {
        public OutputTypes[] Types { get; private set; } = new OutputTypes[]
        {
            new OutputTypes(OutputType.JSON),
            new OutputTypes(OutputType.Usual),
        };
        public OutputTypes Selected { get; set; }

        public override string Value => Selected.Value;
        public RequestOutput() : base("output", "Вывод", "json")
        {
            Selected = Types[0];
        }
    }
}
