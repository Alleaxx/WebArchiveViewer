using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WebArchive.Data;

namespace WebArchiveViewer.ViewModels
{
    public class OperationState : NotifyObject
    {
        private bool isPaused;
        private DateTime startDateTime;

        public bool IsPaused
        {
            get => isPaused;
            set
            {
                Set(ref isPaused, value);
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
            set => Set(ref startDateTime, value);
        }
        public TimeSpan FromStart => DateTime.Now - StartDateTime;

        public OperationState(bool paused)
        {
            IsPaused = paused;
            StartDateTime = DateTime.Now;
        }
    }
}
