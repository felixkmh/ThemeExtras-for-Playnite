using Extras.Abstractions;
using Extras.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Playnite.SDK.Controls;
using PlayniteCommon.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace Extras.Controls
{
    public class ThemedPluginUserControl : PluginUserControl
    {
        bool _initialized = false;

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);
            
            if (!(Parent is ContentControl parent) || _initialized)
            {
                return;
            }

            var controls = this.GetLogicalDescendants<IThemedControl>().ToList();
            foreach (var control in controls)
            {
                var enumerator = control.GetLocalValueEnumerator();
                while (enumerator.MoveNext())
                {
                    var dependencyProperty = enumerator.Current.Property;
                    var key = $"{control.GetType().Name}:{dependencyProperty.Name}";
                    object resource = parent.TryFindResource(key);
                    if (resource is MarkupExtension extension)
                    {
                        resource = extension.ProvideValue(null);
                    }
                    if (resource?.GetType() == dependencyProperty.PropertyType)
                    {
                        control.SetCurrentValue(dependencyProperty, resource);
                    } else if (resource is Binding binding)
                    {
                        control.SetBinding(dependencyProperty, binding);
                    }
                }
            }

            _initialized = true;
        }
    }
}
