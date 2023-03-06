using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WebArchive.Data;
namespace WebArchiveViewer
{
    //Загрузчик снапшота из файла
    public class FileSnapshotLoader : SnapshotLoader
    {
        public override string ToString()
        {
            return "Файловый загрузчик снапшота";
        }

        public FileSnapshotLoader(SnapshotImporter receiver) : base(receiver)
        {

        }

        protected async override void LoadSnapshot()
        {
            var file = FileDialog.Open();
            if(file == null || !file.Exists)
            {
                return;
            }

            string path = file.FullName;
            await Task.Run(() => OpenFrom(path));
            SendSnapshot();
        }
        private void OpenFrom(string path)
        {
            Snapshot = FileDialog.OpenReadJson<Snapshot>(path);
            Snapshot.FilePath = path;
            Snapshot.InitAfterLoad();
        }
    }
}
