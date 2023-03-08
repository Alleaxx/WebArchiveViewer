using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WebArchiveViewer.UI.Converters
{
    //значение = value / arg1 * arg2
    internal class DivideMultiplyConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var value = System.Convert.ToDouble(values[0]);
                var x = System.Convert.ToDouble(values[1]);
                var y = System.Convert.ToDouble(values[2]);

                return value / x * y;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return 0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
