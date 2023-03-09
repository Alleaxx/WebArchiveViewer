using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchiveViewer.ViewModels.ViewOptions
{
    public enum SortsEnum
    {
        Date,           //дата
        PageName,       //имя страницы
        LinkURL,        //ссылка
        MimeType,       //тип ссылки
        StatusCode,     //код ответа
        Category,       //категория
        LinkIndex,      //порядковый номер ссылки
        None            //нет
    }
}
