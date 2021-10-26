using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using WebArchive.Data;
namespace WebArchiveViewer
{
    public abstract class SnapLoader : NotifyObj
    {
        public override string ToString()
        {
            return "Загрузчик снапшота";
        }

        private readonly SnapshotReceiver Receiver;
        protected readonly IFileDialog FileDialog;

        public Snapshot Snapshot
        {
            get => snapshot;
            protected set
            {
                snapshot = value;
                OnPropertyChanged();
            }
        }
        private Snapshot snapshot;


        protected SnapLoader(SnapshotReceiver receiver)
        {
            Receiver = receiver;
            FileDialog = new FileDialog();
        }
        protected override void InitCommands()
        {
            base.InitCommands();
            LoadCommand = new RelayCommand(obj => LoadSnapshot(), RelayCommand.IsTrue);
        }


        public ICommand LoadCommand { get; private set; }
        protected abstract void LoadSnapshot();
        protected void SendSnapshot()
        {
            if (Receiver != null)
            {
                Receiver.ReceiveSnapshot(Snapshot);
            }
        }
    }
}
