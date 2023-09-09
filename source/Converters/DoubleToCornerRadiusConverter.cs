using Extras.Extensions;
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
            var radius = System.Convert.ToDouble(value, CultureInfo.InvariantCulture);

            if (parameter is string factorsString)
            {
                var factors = factorsString.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToDoubles().ToList();
                if (factors.Count() == 4)
                {
                    if (targetType == typeof(CornerRadius))
                    {
                        return new CornerRadius(radius * factors[0], radius * factors[1], radius * factors[2], radius * factors[3]);
                    }
                    if (targetType == typeof(Thickness))
                    {
                        return new Thickness(radius * factors[0], radius * factors[1], radius * factors[2], radius * factors[3]);
                    }
                }
                if (factors.Count() == 1)
                {
                    if (targetType == typeof(CornerRadius))
                    {
                        return new CornerRadius(radius * factors[0]);
                    }
                    if (targetType == typeof(Thickness))
                    {
                        return new Thickness(radius * factors[0]);
                    }
                }
            }

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
