using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchiveViewer
{

    public class FileSnapLoader : SnapLoader
    {
        public override string ToString() => "Файловый загрузчик снапшота";
        public FileSnapLoader(SnapshotReceiver receiver) : base(receiver)
        {

        }

        protected async override void LoadSnapshot()
        {
            var file = FileDialog.Open();
            if (file != null && file.Exists)
            {
                string path = file.FullName;
                await Task.Run(() => OpenFrom(path));
                SendSnapshot();
            }
        }
        public void OpenFrom(string path)
        {
            Snapshot = FileDialog.OpenReadJson<SiteSnapshot>(path);
            Snapshot.FilePath = path;
            Snapshot.ViewOptions.SetSnapshot(Snapshot);
        }
    }
}
