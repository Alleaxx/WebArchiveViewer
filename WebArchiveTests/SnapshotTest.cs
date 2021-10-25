using WebArchiveViewer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.IO;
using System.Linq;

namespace WebArchiveTests
{
    [TestClass]
    public class SnapshotTest
    {
        private Snapshot SnapExample { get; set; }
        private string OpenPath { get; set; }
        private string SavePath { get; set; }
        public SnapshotTest()
        {
            ArchiveLink[] links = new ArchiveLink[]
            {
                new ArchiveLink(){ LinkSource = "http://ru-minecraft.ru:80/forum", TimeStamp="20111018031435", StatusCode="200", MimeType="text/html" },
                new ArchiveLink(){ LinkSource = "http://ru-minecraft.ru:80/forum", TimeStamp="20111217112750", StatusCode="302", MimeType="text/html" },
            };
            SnapExample = new Snapshot("norequest", "http://ru-minecraft.ru/forum", links);

            OpenPath = Directory.GetCurrentDirectory() + "\\test-open.json";
            SavePath = Directory.GetCurrentDirectory() + "\\test-save.json";
        }



        //-������� ���� � ������������ ����������, ��������� ���������� ������


        //������ ���� � �������������� ��������
        [TestMethod]
        public void NewSnapshotFilePathIsEmpty()
        {
            Snapshot snap = new Snapshot();
            Assert.AreEqual(null, snap.FilePath);
        }


        //-- ��������� ����
        //�������� ��������� ������ �����
        //�������� ��������� �������� ���� � ������������ ��������
        [TestMethod]
        public void SaveSnapshot()
        {
            throw new System.Exception("��� ��� ����� ������");

            //if (File.Exists(SavePath))
            //    File.Delete(SavePath);

            //SnapExample.SavingCopy(SavePath);
            //FileInfo file = new FileInfo(SavePath);
            //Assert.IsTrue(file.Exists, "������������ ����� �� ����������");
            //Assert.AreEqual(SavePath, SnapExample.FilePath, "����������� ������� �� �������� ����");
        }

        //�������� �������� �������� �� �����
        [TestMethod]
        public void OpenSnapshot()
        {
            IFileDialog dialog = new FileDialog();
            if (File.Exists(OpenPath))
            {
                Snapshot snapshot = dialog.OpenReadJson<Snapshot>(OpenPath);
                //snapshot.ViewOptions = new ViewOptions(snapshot);
                Assert.AreEqual(2, snapshot.Links.Length, "����������� ����������� ���� �������� (�������� ���������� ������)");
                //Assert.AreEqual(2, snapshot.ViewOptions.Codes.Length, "����������� ����������� ���� �������� (�������� ���������� ����� ������)");
                //Assert.AreEqual(1, snapshot.ViewOptions.Types.Length, "����������� ����������� ���� �������� (�������� ���������� �����)"); ;
            }
            else
            {
                Assert.Fail($"���� ��� �������� �������� �������� �� ������: {OpenPath}");
            }
        }

        //�������� ���������� �� ����
        [TestMethod]
        public void FilterCode()
        {
            //SnapExample.ViewOptions.Codes[0].Enabled = false;
            //Assert.AreEqual(1, SnapExample.ViewOptions.GetFilteredLinks().Count(), "������ �� ����� �������� �����������");
        }

        //�������� ���������� �� ������
        [TestMethod]
        public void FilterSearch()
        {
            //SnapExample.ViewOptions.Search = "����� ������ ��� ���";
            //Assert.AreEqual(0, SnapExample.ViewOptions.GetFilteredLinks().Count(), "������ �� ������ ������ �������� �����������");
        }

        //�������� ����������� ���������
        [TestMethod]
        public void CategoryCorrect()
        {
            var link = SnapExample.Links[0];
            Assert.AreEqual("�����", link.Category, "��������� ������������ �������");
        }


        //��������� ��� ��� ��������� ������, �������������� ��� ����������� �����
        [TestMethod]
        public void NameLoadAvailable()
        {
            var link = SnapExample.Links[0];
            link.Name = null;
            var command = new LoadLink(link).LoadNameCommand;
            bool canExecute = command.CanExecute(null);

            Assert.IsTrue(canExecute, "������� �������� ����� ����������");
            command.Execute(null);
        }
    }
}
