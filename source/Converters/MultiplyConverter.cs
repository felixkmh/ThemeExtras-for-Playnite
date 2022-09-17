using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Extras.Converters
{
    public class MultiplyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (System.Convert.ToDouble(value) is double b && System.Convert.ToDouble(parameter) is double e)
            {
                return System.Convert.ChangeType(b * e, targetType);
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (System.Convert.ToDouble(value) is double b && System.Convert.ToDouble(parameter) is double e)
            {
                return System.Convert.ChangeType(b / e, targetType);
            }
            return DependencyProperty.UnsetValue;
        }
    }
}
