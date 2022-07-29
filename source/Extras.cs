using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Extras
{
    public class Extras : GenericPlugin
    {
        internal static readonly ILogger logger = LogManager.GetLogger();

        internal const string ExtensionName = "ThemeExtras";
        internal const string UserRatingElement = "UserRating";
        internal const string CommunityRatingElement = "CommunityRating";
        internal const string CriticRatingElement = "CriticRating";

        public ExtrasSettings Settings => settingsViewModel.Settings;
        public ExtrasSettingsViewModel settingsViewModel { get; set; }

        public override Guid Id { get; } = Guid.Parse("d2039edd-78f5-47c5-b190-72afef560fbe");

        public Extras(IPlayniteAPI api) : base(api)
        {
            settingsViewModel = new ExtrasSettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = false
            };
            AddCustomElementSupport(new AddCustomElementSupportArgs
            {
                SourceName = ExtensionName,
                ElementList = new List<string>
                {
                    UserRatingElement,
                    CommunityRatingElement,
                    CriticRatingElement
                }.SelectMany(e => Enumerable.Range(0, 3).Select(i => e + (i == 0 ? "" : i.ToString()))).ToList()
            });
            AddSettingsSupport(new AddSettingsSupportArgs { SourceName = ExtensionName, SettingsRoot = "settingsViewModel.Settings" });
        }

        public override void OnGameInstalled(OnGameInstalledEventArgs args)
        {
            // Add code to be executed when game is finished installing.
        }

        public override void OnGameStarted(OnGameStartedEventArgs args)
        {
            Settings.RunningGames.AddMissing(args.Game);
            Settings.IsAnyGameRunning = Settings.RunningGames.Count > 0;
        }

        public override void OnGameStarting(OnGameStartingEventArgs args)
        {
            // Add code to be executed when game is preparing to be started.
        }

        public override void OnGameStopped(OnGameStoppedEventArgs args)
        {
            // Add code to be executed when game is preparing to be started.
            Settings.RunningGames.Remove(args.Game);
            Settings.IsAnyGameRunning = Settings.RunningGames.Count > 0;
        }

        public override void OnGameUninstalled(OnGameUninstalledEventArgs args)
        {
            // Add code to be executed when game is uninstalled.
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            // Add code to be executed when Playnite is initialized.
        }

        public override Control GetGameViewControl(GetGameViewControlArgs args)
        {
            if (!Application.Current.Resources.Contains("Extras_FilledStarBrush"))
            {
                Application.Current.Resources.Add("Extras_FilledStarBrush", new SolidColorBrush(Colors.White));
            }
            if (!Application.Current.Resources.Contains("Extras_EmptyStarBrush"))
            {
                Application.Current.Resources.Add("Extras_EmptyStarBrush", new SolidColorBrush(Colors.White) { Opacity = 0.3 });
            }
            switch (args.Name)
            {
                case string s when s.StartsWith(UserRatingElement):
                    return new Controls.UserRating();
                case string s when s.StartsWith(CommunityRatingElement):
                    return new Controls.CommunityRating();
                case string s when s.StartsWith(CriticRatingElement):
                    return new Controls.CriticRating();
                default:
                    return null;
            }
        }

        public override void OnApplicationStopped(OnApplicationStoppedEventArgs args)
        {
            // Add code to be executed when Playnite is shutting down.
        }

        public override void OnLibraryUpdated(OnLibraryUpdatedEventArgs args)
        {
            // Add code to be executed when library is updated.
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settingsViewModel;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new ExtrasSettingsView();
        }
    }
}