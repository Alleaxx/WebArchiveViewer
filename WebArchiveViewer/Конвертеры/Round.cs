using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace WebArchiveViewer
{
    public class MathRoundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double number)
            {
                int rounding = 1;
                if (parameter != null && int.TryParse(parameter.ToString(), out int numberRound))
                    rounding = numberRound;

                return Math.Round(number, rounding);
            }
            else
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }

}
