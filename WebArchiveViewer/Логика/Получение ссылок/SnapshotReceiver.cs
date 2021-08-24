using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WebArchiveViewer
{

    public class SnapshotReceiver : NotifyObj
    {
        public override string ToString() => "Импортер снапшотов";

        private ArchiveView SnapshotView { get; set; }

        public ArchiveSnapLoader ArchiveLoader { get; private set; }
        public FileSnapLoader FileLoader { get; private set; }

        public SnapshotReceiver() : this(null) { }
        public SnapshotReceiver(ArchiveView view)
        {
            SnapshotView = view;
            ArchiveLoader = new ArchiveSnapLoader(this);
            FileLoader = new FileSnapLoader(this);
        }

        public void ReceiveSnapshot(SiteSnapshot snapshot)
        {
            if (SnapshotView != null)
            {
                SnapshotView.CurrentSnapshot = snapshot;
            }
        }
    }



    public abstract class SnapLoader : NotifyObj
    {
        public override string ToString() => "Загрузчик снапшота";

        private SnapshotReceiver Receiver { get; set; }
        public SiteSnapshot Snapshot
        {
            get => snapshot;
            protected set
            {
                snapshot = value;
                OnPropertyChanged();
            }
        }
        private SiteSnapshot snapshot;
        protected IFileDialog FileDialog { get; set; }


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
            if(Receiver != null)
            {
                Receiver.ReceiveSnapshot(Snapshot);
            }
        }
    }

}
