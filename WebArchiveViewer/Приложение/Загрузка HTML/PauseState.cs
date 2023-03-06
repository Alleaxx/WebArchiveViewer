using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WebArchive.Data;
namespace WebArchiveViewer
{
    public class PauseState : NotifyObject
    {
        public override string ToString()
        {
            return $"Пауза: {(IsPaused ? "активна" : "выключена")}";
        }

        private bool isPaused;
        private DateTime startDateTime;

        public bool IsPaused
        {
            get => isPaused;
            set
            {
                isPaused = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsPlayed));
            }
        }
        public bool IsPlayed
        {
            get => !isPaused;
            set
            {
                IsPaused = !value;
                OnPropertyChanged();
            }
        }

        public DateTime StartDateTime
        {
            get => startDateTime;
            set
            {
                startDateTime = value;
                OnPropertyChanged();
            }
        }
        public TimeSpan FromStart => DateTime.Now - StartDateTime;

        public PauseState(bool paused)
        {
            IsPaused = paused;
            StartDateTime = DateTime.Now;
        }
    }
}
