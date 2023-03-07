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
    public class ColumnWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool flag)
            {
                if (flag)
                {
                    return 250;
                }
                else
                {
                    return 0;
                }
            }
            return 250;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
    public class ColumnWidthV2Converter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double width = 250;
            if(parameter != null)
            {
                if(double.TryParse(parameter.ToString(), out double res))
                {
                    width = res;
                }
                else if(parameter is string str && str == "Auto")
                {
                    width = double.NaN;
                }
            }

            if (value is bool flag)
            {
                if (!flag)
                {
                    return width;
                }
                else
                {
                    return 0;
                }
            }
            return width;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
