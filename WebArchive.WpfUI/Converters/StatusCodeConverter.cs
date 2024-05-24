using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace WebArchiveViewer.WpfUI.Converters
{
    public class StatusCodeColorConverter : IValueConverter
    {
        private string UndefinedColor { get; set; }
        private IEnumerable<StatusCodeColor> CodeColors { get; set; }

        public StatusCodeColorConverter()
        {
            UndefinedColor = "black";
            CodeColors = new StatusCodeColor[]
            {
                new StatusCodeColor(200, "#69ab3c"),
                new StatusCodeColor(200, 400, "#ab873c"),
                new StatusCodeColor(400, 1000, "#ab3c3c")
            };
        }


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (int.TryParse($"{value}", out int code))
            {
                StatusCodeColor matchStatus = CodeColors.FirstOrDefault(c => code > c.From && code <= c.To);
                if (matchStatus != null)
                {
                    return matchStatus.Color;
                }
            }
            return UndefinedColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }

}
