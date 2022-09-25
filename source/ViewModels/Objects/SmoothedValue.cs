using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Animation;

namespace Extras.ViewModels.Objects
{
    public class SmoothedValue : FrameworkElement
    {
        public static readonly DependencyProperty CurrentValueProperty
            = DependencyProperty.Register(nameof(CurrentValue), typeof(double), typeof(SmoothedValue));

        public static readonly DependencyProperty TargetValueProperty
                    = DependencyProperty.Register(nameof(TargetValue), typeof(double), typeof(SmoothedValue), new PropertyMetadata(0.0, TargetValueChanged));

        protected Duration animationDuration;

        public SmoothedValue(FrameworkElement element, string bindingPath, Duration duration)
        {
            animationDuration = duration;
            SetBinding(TargetValueProperty, new Binding(bindingPath) { Source = element, Mode = BindingMode.OneWay });
            element.IsVisibleChanged += Element_IsVisibleChanged;
            element.Unloaded += Element_Unloaded;
        }

        public double CurrentValue
        {
            get => (double)GetValue(CurrentValueProperty);
            set => SetCurrentValue(CurrentValueProperty, value);
        }

        public double TargetValue
        {
            get => (double)GetValue(TargetValueProperty);
            set => SetCurrentValue(TargetValueProperty, value);
        }

        private static void TargetValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = (SmoothedValue)d;
            var targetValue = (double)e.NewValue;
            if (targetValue > target.CurrentValue)
            {
                var animation = new DoubleAnimation(targetValue, target.animationDuration);
                target.BeginAnimation(CurrentValueProperty, animation, HandoffBehavior.SnapshotAndReplace);
            }
            else
            {
                target.BeginAnimation(CurrentValueProperty, null);
                target.CurrentValue = targetValue;
            }
        }

        private void Element_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is false)
            {
                BeginAnimation(CurrentValueProperty, null);
            }
        }

        private void Element_Unloaded(object sender, RoutedEventArgs e)
        {
            BeginAnimation(CurrentValueProperty, null);
        }
    }
}