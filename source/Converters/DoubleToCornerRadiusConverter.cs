using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Extras.Converters
{
    public class DoubleToCornerRadiusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Dock? side = null;
            if (parameter is Dock)
            {
                side = (Dock)parameter;
            }
            if (parameter is string dockString)
            {
                if (Enum.TryParse<Dock>(dockString, out var parsed))
                {
                    side = parsed;
                }
            }

            var radius = System.Convert.ToDouble(value, CultureInfo.InvariantCulture);

            switch (side)
            {
                case Dock.Left:
                    return new CornerRadius(radius, 0, 0, radius);
                case Dock.Top:
                    return new CornerRadius(radius, radius, 0, 0);
                case Dock.Right:
                    return new CornerRadius(0, radius, radius, 0);
                case Dock.Bottom:
                    return new CornerRadius(0, 0, radius, radius);
                default:
                    return new CornerRadius(radius);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
