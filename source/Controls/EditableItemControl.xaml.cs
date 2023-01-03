using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using Extras.Models;

namespace Extras.Controls
{
    public partial class EditableItemControl : ItemsControl
    {
        public static readonly DependencyProperty ItemPoolProperty = DependencyProperty.Register(
            nameof(ItemPool), typeof(IList), typeof(EditableItemControl), new PropertyMetadata(Array.Empty<object>(), OnItemPoolChanged));

        public static readonly DependencyProperty FilteredItemPoolProperty = DependencyProperty.Register(
            nameof(FilteredItemPool), typeof(ICollectionView), typeof(EditableItemControl), new PropertyMetadata(default(ICollectionView)));

        public static readonly DependencyProperty FilterTextProperty = DependencyProperty.Register(
            nameof(FilterText), typeof(string), typeof(EditableItemControl), new PropertyMetadata(default(string), OnFilterTextChanged));

        public EditableItemControl()
        {
            InitializeComponent();
            
            
        }

        public IList ItemPool
        {
            get => (IList)GetValue(ItemPoolProperty);
            set => SetValue(ItemPoolProperty, value);
        }

        public ICollectionView FilteredItemPool
        {
            get => (ICollectionView)GetValue(FilteredItemPoolProperty);
            private set => SetValue(FilteredItemPoolProperty, value);
        }

        public string FilterText
        {
            get => (string)GetValue(FilterTextProperty);
            set => SetValue(FilterTextProperty, value);
        }

        private static void OnItemPoolChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.SetValue(FilteredItemPoolProperty, new ListCollectionView((IList)e.NewValue));
        }

        public class PlaceholderItem : INamedItem
        {
            public string Name { get; set; } = "+";
        }

        private static void OnFilterTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var poolView = (ICollectionView)d.GetValue(FilteredItemPoolProperty);
            if (poolView is ICollectionView)
            {
                var filter = (string)e.NewValue;
                poolView.Filter = o =>
                {
                    if (o is INamedItem item)
                    {
                        return Matches(item.Name, filter);
                    }

                    return false;
                };
            }
        }

        private static bool Matches(string name, string filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
                return true;

            if (string.IsNullOrEmpty(name))
                return false;

            return name.IndexOf(filter, StringComparison.InvariantCultureIgnoreCase) >= 0;
        }
    }
}