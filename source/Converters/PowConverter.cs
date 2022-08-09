using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Extras.Converters
{
    public class PowConverter : IValueConverter
    {
        public static PowConverter Instance = new PowConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (System.Convert.ToDouble(value) is double b && System.Convert.ToDouble(parameter) is double e)
            {
                return System.Convert.ChangeType(Math.Pow(b, e), targetType);
            }
            throw new NotSupportedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (System.Convert.ToDouble(value) is double b && System.Convert.ToDouble(parameter) is double e)
            {
                return System.Convert.ChangeType(Math.Pow(b, e/1.0), targetType);
            }
            throw new NotSupportedException();
        }
    }
}
