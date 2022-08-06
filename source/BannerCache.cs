using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private readonly BitmapImage defaultBanner;

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

        public BitmapImage CreateImage(string path)
        {
            try
            {
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.UriSource = new Uri(path);
                bitmapImage.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.DecodePixelHeight = 50;
                bitmapImage.EndInit();
                return bitmapImage;
            }
            catch (Exception)
            {}
            return null;
        }

        public BitmapImage GetBanner(Game game)
        {
            if (game.Platforms?.FirstOrDefault() is Platform platform)
            {
                if (platformBanners.TryGetValue(platform, out BitmapImage platformImage))
                {
                    return platformImage;
                }
                foreach(var provider in bannerProviders)
                {
                    var banners = provider.PlatformBanners;
                    if (banners.TryGetValue(platform, out var path))
                    {
                        if (CreateImage(path) is BitmapImage bitmapImage)
                        {
                            platformBanners[platform] = bitmapImage;
                            return bitmapImage;
                        } else
                        {
                            banners.Remove(platform);
                        }
                    }
                }
            }
            var pluginId = game.PluginId;
            if (pluginBanners.TryGetValue(pluginId, out var pluginImage))
            {
                return pluginImage;
            } 
            foreach(var provider in bannerProviders)
            {
                var banners = provider.PluginBanners;
                if (banners.TryGetValue(pluginId, out var path))
                {
                    if (CreateImage(path) is BitmapImage bitmapImage)
                    {
                        pluginBanners[pluginId] = bitmapImage;
                        return bitmapImage;
                    }
                    else
                    {
                        banners.Remove(pluginId);
                    }
                }
            }
            return defaultBanner;
        }
    }
}
