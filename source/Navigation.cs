using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extras.Abstractions.Navigation;
using Extras.Models;

namespace Extras
{
    internal static class ListExtensions
    {
        public static T Pop<T>(this IList<T> list)
        {
            var last = list.LastOrDefault();
            if (last is T)
            {
                list.RemoveAt(list.Count - 1);
            }
            return last;
        }

        public static T Peek<T>(this IList<T> list)
        {
            return list.LastOrDefault();
        }

        public static void Push<T>(this IList<T> list, T item)
        {
            list.Add(item);
        }
    }

    public class Navigation : ObservableObject
    {
        public const int MAX_STEPS = 50;

        IList<INavigationPoint> BackwardsStack { get; } = new List<INavigationPoint>();
        IList<INavigationPoint> ForwardsStack { get; } = new List<INavigationPoint>();

        INavigationPoint currentlyNavigating = null;

        public bool Add(INavigationPoint navigationPoint)
        {
            if (!navigationPoint.Equals(BackwardsStack.LastOrDefault())
                && currentlyNavigating == null)
            {
                BackwardsStack.Push(navigationPoint);
                ThemeExtras.logger.Debug($"Pushed \"{navigationPoint}\"");
                if (BackwardsStack.Count > MAX_STEPS)
                {
                    BackwardsStack.RemoveAt(0);
                }
                ForwardsStack.Clear();
                // Extras.logger.Debug("Cleared forward stack.");
                return true;
            }
            OnPropertyChanged(nameof(CanGoBack));
            OnPropertyChanged(nameof(CanGoForward));
            return false;
        }

        public bool CanGoBack => BackwardsStack.Count > 1;
        public bool CanGoForward => ForwardsStack.Count > 0;

        public void Back()
        {
            if (BackwardsStack.Count > 1)
            {
                ForwardsStack.Push(BackwardsStack.Pop());
                if (ForwardsStack.Count > MAX_STEPS)
                {
                    ForwardsStack.RemoveAt(0);
                }
                currentlyNavigating = BackwardsStack.Peek();
                try
                {
                    currentlyNavigating.Navigate();
                }
                catch (Exception ex)
                {
                    ThemeExtras.logger.Error(ex, string.Format("Failed to execute navigation \"{0}\".", currentlyNavigating));
                }
                ThemeExtras.logger.Debug($"Popped \"{currentlyNavigating}\" from backwards stack.");
                OnPropertyChanged(nameof(CanGoBack));
                OnPropertyChanged(nameof(CanGoForward));
            }
            currentlyNavigating = null;
        }

        public void Forward()
        {
            if (ForwardsStack.Count > 0)
            {
                BackwardsStack.Push(ForwardsStack.Pop());
                if (BackwardsStack.Count > MAX_STEPS)
                {
                    BackwardsStack.RemoveAt(0);
                }
                currentlyNavigating = BackwardsStack.Peek();
                try
                {
                    currentlyNavigating.Navigate();
                }
                catch (Exception ex)
                {
                    ThemeExtras.logger.Error(ex, string.Format("Failed to execute navigation \"{0}\".", currentlyNavigating));
                }
                ThemeExtras.logger.Debug($"Popped \"{currentlyNavigating}\" from forwards stack.");
                OnPropertyChanged(nameof(CanGoBack));
                OnPropertyChanged(nameof(CanGoForward));
            }
            currentlyNavigating = null;
        }
    }
}
