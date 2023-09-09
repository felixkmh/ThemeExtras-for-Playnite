using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using Extras.Abstractions;
using Extras.Models;
using Playnite.SDK;
using Playnite.SDK.Models;

namespace Extras.Controls
{
    public partial class EditableItemsControl : ItemsControl, IThemedControl
    {
        public static readonly DependencyProperty PlaceholderTemplateProperty = DependencyProperty.Register(
            nameof(PlaceholderTemplate), typeof(DataTemplate), typeof(EditableItemsControl), new PropertyMetadata(default(DataTemplate), OnDataTemplateChanged));

        public static readonly DependencyProperty DefaultTemplateProperty = DependencyProperty.Register(
            nameof(DefaultTemplate), typeof(DataTemplate), typeof(EditableItemsControl), new PropertyMetadata(default(DataTemplate), OnDataTemplateChanged));

        public static readonly DependencyProperty ItemsPoolProperty = DependencyProperty.Register(
            nameof(ItemsPool), typeof(IList), typeof(EditableItemsControl), new PropertyMetadata(Array.Empty<object>(), OnItemPoolChanged));

        public static readonly DependencyProperty FilteredItemsPoolProperty = DependencyProperty.Register(
            nameof(FilteredItemsPool), typeof(ICollectionView), typeof(EditableItemsControl), new PropertyMetadata(default(ICollectionView)));

        public static readonly DependencyProperty FilterTextProperty = DependencyProperty.Register(
            nameof(FilterText), typeof(string), typeof(EditableItemsControl), new PropertyMetadata(default(string), OnFilterTextChanged));

        public static readonly DependencyProperty BestMatchProperty = DependencyProperty.Register(
            nameof(BestMatch), typeof(string), typeof(EditableItemsControl), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty MatchProperty = DependencyProperty.Register(
            nameof(Match), typeof(MatchData), typeof(EditableItemsControl), new PropertyMetadata(default(MatchData)));

        public static readonly DependencyProperty EditableItemsSourceProperty = DependencyProperty.Register(
            nameof(EditableItemsSource), typeof(IList), typeof(EditableItemsControl), new PropertyMetadata(default(string), OnEditableItemsSourceChanged));

        public static readonly DependencyProperty AcceptCommandProperty = DependencyProperty.Register(nameof(AcceptCommand), typeof(ICommand), typeof(EditableItemsControl));

        public static readonly DependencyProperty RemoveCommandProperty = DependencyProperty.Register(nameof(RemoveCommand), typeof(ICommand), typeof(EditableItemsControl));

        public static readonly DependencyProperty SaveChangesCommandProperty = DependencyProperty.Register(nameof(SaveChangesCommand), typeof(ICommand), typeof(EditableItemsControl));

        public static readonly DependencyProperty DiscardChangesCommandProperty = DependencyProperty.Register(nameof(DiscardChangesCommand), typeof(ICommand), typeof(EditableItemsControl));

        public static readonly DependencyProperty AutoCompleteCommandProperty = DependencyProperty.Register(nameof(AutoCompleteCommand), typeof(ICommand), typeof(EditableItemsControl));

        public static readonly DependencyProperty IsDeferredProperty = DependencyProperty.Register(nameof(IsDeferred), typeof(bool), typeof(EditableItemsControl));

        public class MatchData : ObservableObject
        {
            private string _filter;
            public string Filter
            {
                get => _filter;
                set => SetValue(ref _filter, value);
            }

            private IEnumerable _matches;
            public IEnumerable Matches
            {
                get => _matches;
                set => SetValue(ref _matches, value);
            }
        }

        public class CustomTemplateSelector : DataTemplateSelector
        {
            public CustomTemplateSelector(EditableItemsControl itemsControl) : base()
            { 
                _itemsControl = itemsControl;
            }

            EditableItemsControl _itemsControl;

            public override DataTemplate SelectTemplate(object item, DependencyObject container)
            {
                if (item is PlaceholderItem)
                    return _itemsControl.PlaceholderTemplate;

                return _itemsControl.DefaultTemplate;
            }
        }

        public EditableItemsControl() : base()
        {
            ItemTemplateSelector = new CustomTemplateSelector(this);
        }

        public bool IsDeferred
        {
            get => (bool)GetValue(IsDeferredProperty);
            set => SetValue(IsDeferredProperty, value);
        }

        public MatchData Match
        {
            get => (MatchData)GetValue(MatchProperty);
            set => SetValue(MatchProperty, value);
        }

        public string BestMatch
        {
            get => (string)GetValue(BestMatchProperty);
            set => SetValue(BestMatchProperty, value);
        }

        public DataTemplate DefaultTemplate
        {
            get => (DataTemplate)GetValue(DefaultTemplateProperty);
            set => SetValue(DefaultTemplateProperty, value);
        }

        public DataTemplate PlaceholderTemplate
        {
            get => (DataTemplate)GetValue(PlaceholderTemplateProperty);
            set => SetValue(PlaceholderTemplateProperty, value);
        }

        public ICommand AcceptCommand
        {
            get => (ICommand)GetValue(AcceptCommandProperty);
            set => SetValue(AcceptCommandProperty, value);
        }

        public ICommand RemoveCommand
        {
            get => (ICommand)GetValue(RemoveCommandProperty);
            set => SetValue(RemoveCommandProperty, value);
        }

        public ICommand SaveChangesCommand
        {
            get => (ICommand)GetValue(SaveChangesCommandProperty);
            set => SetValue(SaveChangesCommandProperty, value);
        }

        public ICommand DiscardChangesCommand
        {
            get => (ICommand)GetValue(DiscardChangesCommandProperty);
            set => SetValue(DiscardChangesCommandProperty, value);
        }

        public ICommand AutoCompleteCommand
        {
            get => (ICommand)GetValue(AutoCompleteCommandProperty);
            set => SetValue(AutoCompleteCommandProperty, value);
        }

        public IList EditableItemsSource
        {
            get => (IList)GetValue(EditableItemsSourceProperty);
            set => SetValue(EditableItemsSourceProperty, value);
        }

        public IList ItemsPool
        {
            get => (IList)GetValue(ItemsPoolProperty);
            set => SetValue(ItemsPoolProperty, value);
        }

        public ICollectionView FilteredItemsPool
        {
            get => (ICollectionView)GetValue(FilteredItemsPoolProperty);
            private set => SetValue(FilteredItemsPoolProperty, value);
        }

        public string FilterText
        {
            get => (string)GetValue(FilterTextProperty);
            set => SetValue(FilterTextProperty, value);
        }

        private static void OnDataTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (EditableItemsControl)d;
            control.Items.Refresh();
        }

