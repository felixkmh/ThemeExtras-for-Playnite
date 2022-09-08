using System;
using System.Collections.Generic;
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

        List<INavigationPoint> backwardsStack = new List<INavigationPoint>();
        List<INavigationPoint> forwardsStack = new List<INavigationPoint>();

        INavigationPoint currentlyNavigating = null;

        public bool Add(INavigationPoint navigationPoint)
        {
            if (!navigationPoint.Equals(backwardsStack.LastOrDefault())
                && currentlyNavigating == null)
            {
                backwardsStack.Push(navigationPoint);
                Extras.logger.Debug($"Pushed \"{navigationPoint}\"");
                if (backwardsStack.Count > MAX_STEPS)
                {
                    backwardsStack.RemoveAt(0);
                }
                forwardsStack.Clear();
                // Extras.logger.Debug("Cleared forward stack.");
                return true;
            }
            return false;
        }

        public bool CanGoBack => backwardsStack.Count > 1;
        public bool CanGoForward => forwardsStack.Count > 0;

        public void Back()
        {
            if (backwardsStack.Count > 1)
            {
                forwardsStack.Push(backwardsStack.Pop());
                if (forwardsStack.Count > MAX_STEPS)
                {
                    forwardsStack.RemoveAt(0);
                }
                currentlyNavigating = backwardsStack.Peek();
                try
                {
                    currentlyNavigating.Navigate();
                }
                catch (Exception ex)
                {
                    Extras.logger.Error(ex, string.Format("Failed to execute navigation \"{0}\".", currentlyNavigating));
                }
                Extras.logger.Debug($"Popped \"{currentlyNavigating}\" from backwards stack.");
                OnPropertyChanged(nameof(CanGoBack));
                OnPropertyChanged(nameof(CanGoForward));
            }
            currentlyNavigating = null;
        }

        public void Forward()
        {
            if (forwardsStack.Count > 0)
            {
                backwardsStack.Push(forwardsStack.Pop());
                if (backwardsStack.Count > MAX_STEPS)
                {
                    backwardsStack.RemoveAt(0);
                }
                currentlyNavigating = backwardsStack.Peek();
                try
                {
                    currentlyNavigating.Navigate();
                }
                catch (Exception ex)
                {
                    Extras.logger.Error(ex, string.Format("Failed to execute navigation \"{0}\".", currentlyNavigating));
                }
                Extras.logger.Debug($"Popped \"{currentlyNavigating}\" from forwards stack.");
                OnPropertyChanged(nameof(CanGoBack));
                OnPropertyChanged(nameof(CanGoForward));
            }
            currentlyNavigating = null;
        }
    }
}
