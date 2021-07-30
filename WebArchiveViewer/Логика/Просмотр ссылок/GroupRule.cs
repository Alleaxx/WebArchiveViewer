using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchiveViewer
{
    public interface IRulesControl
    {
        List<IRule> MainRules { get; }
        void AddRules(IRules rules);
        string CheckLink(ArchiveLink link);
    }
    [Serializable]
    public class RulesControl : IRulesControl
    {
        public event Action<IRule> RuleChanged;

        public List<IRule> MainRules { get; private set; } = new List<IRule>();
        const string Undefined = "???";

        public void AddRules(IRules rules)
        {
            MainRules.Add(rules.GetMainRule());
        }
        public string CheckLink(ArchiveLink link)
        {
            foreach (var rule in MainRules)
            {
                var matchResult = rule.IsMatched(link);
                if (matchResult != null)
                    return matchResult;
            }
            return Undefined;
        }
    }
    public interface IRules
    {
        IRule GetMainRule();
    }
    class RumineRules : IRules
    {
        public IRule GetMainRule()
        {
            var rumineRule = new GroupRule("Все", "",
                new GroupRule("Румине", "ru-minecraft.ru",
                    new GroupRule("Главная страница", "/index.php"),
                    new GroupRule("Главная страница 2.0", "/main"),
                    new GroupRule("Выходная ссылка", "/out?a"),
                    new GroupRule("Раздел: бездна", "bezdna"),
                    new GroupRule("Раздел: читы", "cheats"),
                    new GroupRule("Раздел: Клиенты", "klienty-minecraft"),
                    new GroupRule("Раздел: Карты", "karty-dlja-minecraft"),
                    new GroupRule("Раздел: инструкции и гайды", "instrukcii-gajdy-minecraft"),
                    new GroupRule("Раздел: файлы для майнкрафт", "fayly-dlya-minecraft"),
                    new GroupRule("Раздел: файлы для майнкрафт", "fayly-dlya-minecraft"),
                    new GroupRule("Раздел: похожие игры", "igry-pohozhie-na-minecraft"),
                    new GroupRule("Раздел: моды", "mody-minecraft"),
                    new GroupRule("Раздел: текстур-паки", "tekstur-paki"),
                    new GroupRule("Раздел: текстур-паки 2.0", "tekstur-paki-minecraft"),
                    new GroupRule("Раздел: текстур-паки PE", "tekstury-minecraft-pe"),
                    new GroupRule("Раздел: моды PE", "mods-minecraft-pe"),
                    new GroupRule("Раздел: карты PE", "karty-minecraft-pe"),
                    new GroupRule("Раздел: ресурс-паки", "resource-packs-minecraft"),
                    new GroupRule("Раздел: скины майнкрафт", "skiny-dlja-minecraft"),
                    new GroupRule("Раздел: плагины", "plaginy-minecraft"),
                    new GroupRule("Раздел: разное", "raznoe"),
                    new GroupRule("Раздел: PE", "/minecraft-pe"),
                    new GroupRule("Раздел: PE 2.0", "/minecraft-pocket-edition"),
                    new GroupRule("Раздел: новости сервера", "novosti-servera"),
                    new GroupRule("Раздел: новости майнкрафт", "novosti-minecraft"),
                    new GroupRule("Раздел: видео", "video-minecraft"),
                    new GroupRule("Раздел: лецплаи", "lets-play-minecraft"),
                    new GroupRule("Раздел: хостинг", "hosting-minecraft"),
                    new GroupRule("Раздел: лаунчеры", "/launcher"),
                    new GroupRule("Скачать майнкрафт", "skachat-minecraft"),
                    new GroupRule("Страница: купить майнкрафт", "kupit-minecraft"),
                    new GroupRule("Страница: FAQ", "/faq"),
                    new GroupRule("Страница: правила", "/rules"),
                    new GroupRule("Страница: id предметов", "/id-predmetov-minecraft"),
                    new GroupRule("Страница: добавить новость", "/addnews.html"),
                    new GroupRule("Страница: комиксы майнкрафт", "/komiksy-minecraft"),
                    new GroupRule("Страница: крафтинг в майнкрафт", "/krafting-v-minecraft"),
                    new GroupRule("Страница: купить вип", "/kupit-vip"),
                    new GroupRule("Админпанель", "/admin.php"),
                    new GroupRule("Скины", "skins/"),
                    new GroupRule("Раздел: сборки серверов", "skachat-servera-minecraft"),
                    new GroupRule("Создать скин майнкрафт", "sozdat-skin-dlya-minecraft"),
                    new GroupRule("Скачать минекрафт PE", "/download-pe"),
                    new GroupRule("Скачать минекрафт", "/skachat-maynkraft"),
                    new GroupRule("Загрузить...", "/download/"),
                    new GroupRule("Загрузки", "uploads"),
                    new GroupRule("Избранное", "favorites"),
                    new GroupRule("Профиль пользователя", "user/"),
                    new GroupRule("Чат сайта", "dlechat"),
                    new GroupRule("Движок", "engine/"),
                    new GroupRule("Ачивки", "achievements/"),
                    new GroupRule("Репутация", "/reputation"),
                    new GroupRule("Раздел: сиды", "/seeds"),
                    new GroupRule("Шаблоны", "/templates"),
                    new GroupRule("Раздел: достопримечательности", "/dostoprimechatelnosty-nashego-servera"),
                    new GroupRule("Форум", "forum",
                        new GroupRule("Форумная тема", "showtopic"),
                        new GroupRule("Раздел форума", "categories"),
                        new GroupRule("Создание темы", "newtopic"),
                        new GroupRule("Поиск на форуме", "search"),
                        new GroupRule("Отслеживаемые темы", "watched"),
                        new GroupRule("Загрузки форума", "uploads"),
                        new GroupRule("Создание сообщения", "new_post"),
                        new GroupRule("Симпатии поста", "like_all"))));
            return rumineRule;
        }
    }




    public interface IRule
    {
        string GroupName { get; set; }
        string IsMatched(ArchiveLink link);
    }
    class GroupRule : IRule
    {
        public string GroupName { get; set; }
        public string FoundText { get; set; }

        public List<IRule> Rules { get; set; }

        public GroupRule(string name, string text, params IRule[] rules)
        {
            GroupName = name;
            FoundText = text;
            Rules = new List<IRule>(rules);
        }

        public string IsMatched(ArchiveLink link)
        {
            if (IsLinkHasText(link))
            {
                if (Rules.Count == 0)
                {
                    return GroupName;
                }
                else
                {
                    foreach (IRule rule in Rules)
                    {
                        if (rule.IsMatched(link) is string res && !string.IsNullOrEmpty(res))
                            return res;
                    }
                }
                return GroupName;
            }
            return null;
        }
        private bool IsLinkHasText(ArchiveLink link) => link.LinkSource.Contains(FoundText);
    }
}
