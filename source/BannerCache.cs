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
        Dictionary<GameSource, string> SourceBanners { get; }
        string DefaultBanner { get; }
    }

    public class BannerCache
    {
        public class WeakBitmapImage
        {
            public BitmapImage ImageSource
            {
                get => GetImageSource(); set
                {
                    WeakImageSource.SetTarget(value);
                    Uri = value?.UriSource;
                }
            }
            public WeakReference<BitmapImage> WeakImageSource { get; private set; }
            public Uri Uri { get; private set; }

            public static implicit operator WeakBitmapImage(BitmapImage source)
            {
                return new WeakBitmapImage { WeakImageSource = new WeakReference<BitmapImage>(source), Uri = source?.UriSource };
            }

            public static implicit operator BitmapImage(WeakBitmapImage weakBanner)
            {
                return weakBanner.GetImageSource();
            }

            private BitmapImage GetImageSource()
            {
                if (WeakImageSource.TryGetTarget(out var image))
                {
                    return image;
                }
                var refreshedSource = CreateImage(Uri);
                WeakImageSource.SetTarget(refreshedSource);
                return refreshedSource;
            }
        }

        private readonly IBannerProvider[] bannerProviders;
        private readonly Dictionary<Tuple<Guid, Guid, Guid>, WeakBitmapImage> cache = new Dictionary<Tuple<Guid, Guid, Guid>, WeakBitmapImage>();
        private readonly Dictionary<Platform, WeakBitmapImage> platformBanners = new Dictionary<Platform, WeakBitmapImage>();
        private readonly Dictionary<Guid, WeakBitmapImage> pluginBanners = new Dictionary<Guid, WeakBitmapImage>();
        private readonly Dictionary<GameSource, WeakBitmapImage> sourceBanners = new Dictionary<GameSource, WeakBitmapImage>();
        private readonly MergedDictionary<Platform, string> combinedBannersByPlatform = new MergedDictionary<Platform, string>();
        private readonly MergedDictionary<Guid, string> combinedBannersByPluginId = new MergedDictionary<Guid, string>();
        private readonly MergedDictionary<GameSource, string> combinedBannersBySource = new MergedDictionary<GameSource, string>();
        private readonly WeakBitmapImage defaultBanner;

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
                if (byPlatform != null)
                    foreach (var item in byPlatform)
                    {
                        combinedBannersByPlatform[item.Key] = item.Value;
                    }
                var byPluginId = provider.PluginBanners;
                if(byPluginId != null)
                    foreach (var item in byPluginId)
                    {
                        combinedBannersByPluginId[item.Key] = item.Value;
                    }
                var bySource = provider.SourceBanners;
                if (bySource != null)
                    foreach (var item in bySource)
                    {
                        combinedBannersBySource[item.Key] = item.Value;
                    }
            }
            isInitialized = true;
        }

        public void Reset()
        {
            combinedBannersByPlatform.Clear();
            combinedBannersByPluginId.Clear();
            combinedBannersBySource.Clear();

            cache.Clear();

            isInitialized = false;
        }

        public static BitmapImage CreateImage(string path)
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
            { }
            return null;
        }

        public static BitmapImage CreateImage(Uri uri)
        {
            try
            {
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.UriSource = uri;
                bitmapImage.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                //bitmapImage.DecodePixelHeight = Math.Min(1000, Math.Max(0, ExtendedTheme.Current?.DecodeHeight ?? 50));
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

            Tuple<Guid, Guid, Guid> key = null;
            if ((game.PlatformIds?.Count ?? 0) <= 1)
            {
                key = new Tuple<Guid, Guid, Guid>(game.PlatformIds?.FirstOrDefault() ?? Platform.Empty.Id, game.PluginId, game.Source?.Id ?? GameSource.Empty.Id);
                if (cache.TryGetValue(key, out var image))
                {
                    return image;
                }
            }

            WeakBitmapImage pcImage = null;
            WeakBitmapImage platformImage = null;

            bool isPc = false;
            if (game.Platforms?.OrderBy(p => p.SpecificationId == "pc_windows" ? -1 : 1).FirstOrDefault() is Platform platform)
            {
                isPc = platform.SpecificationId == "pc_windows";
                if (platformBanners.TryGetValue(platform, out platformImage))
                {
                    if (isPc)
                    {
                        pcImage = pcImage ?? platformImage;
                    } else
                    {
                        if (key != null)
                        {
                            cache[key] = platformImage;
                            return platformImage;
                        }
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
                            platformImage = bitmapImage;
                            platformBanners[platform] = bitmapImage;
                            if (key != null)
                            {
                                cache[key] = bitmapImage;
                                return bitmapImage;
                            }
                            break;
                        }
                    } else
                    {
                        combinedBannersByPlatform.Remove(platform);
                    }
                }

            }
            {
                var source = game.Source ?? GameSource.Empty;

                if (source == GameSource.Empty && pcImage != null)
                {
                    if (key != null)
                    {
                        cache[key] = pcImage;
                    }
                    return pcImage;
                }

                if (sourceBanners.TryGetValue(source, out var sourceImage))
                {
                    if (key != null)
                    {
                        cache[key] = sourceImage;
                    }
                    return sourceImage;
                }

                while (combinedBannersBySource.TryGetValue(source, out var path))
                {
                    if (CreateImage(path) is BitmapImage bitmapImage)
                    {
                        sourceBanners[source] = bitmapImage;
                        if (key != null)
                        {
                            cache[key] = bitmapImage;
                        }
                        return bitmapImage;
                    }
                    else
                    {
                        combinedBannersBySource.Remove(source);
                    }
                }
            }

            if (platformImage is WeakBitmapImage)
            {
                return platformImage;
            }

            {
                var pluginId = game.PluginId;

                if (pluginId == Platform.Empty.Id && pcImage != null)
                {
                    if (key != null)
                    {
                        cache[key] = pcImage;
                    }
                    return pcImage;
                }

                if (pluginBanners.TryGetValue(pluginId, out var pluginImage))
                {
                    if (key != null)
                    {
                        cache[key] = pluginImage;
                    }
                    return pluginImage;
                }

                while (combinedBannersByPluginId.TryGetValue(pluginId, out var path))
                {
                    if (CreateImage(path) is BitmapImage bitmapImage)
                    {
                        pluginBanners[pluginId] = bitmapImage;
                        if (key != null)
                        {
                            cache[key] = bitmapImage;
                        }
                        return bitmapImage;
                    }
                    else
                    {
                        combinedBannersByPluginId.Remove(pluginId);
                    }
                }
            }

            if (key != null)
            {
                cache[key] = pcImage ?? defaultBanner;
            }

            return pcImage ?? defaultBanner;
        }
    }
}
