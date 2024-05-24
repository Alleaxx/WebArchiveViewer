using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchive.Data
{
    /// <summary>
    /// Информация о протекающем процессе
    /// </summary>
    public class Operation
    {
        /// <summary>
        /// Состояние процесса
        /// </summary>
        public string Status { get; private set; }

        /// <summary>
        /// Готовность (от 0 до 100)
        /// </summary>
        public int ReadyPercentage { get; private set; }

        /// <summary>
        /// Успешность (отсутствие ошибок)
        /// </summary>
        public bool IsSuccessfull { get; private set; }

        /// <summary>
        /// Признак завершения
        /// </summary>
        public bool IsEnded { get; private set; }

        /// <summary>
        /// Возникшее исключение
        /// </summary>
        public Exception Exception { get; private set; }

        /// <summary>
        /// Дата возникновения
        /// </summary>
        public DateTime Date { get; private set; }

        public Operation(string status, int ready, bool isSuccessfull = true, bool isEnded = false, Exception ex = null)
        {
            Status = status;
            Date = DateTime.Now;
            ReadyPercentage = ready;
            IsSuccessfull = isSuccessfull;
            IsEnded = isEnded;
            Exception = ex;
        }
    }
}
