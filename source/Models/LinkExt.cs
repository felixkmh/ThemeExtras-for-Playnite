using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Extras.Models
{
    public class LinkExt : Link
    {
        private static Dictionary<string, object> iconCache = new Dictionary<string, object>();

        private const string WebsiteIconResourcePrefix = "ThemeExtrasWebIcon_";

        public object Icon => GetIcon();

        public ICommand OpenLinkCommand => new RelayCommand(() => { System.Diagnostics.Process.Start(Url); });

        private object GetIcon()
        {
            var uri = new Uri(Url);

            var domain = uri.Host;

            object icon = null;

            while(domain.Contains("."))
            {
                if (iconCache.TryGetValue(domain, out icon))
                {
                    switch (icon)
                    {
                        case BitmapImage bitmapImage:
                            return new Image() { Source = bitmapImage };
                        case string iconString:
                            return new TextBlock() { Text = iconString, FontFamily = ResourceProvider.GetResource("FontIcoFont") as FontFamily };
                        default:
                            return null;
                    }
                }

                if (icon is null)
                {
                    var key = WebsiteIconResourcePrefix + domain;
                    switch (ResourceProvider.GetResource(key))
                    {
                        case char iconChar:
                            icon = iconChar.ToString();
                            break;
                        case string iconString:
                            icon = iconString;
                            break;
                        default:
                            break;
                    }
                }


                if (icon is null)
                {
                    if (ExtendedTheme.Current?.ThemeExtrasManifest?.WebsiteIconsPath is string path)
                    {
                        var fullPath = Path.Combine(ExtendedTheme.Current.RootPath, path);
                        var dirInfo = new DirectoryInfo(fullPath);
                        if (dirInfo.Exists)
                        {
                            var match = dirInfo.EnumerateFiles().Where(f => f.Name.StartsWith(domain)).FirstOrDefault();
                            if (match is FileInfo)
                            {
                                icon = new BitmapImage(new Uri(match.FullName));
                            }
                        }
                    }
                }

                if (icon is object)
                {
                    iconCache[uri.Host] = icon;
                    iconCache[domain] = icon;
                    switch (icon)
                    {
                        case BitmapImage bitmapImage:
                            return new Image() { Source = bitmapImage };
                        case string iconString:
                            return new TextBlock() { Text = iconString, FontFamily = ResourceProvider.GetResource("FontIcoFont") as FontFamily };
                        default:
                            break;
                    }
                } else
                {
                    iconCache[domain] = null;
                }

                var index = domain.IndexOf('.');
                if (index <= 0)
                {
                    break;
                }
                domain = domain.Substring(index + 1);

            }

            iconCache[uri.Host] = null;

            return null;
        }

        public LinkExt(Link link) : base(link.Name, link.Url)
        {
            PropertyChanged += LinkExt_PropertyChanged;
        }

        private void LinkExt_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(Url))
            {
                OnPropertyChanged(nameof(Icon));
            }
        }
    }
}
