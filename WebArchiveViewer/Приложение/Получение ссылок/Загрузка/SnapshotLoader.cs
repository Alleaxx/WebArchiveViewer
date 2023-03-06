using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using WebArchive.Data;
namespace WebArchiveViewer
{
    //Базовый класс для загрузчика снапшота
    public abstract class SnapshotLoader : NotifyObject
    {
        public override string ToString()
        {
            return "Загрузчик снапшота";
        }

        protected readonly SnapshotImporter Importer;
        protected readonly IFileDialog FileDialog;
        private Snapshot snapshot;

        public Snapshot Snapshot
        {
            get => snapshot;
            protected set
            {
                snapshot = value;
                OnPropertyChanged();
            }
        }

        protected SnapshotLoader(SnapshotImporter importer)
        {
            Importer = importer;
            FileDialog = new FileDialog();
        }

        protected override void InitCommands()
        {
            LoadCommand = new RelayCommand(obj => LoadSnapshot(), RelayCommand.IsAlwaysAvailable);
        }
        public ICommand LoadCommand { get; private set; }
        protected abstract void LoadSnapshot();
        protected void SendSnapshot()
        {
            if(Importer == null)
            {
                return;
            }
            Importer.ReceiveSnapshot(Snapshot);
        }
    }
}
