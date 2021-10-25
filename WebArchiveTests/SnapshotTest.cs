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



        //-Открыть файл с некорректным содержимым, проверить отмененный запрос


        //Пустой путь у новосозданного снапшота
        [TestMethod]
        public void NewSnapshotFilePathIsEmpty()
        {
            Snapshot snap = new Snapshot();
            Assert.AreEqual(null, snap.FilePath);
        }


        //-- СОХРАНИТЬ ФАЙЛ
        //Проверка появления нового файла
        //Проверка установка свойства пути у сохраненного снапшота
        [TestMethod]
        public void SaveSnapshot()
        {
            throw new System.Exception("Про это место забыли");

            //if (File.Exists(SavePath))
            //    File.Delete(SavePath);

            //SnapExample.SavingCopy(SavePath);
            //FileInfo file = new FileInfo(SavePath);
            //Assert.IsTrue(file.Exists, "Сохраненного файла не существует");
            //Assert.AreEqual(SavePath, SnapExample.FilePath, "Сохраненный снапшот не сохранил путь");
        }

        //Проверка открытия снапшота из файла
        [TestMethod]
        public void OpenSnapshot()
        {
            IFileDialog dialog = new FileDialog();
            if (File.Exists(OpenPath))
            {
                Snapshot snapshot = dialog.OpenReadJson<Snapshot>(OpenPath);
                //snapshot.ViewOptions = new ViewOptions(snapshot);
                Assert.AreEqual(2, snapshot.Links.Length, "Некорректно открывается файл снапшота (неверное количество ссылок)");
                //Assert.AreEqual(2, snapshot.ViewOptions.Codes.Length, "Некорректно открывается файл снапшота (неверное количество кодов ответа)");
                //Assert.AreEqual(1, snapshot.ViewOptions.Types.Length, "Некорректно открывается файл снапшота (неверное количество типов)"); ;
            }
            else
            {
                Assert.Fail($"Файл для проверки открытия снапшота не найден: {OpenPath}");
            }
        }

        //Проверка фильтрации по коду
        [TestMethod]
        public void FilterCode()
        {
            //SnapExample.ViewOptions.Codes[0].Enabled = false;
            //Assert.AreEqual(1, SnapExample.ViewOptions.GetFilteredLinks().Count(), "Фильтр по кодам работает некорректно");
        }

        //Проверка фильтрации по поиску
        [TestMethod]
        public void FilterSearch()
        {
            //SnapExample.ViewOptions.Search = "такой строки там нет";
            //Assert.AreEqual(0, SnapExample.ViewOptions.GetFilteredLinks().Count(), "Фильтр по строке поиска работает некорректно");
        }

        //Проверка выставления категории
        [TestMethod]
        public void CategoryCorrect()
        {
            var link = SnapExample.Links[0];
            Assert.AreEqual("Форум", link.Category, "Категория выставляется неверно");
        }


        //Загрузить имя для указанной ссылки, удостовериться что загружается верно
        [TestMethod]
        public void NameLoadAvailable()
        {
            var link = SnapExample.Links[0];
            link.Name = null;
            var command = new LoadLink(link).LoadNameCommand;
            bool canExecute = command.CanExecute(null);

            Assert.IsTrue(canExecute, "Команда загрузки имени недоступна");
            command.Execute(null);
        }
    }
}
