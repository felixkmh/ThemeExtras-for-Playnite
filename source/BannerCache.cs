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
            if (game.Platforms?.FirstOrDefault() is Platform platform)
            {
                if (platformBanners.TryGetValue(platform, out BitmapImage platformImage))
                {
                    return platformImage;
                }

                while (combinedBannersByPlatform.TryGetValue(platform, out var path))
                {
                    if (CreateImage(path) is BitmapImage bitmapImage)
                    {
                        platformBanners[platform] = bitmapImage;
                        return bitmapImage;
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

                while (combinedBannersByPluginId.TryGetValue(pluginId, out var path))
                {
                    if (CreateImage(path) is BitmapImage bitmapImage)
                    {
                        pluginBanners[pluginId] = bitmapImage;
                        return bitmapImage;
                    }
                    else
                    {
                        combinedBannersByPluginId.Remove(pluginId);
                    }
                }
            }

            return defaultBanner;
        }
    }
}
