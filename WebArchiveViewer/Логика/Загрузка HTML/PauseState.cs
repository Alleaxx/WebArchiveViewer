using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchiveViewer
{
    public class PauseState : NotifyObj
    {
        public override string ToString() => $"Пауза: {(IsPaused ? "активна" : "выключена")}";

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
        private bool isPaused;
        public bool IsPlayed
        {
            get => !isPaused;
            set
            {
                IsPaused = !value;
                OnPropertyChanged();
            }
        }

        public PauseState(bool paused)
        {
            IsPaused = paused;
        }
    }
}
