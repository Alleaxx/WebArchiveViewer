using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WebArchiveViewer
{
    public interface IRulesControl
    {
        IEnumerable<IRule> Rules { get; }
        void AddRules(IRules rules);
        string CheckLink(IArchLink link);
        string CheckLink(string link);
    }
    [Serializable]
    public class RulesControl : NotifyObj, IRulesControl
    {
        const string Undefined = "???";


        [JsonIgnore]
        public IEnumerable<IRule> Rules => MainRules;
        public ObservableCollection<GroupRule> MainRules { get; private set; } = new ObservableCollection<GroupRule>();


        protected override void InitCommands()
        {
            base.InitCommands();
            RemoveRuleCommand = new RelayCommand(RemoveRule, NotMain);
            ShowRulesCommand = new RelayCommand(ShowRules);
        }


        private bool NotMain(object obj) => obj is IRule rule && MainRules.First() != rule;
        [JsonIgnore]
        public ICommand RemoveRuleCommand { get; private set; }
        private void RemoveRule(object obj)
        {
            if(obj is IRule ruleToRemove)
            {
                foreach (var mainRule in MainRules)
                {
                    mainRule.Remove(ruleToRemove);
                }
            }
        }

        [JsonIgnore]
        public ICommand ShowRulesCommand { get; private set; }
        private void ShowRules(object obj)
        {
            RulesWindow window = new RulesWindow(this);
            window.ShowDialog();
        }



        public void AddRules(IRules rules)
        {
            MainRules.Add(rules.GetMainRule() as GroupRule);
        }
        public string CheckLink(IArchLink link)
        {
            return CheckLink(link.LinkSource);
        }
        public string CheckLink(string link)
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
    public class DefaultRules : IRules
    {
        private IRule MainRule { get; set; }
        public DefaultRules(string name)
        {
            MainRule = new GroupRule(name, "");
        }
        public IRule GetMainRule() => MainRule;
    }
    public class RumineRules : IRules
    {
        public IRule GetMainRule()
        {
            var rumineRule = new GroupRule("Все", "",
                new GroupRule("Румине", "ru-minecraft.ru",
                    new GroupRule("Главная страница", "/index.php"),
                    new GroupRule("Главная страница 2.0", "/main"),
                    new GroupRule("Выходная ссылка", "/out?a"),
                    new GroupRule("Форум", "forum",
                        new GroupRule("Форумная тема", "showtopic"),
                        new GroupRule("Раздел форума", "categories"),
                        new GroupRule("Создание темы", "newtopic"),
                        new GroupRule("Поиск на форуме", "search"),
                        new GroupRule("Отслеживаемые темы", "watched"),
                        new GroupRule("Загрузки форума", "uploads"),
                        new GroupRule("Создание сообщения", "new_post"),
                        new GroupRule("Симпатии поста", "like_all")),
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
                    new GroupRule("Раздел: достопримечательности", "/dostoprimechatelnosty-nashego-servera")));
            return rumineRule;
        }
    }



    public interface IRule
    {
        string GroupName { get; set; }
        string IsMatched(string link);
        string IsMatched(IArchLink link);
        IRule FindOwner(IRule rule);
        void Remove(IRule rule);
    }

    [Serializable]
    public class GroupRule : IRule
    {
        public string GroupName { get; set; }
        public string FoundText { get; set; }

        public ObservableCollection<GroupRule> Rules { get; set; }

        public GroupRule() : this("-","")
        {

        }
        public GroupRule(string name, string text, params GroupRule[] rules)
        {
            GroupName = name;
            FoundText = text;
            Rules = new ObservableCollection<GroupRule>(rules);

            AddRuleCommand = new RelayCommand(AddRule);
        }


        [JsonIgnore]
        public ICommand AddRuleCommand { get; private set; }
        private void AddRule(object obj)
        {
            Rules.Insert(0, new GroupRule());
        }



        public string IsMatched(IArchLink link)
        {
            return IsMatched(link.LinkSource);
        }
        public string IsMatched(string link)
        {
            if (link.Contains(FoundText))
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
        public IRule FindOwner(IRule rule)
        {
            throw new NotImplementedException();
        }

        public void Remove(IRule ruleToRemove)
        {
            Rules.Remove(ruleToRemove as GroupRule);
            foreach (var rule in Rules)
            {
                rule.Remove(ruleToRemove);
            }
        }
    }
}
