using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extras.Abstractions.Navigation
{
    public interface INavigationPoint : IEquatable<INavigationPoint>
    {
        void Navigate();
        string DisplayName { get; }
    }
}
