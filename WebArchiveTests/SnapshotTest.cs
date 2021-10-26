using WebArchiveViewer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.IO;
using System.Linq;

using WebArchive.Data;
namespace WebArchive.Tests
{
    [TestClass]
    public class SnapshotTest
    {
        private Snapshot SnapExample { get; set; }
        private string OpenPath { get; set; }
        public SnapshotTest()
        {
            ArchiveLink[] links = new ArchiveLink[]
            {
                new ArchiveLink(){ LinkSource = "http://ru-minecraft.ru:80/forum", TimeStamp="20111018031435", StatusCode="200", MimeType="text/html" },
                new ArchiveLink(){ LinkSource = "http://ru-minecraft.ru:80/forum", TimeStamp="20111217112750", StatusCode="302", MimeType="text/html" },
            };
            SnapExample = new Snapshot("norequest", "http://ru-minecraft.ru/forum", links);
            SnapExample.UpdateLinkCategories();

            OpenPath = Directory.GetCurrentDirectory() + "\\test-open.json";
        }

        //������ ���� � �������������� ��������
        [TestMethod]
        public void NewSnapshotFilePathIsEmpty()
        {
            Snapshot snap = new Snapshot();
            Assert.AreEqual(null, snap.FilePath);
        }

        //�������� �������� �������� �� �����
        [TestMethod]
        public void OpenSnapshot()
        {
            IFileDialog dialog = new FileDialog();
            if (File.Exists(OpenPath))
            {
                Snapshot snapshot = dialog.OpenReadJson<Snapshot>(OpenPath);
                Assert.AreEqual(2, snapshot.Links.Length, "����������� ����������� ���� �������� (�������� ���������� ������)");
            }
            else
            {
                Assert.Fail($"���� ��� �������� �������� �������� �� ������: {OpenPath}");
            }
        }

        //�������� ����������� ���������
        [TestMethod]
        public void CategoryCorrect()
        {
            var link = SnapExample.Links[0];
            Assert.AreEqual("�����", link.Category, "��������� ������������ �������");
        }
    }
}
