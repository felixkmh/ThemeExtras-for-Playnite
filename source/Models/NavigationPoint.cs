using Extras.Abstractions.Navigation;
using Playnite.SDK;
using Playnite.SDK.Models;
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

        public string DisplayName => Title;

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
            return DisplayName;
        }
    }

    public class LibraryNavigation : INavigationPoint
    {
        public IList<Guid> SelectedIds { get; set; } = new List<Guid>();

        public Playnite.SDK.DesktopView DesktopView { get; set; }

        private Lazy<string> displayName;

        public string DisplayName => displayName.Value;

        public LibraryNavigation()
        {
            displayName = new Lazy<string>(GetDisplayName);
        }

        private string GetDisplayName()
        {
            var sb = new StringBuilder();
            switch (DesktopView)
            {
                case DesktopView.Details:
                    sb.Append(ResourceProvider.GetString("LOCDetailsViewLabel"));
                    break;
                case DesktopView.Grid:
                    sb.Append(ResourceProvider.GetString("LOCGridViewLabel"));
                    break;
                case DesktopView.List:
                    sb.Append(ResourceProvider.GetString("LOCListViewLabel"));
                    break;
                default:
                    break;
            }
            var selectedGames = SelectedIds
                .Select(id => API.Instance.Database.Games.Get(id))
                .OfType<Game>()
                .ToList();
            if (selectedGames.Any())
            {
                sb.Append(" - ");
                sb.Append(selectedGames.First().Name);
            }
            if (selectedGames.Count > 1)
            {
                sb.Append(", ...");
            }
            return sb.ToString();
        }

        public void Navigate()
        {
            var api = Playnite.SDK.API.Instance;

            if (api.MainView.ActiveDesktopView != DesktopView)
            {
                api.MainView.ActiveDesktopView = DesktopView;
            }
            api.MainView.SelectGames(SelectedIds);
            api.MainView.SwitchToLibraryView();
        }

        public override string ToString()
        {
            return DisplayName;
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
