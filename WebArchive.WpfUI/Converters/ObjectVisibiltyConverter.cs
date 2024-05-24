using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;

namespace WebArchiveViewer.WpfUI.Converters
{
    public class ObjectVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility visibility = Visibility.Visible;

            if (value == null)
                visibility = Visibility.Collapsed;
            else if (value is int number && number == 0)
                visibility = Visibility.Collapsed;
            else if (value is string str && string.IsNullOrEmpty(str))
                visibility = Visibility.Collapsed;
            else if (value is bool YesOrNo && !YesOrNo)
                visibility = Visibility.Collapsed;

            if (parameter != null && bool.TryParse(parameter.ToString(), out bool res) && res)
            {
                if (visibility == Visibility.Collapsed)
                    visibility = Visibility.Visible;
                else
                    visibility = Visibility.Collapsed;
            }

            return visibility;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
