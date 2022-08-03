using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extras.Models
{
    public class ThemeManifest
    {
        public Version ThemeApiVersion { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Author { get; set; }
        public string Version { get; set; }
    }
}
