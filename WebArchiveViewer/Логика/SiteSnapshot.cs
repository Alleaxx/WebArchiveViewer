using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace WebArchiveViewer
{
    //Сайт со ссылками из архива на указанную дату
    [Serializable]
    class SiteSnapshot
    {
        public string SourceURI { get; set; }
        public DateTime Date { get; set; }
        public LinksViewOptionsGet ViewOptions { get; set; }
        public ArchiveLink[] Links { get; set; }

        public SiteSnapshot()
        {

        }
        public bool Save(string path)
        {            
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            //Запись
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                byte[] arr = Encoding.Default.GetBytes(json);
                fs.Write(arr,0,arr.Length);
                return true;
            }
        }
    }

    //Ссылка в веб архиве на сайт
    [Serializable]
    class ArchiveLink
    {
        public string Name { get; set; } = "Имя ссылки";
        public int Index { get; set; }
        public string TimeStamp { get; set; }
        public DateTime Date { get; set; }
        public string LinkSource { get; set; }
        public string Link => $"https://web.archive.org/web/{TimeStamp}/{LinkSource}";
        public string MimeType { get; set; }
        public string StatusCode { get; set; }

        [JsonIgnore]
        public string Category { get; set; } = "Общее";
        public string ActualState { get; set; }

        public string SetRumineCategory()
        {
            //Regex mainForum = new Regex(@"https?:\\\\ru-minecraft.ru");
            //if (mainForum.Match(Original).Value == Original)
            //    Category = "Главная форума";
            if (LinkSource.EndsWith(".html"))
                Category = "Страница сайта";
            if (LinkSource.Contains("php"))
                Category = "php-скрипты";

            if (LinkSource.Contains("/forum"))
            {
                Category = "Форум";
                if (LinkSource.Contains("showtopic"))
                    Category = "Форумная тема";
                if (LinkSource.Contains("search") || LinkSource.Contains("watched") || LinkSource.Contains("newtopic") || LinkSource.Contains("new_post"))
                    Category = "Служебная форума";
                if (LinkSource.Contains("categories"))
                    Category = "Раздел форума";
                if (LinkSource.Contains("like_all"))
                    Category = "Симпатии поста";
            }

            if (LinkSource.Contains("/user/"))
            {
                Category = "Профиль пользователя";
                if (LinkSource.Contains("news"))
                    Category = "Новости пользователя";
                if (LinkSource.Contains("message"))
                    Category = "Сообщения пользователя";
                if (LinkSource.Contains("reputation"))
                    Category = "Репутация пользователя";
            }
            if (LinkSource.Contains("lastcomments"))
                Category = "Последние комментарии юзера";

            if (LinkSource.Contains("out?a") ||LinkSource.Contains("leech_out"))
                Category = "Внешняя ссылка";
            if (LinkSource.Contains("dlechat"))
                Category = "Чат сайта";

            return Category;
        }

    }


    public class StatusCodeColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string text = (string)value;
            if (text == "-")
                    return "yellow";

                if (text == "200")
                    return "green";
                if (System.Convert.ToInt32(text) > 200 && System.Convert.ToInt32(text) < 400)
                    return "#FFE2A400";
                if (System.Convert.ToInt32(text) > 400)
                    return "red";

                return "black";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
    public class DateTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime date = (DateTime)value;
            string param = (string)parameter;
            if (string.IsNullOrEmpty(param))
                param = "dd MMMM yyyy, HH:mm";
            return date.ToString(param);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
