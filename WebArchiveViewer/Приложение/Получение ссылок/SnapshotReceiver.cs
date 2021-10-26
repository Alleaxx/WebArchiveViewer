using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using WebArchive.Data;
namespace WebArchiveViewer
{
    //Получатель снапшота
    public class SnapshotReceiver : NotifyObj
    {
        public override string ToString()
        {
            return "Импортер снапшотов";
        }

        private readonly ArchiveView ArchiveView;

        public ArchiveSnapLoader ArchiveLoader { get; private set; }
        public FileSnapLoader FileLoader { get; private set; }

        public SnapshotReceiver() : this(null) { }
        public SnapshotReceiver(ArchiveView view)
        {
            ArchiveView = view;
            ArchiveLoader = new ArchiveSnapLoader(this);
            FileLoader = new FileSnapLoader(this);
        }

        public void ReceiveSnapshot(Snapshot snapshot)
        {
            if(ArchiveView != null)
            {
                ArchiveView.SetSnapshot(snapshot);
            }
        }
    }
}
