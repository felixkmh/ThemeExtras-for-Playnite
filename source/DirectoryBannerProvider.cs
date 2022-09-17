using Extras.Models;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extras
{
    public class DirectoryBannerProvider : IBannerProvider
    {
        public string BannersByPlatformNamePath { get; set; }
        public string BannersByPluginIdPath { get; set; }
        public string BannersBySourceNamePath { get; set; }
        public string BannersBySpecIdPath { get; set; }
        public string DefaultBanner { get; set; }

        public Dictionary<Platform, string> PlatformBanners => GetPlatformBanners();

        public Dictionary<Guid, string> PluginBanners => GetPluginBanners();

        public Dictionary<GameSource, string> SourceBanners => GetSourceBanners();

        public Dictionary<Guid, string> GetPluginBanners()
        {
            var pluginBanners = new Dictionary<Guid, string>();
            var plugins = Playnite.SDK.API.Instance.Addons.Plugins
                .OfType<LibraryPlugin>()
                .Select(lp => lp.Id)
                .Concat(new[] { Guid.Empty })
                .ToHashSet();
            if (BannersByPluginIdPath is string bannersByPluginId)
            {
                var fullPath = bannersByPluginId;
                if (Directory.Exists(fullPath))
                {
                    var dirInfo = new DirectoryInfo(fullPath);
                    var bannerPaths = dirInfo.EnumerateFiles("*", SearchOption.AllDirectories)
                                             .Where(f => f.Extension.Equals(".png", StringComparison.OrdinalIgnoreCase) ||
                                                         f.Extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                                         f.Extension.Equals(".ico", StringComparison.OrdinalIgnoreCase));
                    foreach (var bannerFile in bannerPaths)
                    {
                        var pluginIdString = Path.GetFileNameWithoutExtension(bannerFile.Name);
                        if (Guid.TryParse(pluginIdString, out var id) && plugins.Contains(id))
                        {
                            pluginBanners[id] = bannerFile.FullName;
                        }
                    }
                }
            }

            return pluginBanners;
        }

        public Dictionary<GameSource, string> GetSourceBanners()
        {
            var sourceBanners = new Dictionary<GameSource, string>();
            var sources = Playnite.SDK.API.Instance.Database.Sources
                .Where(p => !string.IsNullOrEmpty(p.Name))
                .GroupBy(p => p.Name)
                .ToDictionary(p => p.First().Name, p => p.ToList());
            if (BannersBySourceNamePath is string bannersBySourceName)
            {
                var fullPath = bannersBySourceName;
                if (Directory.Exists(fullPath))
                {
                    var dirInfo = new DirectoryInfo(fullPath);
                    var bannerPaths = dirInfo.EnumerateFiles("*", SearchOption.AllDirectories)
                                             .Where(f => f.Extension.Equals(".png", StringComparison.OrdinalIgnoreCase) ||
                                                         f.Extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                                         f.Extension.Equals(".ico", StringComparison.OrdinalIgnoreCase));
                    foreach (var bannerFile in bannerPaths)
                    {
                        var sourceNameString = Path.GetFileNameWithoutExtension(bannerFile.Name);
                        if (sources.TryGetValue(sourceNameString, out var groupedSources))
                        {
                            foreach (var source in groupedSources)
                            {
                                if (!sourceBanners.ContainsKey(source))
                                {
                                    sourceBanners[source] = bannerFile.FullName;
                                }
                            }
                        }
                    }
                }
            }

            return sourceBanners;
        }

        private Dictionary<Platform, string> GetPlatformBanners()
        {
            var platformBanners = new Dictionary<Platform, string>();
            var platformsBySpecId = Playnite.SDK.API.Instance.Database.Platforms
                .Concat(new[] { Platform.Empty })
                .Where(p => !string.IsNullOrEmpty(p.SpecificationId))
                .GroupBy(p => p.SpecificationId)
                .ToDictionary(g => g.First().SpecificationId, g => g.ToList());

            if (BannersBySpecIdPath is string bannersBySpecIdPath)
            {
                var fullPath = bannersBySpecIdPath;
                if (Directory.Exists(fullPath))
                {
                    var dirInfo = new DirectoryInfo(fullPath);
                    var bannerPaths = dirInfo.EnumerateFiles("*", SearchOption.AllDirectories)
                                             .Where(f => f.Extension.Equals(".png", StringComparison.OrdinalIgnoreCase) ||
                                                         f.Extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase));
                    foreach (var bannerFile in bannerPaths)
                    {
                        var specIdString = Path.GetFileNameWithoutExtension(bannerFile.Name);
                        if (platformsBySpecId.TryGetValue(specIdString, out var platforms))
                        {
                            foreach (var platform in platforms)
                            {
                                platformBanners[platform] = bannerFile.FullName;
                            }
                        }
                    }
                }
            }

            if (BannersByPlatformNamePath is string bannersByPlatformName)
            {
                var fullPath = bannersByPlatformName;
                var platformsByName = Playnite.SDK.API.Instance.Database.Platforms
                .Concat(new[] { Platform.Empty })
                .Where(p => !string.IsNullOrEmpty(p.Name))
                .GroupBy(p => p.Name)
                .ToDictionary(p => p.First().Name, p => p.ToList());
                if (Directory.Exists(fullPath))
                {
                    var dirInfo = new DirectoryInfo(fullPath);
                    var bannerPaths = dirInfo.EnumerateFiles("*", SearchOption.AllDirectories)
                                             .Where(f => f.Extension.Equals(".png", StringComparison.OrdinalIgnoreCase) ||
                                                         f.Extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                                         f.Extension.Equals(".ico", StringComparison.OrdinalIgnoreCase));
                    foreach (var bannerFile in bannerPaths)
                    {
                        var platformName = Path.GetFileNameWithoutExtension(bannerFile.Name);
                        if (platformsByName.TryGetValue(platformName, out var platforms))
                        {
                            foreach (var platform in platforms)
                            {
                                if (!platformBanners.ContainsKey(platform))
                                {
                                    platformBanners[platform] = bannerFile.FullName;
                                }

                            }
                        }
                    }
                }
            }

            return platformBanners;
        }
    }
}
