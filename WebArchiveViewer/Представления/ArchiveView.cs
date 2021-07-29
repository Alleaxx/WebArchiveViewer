using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

using Newtonsoft.Json;

namespace WebArchiveViewer
{
    class ObjNotify : INotifyPropertyChanged
    {        
        #region Интерфейс INotify
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        #endregion
    }

    //Представление просмотра ссылок с архива
    class ArchiveView : ObjNotify
    {
        public string Status
        {
            get => status;
            set
            {
                status = value;
                OnPropertyChanged();
            }
        }
        private string status;


        //Инициализация
        public ArchiveView()
        {
            Status = "Ожидание списка ссылок";
            OpenSnapshotCommand = new RelayCommand(OpenSnapshotFileExe, (object obj) => true);
            SaveSnapFileCommand = new RelayCommand(SaveSnapFileExe, IsSnapshotOpened);
            CloseSnapCommand = new RelayCommand(CloseSnapExe, IsSnapshotOpened);
            ClearProgressCommand = new RelayCommand(ClearProgress, IsSnapshotOpened);
            LoadNamesCommand = new RelayCommand(LoadNames, IsSnapshotOpened);

            Rules = new List<GroupRule>()
            {
                new GroupRule("Все","",
                    new GroupRule("Румине","ru-minecraft.ru",
                        new GroupRule("Главная страница","/index.php"),
                        new GroupRule("Главная страница 2.0","/main"),
                        new GroupRule("Выходная ссылка","/out?a"),
                        new GroupRule("Раздел: бездна","bezdna"),
                        new GroupRule("Раздел: читы","cheats"),
                        new GroupRule("Раздел: Клиенты","klienty-minecraft"),
                        new GroupRule("Раздел: Карты","karty-dlja-minecraft"),
                        new GroupRule("Раздел: инструкции и гайды","instrukcii-gajdy-minecraft"),
                        new GroupRule("Раздел: файлы для майнкрафт","fayly-dlya-minecraft"),
                        new GroupRule("Раздел: файлы для майнкрафт","fayly-dlya-minecraft"),
                        new GroupRule("Раздел: похожие игры","igry-pohozhie-na-minecraft"),
                        new GroupRule("Раздел: моды","mody-minecraft"),
                        new GroupRule("Раздел: текстур-паки","tekstur-paki"),
                        new GroupRule("Раздел: текстур-паки 2.0","tekstur-paki-minecraft"),
                        new GroupRule("Раздел: текстур-паки PE","tekstury-minecraft-pe"),
                        new GroupRule("Раздел: моды PE","mods-minecraft-pe"),
                        new GroupRule("Раздел: карты PE","karty-minecraft-pe"),
                        new GroupRule("Раздел: ресурс-паки","resource-packs-minecraft"),
                        new GroupRule("Раздел: скины майнкрафт","skiny-dlja-minecraft"),
                        new GroupRule("Раздел: плагины","plaginy-minecraft"),
                        new GroupRule("Раздел: разное","raznoe"),
                        new GroupRule("Раздел: PE","/minecraft-pe"),
                        new GroupRule("Раздел: PE 2.0","/minecraft-pocket-edition"),
                        new GroupRule("Раздел: новости сервера","novosti-servera"),
                        new GroupRule("Раздел: новости майнкрафт","novosti-minecraft"),
                        new GroupRule("Раздел: видео","video-minecraft"),
                        new GroupRule("Раздел: лецплаи","lets-play-minecraft"),
                        new GroupRule("Раздел: хостинг","hosting-minecraft"),
                        new GroupRule("Раздел: лаунчеры","/launcher"),
                        new GroupRule("Скачать майнкрафт","skachat-minecraft"),
                        new GroupRule("Страница: купить майнкрафт","kupit-minecraft"),
                        new GroupRule("Страница: FAQ","/faq"),
                        new GroupRule("Страница: правила","/rules"),
                        new GroupRule("Страница: id предметов","/id-predmetov-minecraft"),
                        new GroupRule("Страница: добавить новость","/addnews.html"),
                        new GroupRule("Страница: комиксы майнкрафт","/komiksy-minecraft"),
                        new GroupRule("Страница: крафтинг в майнкрафт","/krafting-v-minecraft"),
                        new GroupRule("Страница: купить вип","/kupit-vip"),
                        new GroupRule("Админпанель","/admin.php"),
                        new GroupRule("Скины","skins/"),
                        new GroupRule("Раздел: сборки серверов","skachat-servera-minecraft"),
                        new GroupRule("Создать скин майнкрафт","sozdat-skin-dlya-minecraft"),
                        new GroupRule("Скачать минекрафт PE","/download-pe"),
                        new GroupRule("Скачать минекрафт","/skachat-maynkraft"),
                        new GroupRule("Загрузить...","/download/"),
                        new GroupRule("Загрузки","uploads"),
                        new GroupRule("Избранное","favorites"),
                        new GroupRule("Профиль пользователя","user/"),
                        new GroupRule("Чат сайта","dlechat"),
                        new GroupRule("Движок","engine/"),
                        new GroupRule("Ачивки","achievements/"),
                        new GroupRule("Репутация","/reputation"),
                        new GroupRule("Раздел: сиды","/seeds"),
                        new GroupRule("Шаблоны","/templates"),
                        new GroupRule("Раздел: достопримечательности","/dostoprimechatelnosty-nashego-servera"),
                        new GroupRule("Форум","forum",
                            
                            new GroupRule("Форумная тема","showtopic"),
                            new GroupRule("Раздел форума","categories"),
                            new GroupRule("Создание темы","newtopic"),
                            new GroupRule("Поиск на форуме","search"),
                            new GroupRule("Отслеживаемые темы","watched"),
                            new GroupRule("Загрузки форума","uploads"),
                            new GroupRule("Создание сообщения","new_post"),
                            new GroupRule("Симпатии поста","like_all")))),
                        new GroupRule("Страница","/html"),
            };

        }


