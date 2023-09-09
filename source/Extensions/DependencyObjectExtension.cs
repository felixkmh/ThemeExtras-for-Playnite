using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Extras.Extensions
{
    public static class DependencyObjectExtension
    {
        public static IEnumerable<T> GetLogicalDescendants<T>(this DependencyObject parent)
        {
            var children = LogicalTreeHelper.GetChildren(parent);
            foreach (var child in children)
            {
                if (child is T hit)
                {
                    yield return hit;
                }
                if (child is DependencyObject dependencyObject)
                {
                    foreach(var grandChild in dependencyObject.GetLogicalDescendants<T>())
                    {
                        yield return grandChild;
                    }
                }
            }
        }
    }
}
