using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
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

        private object icon = null;
        public object Icon { get => icon; set => SetValue(ref icon, value); }

        public ICommand OpenLinkCommand => new RelayCommand(() => 
        {
            try
            {
                System.Diagnostics.Process.Start(Url); 
            }
            catch (Exception ex)
            {
                Extras.logger.Debug(ex, $"Failed to open url {Url}.");
            }
        });

        private static readonly string assemblyDir = Path.GetDirectoryName(typeof(Extras).Assembly.Location);
        public static FontFamily IconFont = new FontFamily($"file:///{Path.Combine(assemblyDir, "Assets/Fonts/brands.ttf")}#brands");
        public static FontFamily FontAwesomeFont = new FontFamily($"file:///{Path.Combine(assemblyDir, "Assets/Fonts/Font Awesome 6 Brands-Regular-400.otf")}#Font Awesome 6 Brands");

        public static string UserIconDir => Extras.Instance.UserLinkIconDir;

        public static readonly ReadOnlyDictionary<string, Uri> FileIconDict = new ReadOnlyDictionary<string,Uri>(new Dictionary<string, Uri>
        {
            { "bethesda.net", new Uri(Path.Combine(assemblyDir, "Assets/Icons/bethesda.ico")) },
            { "gog.com", new Uri(Path.Combine(assemblyDir, "Assets/Icons/gog.ico")) },
            { "humblebundle.com", new Uri(Path.Combine(assemblyDir, "Assets/Icons/humble.ico")) },
            { "origin.com", new Uri(Path.Combine(assemblyDir, "Assets/Icons/origin.ico")) },
        });

        public static readonly ReadOnlyDictionary<string, char> IconFontDict = new ReadOnlyDictionary<string, char>(new Dictionary<string, char>
        {
            { "epicgames.com", (char)59882 },
            { "ubisoftconnect.com", (char)60484 },
            { "ubisoft.com", (char)60484 },
            { "uplay.com", (char)60484 },
        });

        public static readonly ReadOnlyDictionary<string, char> FontAwesomeDict = new ReadOnlyDictionary<string, char>(new Dictionary<string, char>
        {
            { "steamcommunity.com", '\uf1b6' },
            { "steampowered.com", '\uf1b6' },
            { "discord.gg", '\uf392' },
            { "discord.com", '\uf392' },
            { "twitch.tv", '\uf1e8' },
            { "facebook.com", '\uf09a' },
            { "twitter.com", '\uf099' },
            { "amazon.com", '\uf270' },
            { "apple.com", '\uf179' },
            { "battle.net", '\uf835' },
            { "github.com", '\uf09b' },
            { "xbox.com", '\uf412' },
            { "microsoft.com", '\uf17a' },
            { "itch.io", '\uf83a' },
            { "wikipedia.org", '\uf266' },
            { "playstation.com", '\uf3df' },
            { "play.google.com", '\uf3ab' },
            { "google.com", '\uf1a0' },
            { "spotify.com", '\uf1bc' },
            { "kickstarter.com", '\uf3bc' },
            { "mixer.com", '\ue056' },
            { "napster.com", '\uf3d2' },
            { "patreon.com", '\uf3d9' },
            { "paypal.com", '\uf1ed' },
            { "pinterest.com", '\uf0d2' },
            { "reddit.com", '\uf1a1' },
            { "soundcloud.com", '\uf1be' },
            { "teamspeak.com", '\uf4f9' },
            { "telegram.org", '\uf2c6' },
            { "tiktok.com", '\ue07b' },
            { "tumblr.com", '\uf173' },
            { "unity.com", '\ue049' },
            { "youtube.com", '\uf167' },
            { "wordpress.com", '\uf411' },
            { "instagram.com", '\uF16D' },
        });

        public static async Task<object> GetIconAsync(string Url)
        {
            if (!Uri.TryCreate(Url, UriKind.Absolute, out var uri))
            {
                return null;
            }

            var domain = uri.Host ?? "";

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
                    var dirInfo = new DirectoryInfo(UserIconDir);
                    if (dirInfo.Exists)
                    {
                        var files = dirInfo.EnumerateFiles("*", SearchOption.AllDirectories)
                            .Where(f => f.Extension.Equals(".ico", StringComparison.OrdinalIgnoreCase) ||
                                        f.Extension.Equals(".png", StringComparison.OrdinalIgnoreCase) ||
                                        f.Extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase));
                        var userIconPath = files
                            .FirstOrDefault(f => Path.GetFileNameWithoutExtension(f.Name).Equals(domain, StringComparison.OrdinalIgnoreCase));

                        if (userIconPath != null)
                        {
                            try
                            {
                                var bitmap = new BitmapImage();
                                bitmap.BeginInit();
                                bitmap.UriSource = new Uri(userIconPath.FullName);
                                bitmap.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                                bitmap.EndInit();
                                icon = bitmap;
                            }
                            catch (Exception ex)
                            {
                                icon = null;
                                Extras.logger.Error(ex, $"Failed to load link icon \"{userIconPath}\" for domain \"{domain}\".");
                            }
                        }
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
                        case BitmapImage bitmapImage:
                            icon = bitmapImage;
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

                var hostToLower = domain.ToLower();
                if (icon is null)
                {

                    if (FontAwesomeDict.TryGetValue(hostToLower, out var fontAwesomeChar))
                    {
                        icon = new Tuple<char, FontFamily>(fontAwesomeChar, FontAwesomeFont);
                    }
                }

                if (icon is null)
                {
                    if (IconFontDict.TryGetValue(hostToLower, out var iconFontChar))
                    {
                        icon = new Tuple<char, FontFamily>(iconFontChar, IconFont);
                    }
                }

                if (icon is null)
                {
                    if (FileIconDict.TryGetValue(hostToLower, out var iconUri))
                    {
                        try
                        {
                            var bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.UriSource = iconUri;
                            bitmap.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.EndInit();
                            icon = bitmap;
                        }
                        catch (Exception ex)
                        {
                            icon = null;
                            Extras.logger.Error(ex, $"Failed to load link icon \"{iconUri}\" for domain \"{domain}\".");
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

            if (icon is null)
            {
                string faviconUrl = $@"http://www.google.com/s2/favicons?domain={uri.Host}&sz=32";
                try
                {
                    Uri faviconUri = new Uri(faviconUrl);
                    var httpClient = await HttpClientFactory.GetClientAsync();
                    var response = await httpClient.GetAsync(faviconUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.StreamSource = await response.Content.ReadAsStreamAsync();
                        bitmap.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        icon = bitmap;
                        iconCache[uri.Host] = icon;
                        return new Image() { Source = bitmap };
                    }
                }
                catch (Exception ex)
                {
                    icon = null;
                    Extras.logger.Error(ex, $"Failed to load link icon \"{faviconUrl}\" for domain \"{domain}\".");
                }
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
