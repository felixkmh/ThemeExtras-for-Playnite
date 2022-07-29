using Playnite.SDK;
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
            [YamlDotNet.Serialization.YamlIgnore]
            public IEnumerable<string> FeaturesLocalized => Features.Select(f => f.StartsWith("LOC") ? ResourceProvider.GetString(f) : f);
            [YamlDotNet.Serialization.YamlIgnore]
            public string AddonInstallUri => string.Format(InstallUri, AddonName);
        }

        public string ThemeId { get; set; }
        public List<Recommendation> Recommendations { get; set; }
    }
}
