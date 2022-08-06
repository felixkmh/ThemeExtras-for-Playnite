using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extras.Models
{
    public class ThemeExtrasManifest
    {
        const string InstallUri = "playnite://playnite/installaddon/{0}";

        public class Recommendation
        {
            public string AddonName { get; set; }
            public string AddonId { get; set; }
            public List<string> Features { get; set; }
            [DontSerialize]
            internal IEnumerable<string> FeaturesLocalized => Features.Select(f => f.StartsWith("LOC") ? ResourceProvider.GetString(f) : f);
            [DontSerialize]
            internal string AddonInstallUri => string.Format(InstallUri, AddonName);
        }

        public string ThemeId { get; set; }
        public List<Recommendation> Recommendations { get; set; }

        public List<string> PersistentPaths { get; set; }

        public string DefaultBannerPath { get; set; }
        public string BannersBySpecIdPath { get; set; }
        public string BannersByPluginIdPath { get; set; }
        public string BannersByPlatformName { get; set; }
    }
}
