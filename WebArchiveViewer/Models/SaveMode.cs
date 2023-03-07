using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchiveViewer
{
    public enum SaveMode
    {
        AllShowed,              //все показанные на странице
        AllFiltered,            //все прошедшие фильтрацию
        AllNotFiltered,         //все не прошедшие фильтрацию
        All,                    //все
        AllDefaultPath,         //все, сохранение по стандартному пути
    }
}
