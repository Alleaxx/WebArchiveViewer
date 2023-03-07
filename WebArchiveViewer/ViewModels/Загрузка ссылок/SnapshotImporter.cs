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
    public class SnapshotImporter : NotifyObject
    {
        public override string ToString()
        {
            return "Импортер снапшотов";
        }

        private readonly ArchiveMainWindowViewModel ArchiveView;

        public LoadWindowViewModel ArchiveLoader { get; private set; }
        public FileSnapshotLoader FileLoader { get; private set; }

        public SnapshotImporter() : this(null)
        {

        }
        public SnapshotImporter(ArchiveMainWindowViewModel view)
        {
            ArchiveView = view;
            ArchiveLoader = new LoadWindowViewModel(this);
            FileLoader = new FileSnapshotLoader(this);
        }

        public void ReceiveSnapshot(Snapshot snapshot)
        {
            if(ArchiveView == null)
            {
                return;
            }

            ArchiveView.SetSnapshot(snapshot);
        }
    }
}