        private bool IsSnapshotOpened(object obj) => CurrentSnapshot != null;
        //Открытие уже имеющегося файла со ссылками
        private void OpenSnapshotFileExe(object obj)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.DefaultExt = ".json";
            dialog.Filter = "JSON-файл (*.json) |*.json";
            if (dialog.ShowDialog() == true)
            {
                OpenSnapshotFile(dialog.FileName);
                Status = "Ссылки из файла";
                OpenedFilePath = new FileInfo(dialog.FileName).Name;
                OnPropertyChanged(nameof(OpenedFilePath));
            }
        }
        public RelayCommand OpenSnapshotCommand { get; set; }

        private void OpenSnapshotFile(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                // преобразуем строку в байты
                byte[] array = new byte[fs.Length];
                // считываем данные
                fs.Read(array, 0, array.Length);
                // декодируем байты в строку
                string textFromFile = Encoding.Default.GetString(array);
                SiteSnapshot snap = JsonConvert.DeserializeObject<SiteSnapshot>(textFromFile);
                CurrentSnapshot = snap;
            }

        }

        public string OpenedFilePath { get; set; }

        //Сохранение отображаемых ссылок как отчета
        enum SaveMode
        {
            allShowed,
            allFiltered,
            all,
        }
        private void SaveSnapFileExe(object obj)
        {
            SaveMode saveMode = SaveMode.allShowed;
            if (obj != null && Enum.TryParse(obj.ToString(), out SaveMode mode))
                saveMode = mode;
            ArchiveLink[] links = null;
            switch (saveMode)
            {
                case SaveMode.allShowed:
                    links = LinksPager.PageNow.Elements;
                    break;
                case SaveMode.all:
                    links = CurrentSnapshot.Links;
                    break;
                case SaveMode.allFiltered:
                    links = CurrentSnapshot.Links.Where(l => Filter(l)).ToArray();
                    break;
            }


            Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog();
            SiteSnapshot copy = new SiteSnapshot()
            {
                SourceURI = CurrentSnapshot.SourceURI,
                Date = DateTime.Now,
                ViewOptions = new LinksViewOptionsGet()
                {
                    From = Options.From,
                    To = Options.To,
                    Search = Options.Search,
                    Codes = Options.Codes,
                    Types = Options.Types,
                },
                Links = links
            };

            dialog.FileName = $"{copy.Links.Length} - {copy.SourceURI}";
            dialog.DefaultExt = ".json";
            dialog.InitialDirectory = Directory.GetCurrentDirectory();
            dialog.Filter = "JSON-файл (*.json) |*.json";
            if (dialog.ShowDialog() == true)
            {
                copy.Save(dialog.FileName);
            }
        }
        public RelayCommand SaveSnapFileCommand { get; set; }

        //Сохранение отображаемых ссылок как отчета
        private void CloseSnapExe(object obj)
        {
            CurrentSnapshot = null;
            LinksPager = null;
        }
        public RelayCommand CloseSnapCommand { get; set; }
        
        
        public RelayCommand ClearProgressCommand { get; set; }
        private void ClearProgress(object obj)
        {
            foreach (ArchiveLink link in CurrentSnapshot.Links)
            {
                link.ActualState = null;
            }
            UpdateList();
        }
        
        
        public RelayCommand LoadNamesCommand { get; set; }
        private void LoadNames(object obj)
        {
            FileInfo[] files = new DirectoryInfo(@"D:\Users\Allexx\Documents\Румайн\Проекты\История Румине\ProjectArch 2.0\Форум Румине\Темы [28527 - 28527]").GetFiles();
            int counter = 0;
            var links = currentSnapshot.Links.ToList();
            foreach (var file in files)
            {
                string html = File.ReadAllText(file.FullName);
                int pos = html.IndexOf("__wm.wombat");
                string funcText = html.Substring(pos, html.IndexOf(";",pos) - pos);
                var funcParams = funcText.Split('"');

                string Link = funcParams[1];
                string name = $"{html.Substring(html.IndexOf("<title>"),html.IndexOf("</title>") - html.IndexOf("<title>"))}";
                var l1 = links.Find(l => l.LinkSource == Link);
                if(l1 != null)
                    l1.Name = name;

                if (counter > 500)
                    break;
            }
            UpdateList();
        }



        //Текущий загруженный снапшот сайта
        public SiteSnapshot CurrentSnapshot
        {
            get => currentSnapshot;
            set
            {
                currentSnapshot = value;
                OnPropertyChanged();
                if (value != null)
                {
                    Options = new LinksViewOptionsView(this, value);
                    CategoriesFound = new List<OptionCount<string>>();
                    foreach (ArchiveLink link in value.Links)
                    {
                        string category = Rules.First().IsMatched(link);
                        link.Category = category;
                        OptionCount<string> cate = CategoriesFound.Find(o => o.Value == category);

                        if (cate == null)
                            CategoriesFound.Add(new OptionCount<string>(category, true));
                        else if (cate != null)
                            cate.Amount++;
                    }
                    OnPropertyChanged(nameof(CategoriesFound));

                    UpdateList();
                    Status = "Ссылки загружены";
                }
                else
                {
                    Options = null;
                    Status = "Ссылки закрыты";
                }
            }
        }
        private SiteSnapshot currentSnapshot;

        //Опции просмотра снапшота
        public LinksViewOptionsView Options
        {
            get => options;
            set
            {
                options = value;
                OnPropertyChanged();
            }
        }
        private LinksViewOptionsView options;

        public Pager<ArchiveLink> LinksPager
        {
            get => linksPager;
            set
            {
                linksPager = value;
                OnPropertyChanged();
            }
        }
        private Pager<ArchiveLink> linksPager;
        public int LinksFilteredAmount { get; set; }

        //Обновление списка ссылок
        public void UpdateList()
        {
            List<ArchiveLink> links = currentSnapshot.Links.Where(l => Filter(l)).ToList();
            if (SortSelected != null)
            {
                switch (SortSelected.Value)
                {
                    case "Имя":
                        links = links.OrderBy(l => l.LinkSource).ToList();
                        break;
                    case "Дата":
                        links = links.OrderBy(l => l.Date).ToList();
                        break;
                    case "Тип":
                        links = links.OrderBy(l => l.MimeType).ToList();
                        break;
                }
            }

            LinksFilteredAmount = links.Count;
            OnPropertyChanged(nameof(LinksFilteredAmount));

            LinksPager = new Pager<ArchiveLink>(links);
        }
        private bool Filter(object obj)
        {
            ArchiveLink link = obj as ArchiveLink;
            if (Options.From.Year > 1990 && (link.Date < Options.From || link.Date > Options.To))
                return false;
            if (!Options.Codes.ToList().Find(opt => opt.Value == link.StatusCode).Enabled)
                return false;
            if (!Options.Types.ToList().Find(opt => opt.Value == link.MimeType).Enabled)
                return false;
            if (!CategoriesFound.Find(opt => opt.Value == link.Category).Enabled)
                return false;
            if (Options.ShowLoaded.HasValue && ((!Options.ShowLoaded.Value && link.ActualState == "200") || (Options.ShowLoaded.Value && link.ActualState == null)))
                return false;
            if (!string.IsNullOrEmpty(Options.Search) && !link.LinkSource.Contains(Options.Search))
                return false;
            return true;
        }



        //Сортировка и группировка
        public List<GroupRule> Rules { get; set; }

        public List<OptionCount<string>> CategoriesFound { get; set; }
        public List<Option<string>> Groups { get; set; } = new List<Option<string>>()
        {
            new Option<string>("Тип",false),
            new Option<string>("Код",false),
            new Option<string>("Категория",false),
            new Option<string>("Нет", true)
        };
        public List<Option<string>> Sorts { get; set; } = new List<Option<string>>()
        {
            new Option<string>("Дата", false),
            new Option<string>("Имя", false),
            new Option<string>("Тип", false),
            new Option<string>("Нет", true)
        };

        //Выбранные сортировки и группировки
        public Option<string> SortSelected
        {
            get => sortSelected;
            set
            {
                sortSelected = value;
                OnPropertyChanged();
                UpdateList();
            }
        }
        private Option<string> sortSelected;
    }
}
