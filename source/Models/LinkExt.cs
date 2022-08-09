using Playnite.SDK;
using Playnite.SDK.Data;
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

        public static List<Models.FontIconInfo> iconInfos = Serialization.FromJsonFile<List<Models.FontIconInfo>>(Path.Combine(Path.GetDirectoryName(typeof(Extras).Assembly.Location), "Assets/Fonts/brands.json"));
        public static FontFamily iconFont = new FontFamily($"file:///{Path.Combine(Path.GetDirectoryName(typeof(Extras).Assembly.Location), "Assets/Fonts/brands.ttf")}#brands");

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
                        case Tuple<char, FontFamily> iconString:
                            return new TextBlock() { Text = iconString.Item1.ToString(), FontFamily = iconString.Item2 };
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
                            icon = new Tuple<char, FontFamily>(iconChar, ResourceProvider.GetResource("FontIcoFont") as FontFamily) ;
                            break;
                        case string iconString:
                            icon = new Tuple<char, FontFamily>(iconString[0], ResourceProvider.GetResource("FontIcoFont") as FontFamily);
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
                                try
                                {
                                    var bitmap = new BitmapImage();
                                    bitmap.BeginInit();
                                    bitmap.UriSource = new Uri(match.FullName);
                                    bitmap.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                                    bitmap.EndInit();
                                    icon = bitmap;
                                }
                                catch (Exception ex)
                                {
                                    icon = null;
                                    Extras.logger.Error(ex, $"Failed to load link icon \"{match.FullName}\" for domain \"{domain}\".");
                                }
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
                        case Tuple<char, FontFamily> iconString:
                            return new TextBlock() { Text = iconString.Item1.ToString(), FontFamily = iconString.Item2 };
                        default:
                            return null;
                    }
                }

                var index = domain.IndexOf('.');
                if (index <= 0)
                {
                    break;
                }
                domain = domain.Substring(index + 1);

            }

            var host = uri.Host.ToLower();

            if (iconInfos.Where(i => host.Contains(i.Name)).OrderByDescending(i => i.Name.Length).FirstOrDefault() is FontIconInfo info)
            {
                iconCache[uri.Host] = new Tuple<char, FontFamily>(info.Char, iconFont);
                return new TextBlock() { Text = info.Char.ToString(), FontFamily = iconFont };
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
