using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Extras.Converters
{
    public class IntToRatingBrushConverter : IValueConverter
    {
        static readonly LinearGradientBrush gradientBrush = new LinearGradientBrush();

        Color Lerp(in Color a, in Color b, float t)
        {
            float oneMinusT = 1f - t;
            return Color.FromScRgb(
                oneMinusT * a.ScA + t * b.ScA,
                oneMinusT * a.ScR + t * b.ScR,
                oneMinusT * a.ScG + t * b.ScG,
                oneMinusT * a.ScB + t * b.ScB
            );
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int? intValue = value as int?;
            if (intValue is null && value is string intString)
            {
                if (int.TryParse(intString, out var parsedInt))
                {
                    intValue = parsedInt;
                }
            }
            if (intValue is int actualInt)
            {
                if (actualInt >= 0 && actualInt < 33)
                {
                    var t = 1f * actualInt / 33;
                    return Lerp(Colors.OrangeRed, Colors.LightYellow, t);
                }
                if (actualInt >= 33 && actualInt < 66)
                {
                    var t = 1f * (actualInt - 33) / 33;
                    return Lerp(Colors.LightYellow, Colors.YellowGreen, t);
                }
                if (actualInt >= 66 && actualInt <= 100)
                {
                    var t = 1f * (actualInt - 66) / 34;
                    return Lerp(Colors.YellowGreen, Colors.LawnGreen, t);
                }
            }

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
