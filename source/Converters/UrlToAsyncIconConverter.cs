using PlayniteCommon.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Extras.Converters
{
    public class UrlToAsyncIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string url = null;
            switch (value)
            {
                case string valueString:
                    url = valueString;
                    break;
                case Uri valueUri:
                    url = valueUri.ToString();
                    break;
                default:
                    break;
            }
            if (!string.IsNullOrEmpty(url))
            {
                return new AsyncValue<object>(async () => await Models.LinkExt.GetIconAsync(url));
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
