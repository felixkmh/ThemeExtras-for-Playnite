using Extras.ViewModels.Objects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Extras.Converters
{
    public class DoubleToSmoothedValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is FrameworkElement element && parameter is string optionsString)
            {
                var options = optionsString.Split(',');
                string bindingPath = null;
                var duration = new Duration(TimeSpan.FromSeconds(0.15));
                if (options.Length > 0)
                {
                    bindingPath = options[0].Trim();
                }
                if (options.Length > 1)
                {
                    if (double.TryParse(options[1].Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out var seconds))
                    {
                        duration = new Duration(TimeSpan.FromSeconds(seconds));
                    }
                }
                if (!string.IsNullOrEmpty(bindingPath))
                {
                    var smoothValue = new SmoothedValue(element, bindingPath, duration);
                    return smoothValue;
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
