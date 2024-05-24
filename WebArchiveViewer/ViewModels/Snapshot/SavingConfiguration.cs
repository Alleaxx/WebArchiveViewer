using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchiveViewer.ViewModels
{
    public enum SaveMode
    {
        AllShowed,              //все показанные на странице
        AllFiltered,            //все прошедшие фильтрацию
        AllNotFiltered,         //все не прошедшие фильтрацию
        All,                    //все
        AllDefaultPath,         //все, сохранение по стандартному пути
        OnlyRules,
    }
    public class SavingConfiguration
    {
        public string FilePath { get; set; }
        public bool UseDefaultPath => string.IsNullOrEmpty(FilePath);

        public SaveMode Mode { get; set; }

        public SavingConfiguration()
        {

        }
        public SavingConfiguration(string filePath = null, SaveMode mode = SaveMode.All)
        {
            FilePath = filePath;
            Mode = mode;
        }
        public SavingConfiguration(SaveMode mode = SaveMode.All, string filePath = null)
        {
            FilePath = filePath;
            Mode = mode;
        }
    }
}
