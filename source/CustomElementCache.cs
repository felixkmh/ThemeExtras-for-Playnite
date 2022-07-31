using Playnite.SDK.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Extras
{
    public class CustomElementCache<T> where T : FrameworkElement
    {
        private Func<string, T> generator;

        public CustomElementCache(Func<string, T> generator)
        {
            this.generator = generator;
        }

        public T GetOrGenerate(string name)
        {
            Stack<T> elements = null;
            if (cache.TryGetValue(name, out var existing))
            {
                elements = existing;
            } else
            {
                elements = new Stack<T>();
                cache[name] = elements;
            }
            T element = null;
            if (elements.Count > 0)
            {
                element = elements.Pop();
            } else
            {
                element = generator(name);
            }
            if (element != null)
            {
                element.Tag = name;
                element.Unloaded += Element_Unloaded;
            }
            return element;
        }

        private void Element_Unloaded(object sender, RoutedEventArgs e)
        {
            var element = sender as T;
            var name = element.Tag as string;
            element.Unloaded -= Element_Unloaded;
            cache[name].Push(element);
        }

        Dictionary<string, Stack<T>> cache = new Dictionary<string, Stack<T>>();
    }
}
