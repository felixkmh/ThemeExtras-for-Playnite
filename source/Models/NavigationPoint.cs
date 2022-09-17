using Extras.Abstractions.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Extras.Models
{
    public class ViewNavigation : INavigationPoint
    {
        public string Title { get; set; } = "";
        public ICommand ActivationCommand { get; set; }

        public bool Equals(INavigationPoint other)
        {
            if (other is ViewNavigation otherViewNavigation)
            {
                return ActivationCommand == otherViewNavigation.ActivationCommand;
            }
            return false;
        }

        public void Navigate()
        {
            ActivationCommand?.Execute(null);
        }

        public override string ToString()
        {
            return $"Switch to {Title} view.";
        }
    }

    public class LibraryNavigation : INavigationPoint
    {
        public IList<Guid> SelectedIds { get; set; } = new List<Guid>();

        public Playnite.SDK.DesktopView DesktopView { get; set; }

        public void Navigate()
        {
            var api = Playnite.SDK.API.Instance;

            if (api.MainView.ActiveDesktopView != DesktopView)
            {
                // api.MainView.ActiveDesktopView = DesktopView;
            }
            api.MainView.SelectGames(SelectedIds);
            api.MainView.SwitchToLibraryView();
        }

        public override string ToString()
        {
            return String.Format("{0} - [{1}]", DesktopView.ToString(), string.Join(", ", SelectedIds.Select(id => Playnite.SDK.API.Instance.Database.Games.Get(id).Name)));
        }

        public override int GetHashCode()
        {
            int hashCode = 1921412036;
            hashCode = hashCode * -1521134295 + EqualityComparer<IList<Guid>>.Default.GetHashCode(SelectedIds);
            hashCode = hashCode * -1521134295 + DesktopView.GetHashCode();
            return hashCode;
        }

        public bool Equals(INavigationPoint other)
        {
            if (other is LibraryNavigation otherLibraryNavigation)
            {
                return DesktopView == otherLibraryNavigation.DesktopView
                    && SelectedIds.SequenceEqual(otherLibraryNavigation.SelectedIds);
            }
            return false;
        }
    }
}
