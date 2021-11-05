using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WebArchive.Data;
namespace WebArchiveViewer
{
    public class ProcessProgress : NotifyObj
    {
        public override string ToString()
        {
            return $"Процесс: {Status}: {Now} / {Maximum}";
        }

        public bool InProgress => Now > 0 && Now < Maximum;

        public int Maximum { get; private set; }
        public int Now { get; private set; }
        public string Status { get; private set; }

        public void SetStatus(string status, int now)
        {
            Status = status;
            Now = now;

            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(Now));
            OnPropertyChanged(nameof(InProgress));
        }

        public ProcessProgress(string status, int max)
        {
            Status = status;
            Now = 0;
            Maximum = max;
        }
    }
}
