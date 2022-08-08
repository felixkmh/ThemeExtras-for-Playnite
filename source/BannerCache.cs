using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Extras
{
    public interface IBannerProvider
    {
        Dictionary<Platform, string> PlatformBanners { get; }
        Dictionary<Guid, string> PluginBanners { get; }
        string DefaultBanner { get; }
    }

    public class BannerCache
    {
        private readonly IBannerProvider[] bannerProviders;
        private readonly Dictionary<Tuple<Guid, Guid>, BitmapImage> cache = new Dictionary<Tuple<Guid, Guid>, BitmapImage>();
        private readonly Dictionary<Platform, BitmapImage> platformBanners = new Dictionary<Platform, BitmapImage>();
        private readonly Dictionary<Guid, BitmapImage> pluginBanners = new Dictionary<Guid, BitmapImage>();
        private readonly MergedDictionary<Platform, string> combinedBannersByPlatform = new MergedDictionary<Platform, string>();
        private readonly MergedDictionary<Guid, string> combinedBannersByPluginId = new MergedDictionary<Guid, string>();
        private readonly BitmapImage defaultBanner;

        private bool isInitialized = false;

        public BannerCache(params IBannerProvider[] bannerProviders)
        {
            this.bannerProviders = bannerProviders;

            foreach (var provider in bannerProviders)
            {
                if (!string.IsNullOrEmpty(provider.DefaultBanner))
                {
                    if (CreateImage(provider.DefaultBanner) is BitmapImage bitmapImage)
                    {
                        defaultBanner = bitmapImage;
                        break;
                    }
                }
            }
        }

        public void Initialize()
        {
            foreach (var provider in bannerProviders)
            {
                var byPlatform = provider.PlatformBanners;
                foreach (var item in byPlatform)
                {
                    combinedBannersByPlatform[item.Key] = item.Value;
                }
                var byPluginId = provider.PluginBanners;
                foreach (var item in byPluginId)
                {
                    combinedBannersByPluginId[item.Key] = item.Value;
                }
            }
            isInitialized = true;
        }

        public BitmapImage CreateImage(string path)
        {
            try
            {
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.UriSource = new Uri(path);
                bitmapImage.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.DecodePixelHeight = Math.Min(1000, Math.Max(0, ExtendedTheme.Current?.DecodeHeight ?? 50));
                bitmapImage.EndInit();
                return bitmapImage;
            }
            catch (Exception)
            {}
            return null;
        }

        public BitmapImage GetBanner(Game game)
        {
            if (!isInitialized)
            {
                Initialize();
            }

            var key = new Tuple<Guid, Guid>(game.PlatformIds?.FirstOrDefault() ?? Platform.Empty.Id, game.PluginId);

            if (cache.TryGetValue(key, out var image))
            {
                return image;
            }

            BitmapImage pcImage = null;

            bool isPc = false;
            if (game.Platforms?.FirstOrDefault() is Platform platform)
            {
                isPc = platform.SpecificationId == "pc_windows";
                if (platformBanners.TryGetValue(platform, out BitmapImage platformImage))
                {
                    if (isPc)
                    {
                        pcImage = pcImage ?? platformImage;
                    } else
                    {
                        cache[key] = platformImage;
                        return platformImage;
                    }
                }

                while (combinedBannersByPlatform.TryGetValue(platform, out var path))
                {
                    if (CreateImage(path) is BitmapImage bitmapImage)
                    {
                        if (isPc)
                        {
                            pcImage = pcImage ?? bitmapImage;
                            break;
                        } else
                        {
                            platformBanners[platform] = bitmapImage;
                            cache[key] = bitmapImage;
                            return bitmapImage;
                        }
                    } else
                    {
                        combinedBannersByPlatform.Remove(platform);
                    }
                }

            }
            {
                var pluginId = game.PluginId;
                if (pluginBanners.TryGetValue(pluginId, out var pluginImage))
                {
                    return pluginImage;
                }

                if (pluginId == Platform.Empty.Id && pcImage != null)
                {
                    return pcImage;
                }

                while (combinedBannersByPluginId.TryGetValue(pluginId, out var path))
                {
                    if (CreateImage(path) is BitmapImage bitmapImage)
                    {
                        pluginBanners[pluginId] = bitmapImage;
                        cache[key] = bitmapImage;
                        return bitmapImage;
                    }
                    else
                    {
                        combinedBannersByPluginId.Remove(pluginId);
                    }
                }
            }

            cache[key] = pcImage ?? defaultBanner;

            return pcImage ?? defaultBanner;
        }
    }
}