        private static void OnEditableItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (EditableItemsControl)d;
            object[] value = ((IList)e.NewValue)?.Cast<object>().Concat(new object[] { new PlaceholderItem() }).ToArray();
            
            control.ItemsSource = value;

            control.FilterText = "";

            if (e.OldValue is INotifyCollectionChanged oldPool)
            {
                oldPool.CollectionChanged -= OnSourceCollectionChanged;
            }
            if (e.NewValue is INotifyCollectionChanged newPool)
            {
                newPool.CollectionChanged += OnSourceCollectionChanged;
            }
        }

        private static void OnItemPoolChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ListCollectionView value = new ListCollectionView((IList)e.NewValue);
            value.SortDescriptions.Add(new SortDescription { PropertyName = "Name.Length" });
            d.SetCurrentValue(FilteredItemsPoolProperty, value);
        }

        private static void OnSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var control = (EditableItemsControl)sender;

            control.Items.RemoveAt(control.Items.Count - 1);

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    control.Items.Add(e.NewItems[0]);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    control.Items.Add(e.OldItems[0]);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    control.Items[e.OldStartingIndex] = e.NewItems[0];
                    break;
                case NotifyCollectionChangedAction.Move:
                    var temp = control.Items[e.OldStartingIndex];
                    control.Items[e.OldStartingIndex] = control.Items[e.NewStartingIndex];
                    control.Items[e.NewStartingIndex] = temp;
                    break;
                case NotifyCollectionChangedAction.Reset:
                    control.Items.Clear();
                    break;
                default:
                    break;
            }

            control.Items.Add(new PlaceholderItem());
        }

        public class PlaceholderItem : INamedItem
        {
            public string Name { get; set; } = "+";

            public bool IsItem => false;

            public bool IsPlaceholder => !IsItem;
        }

        private static void OnFilterTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var poolView = (ICollectionView)d.GetValue(FilteredItemsPoolProperty);
            var control = (EditableItemsControl)d;
            if (poolView is ICollectionView)
            {
                var filter = (string)e.NewValue;
                poolView.Filter = o =>
                {
                    if (o is DatabaseObject item)
                    {
                        return Matches(item.Name, filter) && (!control.EditableItemsSource?.Contains(item) ?? false);
                    }

                    return false;
                };
                control.Match = new MatchData { Filter = filter, Matches = poolView };
                var match = poolView.Cast<DatabaseObject>().FirstOrDefault()?.Name;
                control.BestMatch = $"{control.FilterText}{match?.Substring(control.FilterText.Length)}";
            }
        }

        private static bool Matches(string name, string filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
                return false;

            if (string.IsNullOrEmpty(name))
                return false;

            return name.IndexOf(filter, StringComparison.InvariantCultureIgnoreCase) == 0;
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);
        }
    }
}