using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Extras.Extensions
{
    public static class IEnumerableExtension
    {
        public static IEnumerable<double> ToDoubles(this IEnumerable<string> strings)
        {
            foreach(var s in strings)
            {
                if (double.TryParse(s, System.Globalization.NumberStyles.Float | System.Globalization.NumberStyles.AllowThousands, NumberFormatInfo.InvariantInfo, out var d))
                {
                    yield return d;
                }
            }
        }
    }
}
