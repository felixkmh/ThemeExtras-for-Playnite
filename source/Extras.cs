using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using static System.Reflection.BindingFlags;

namespace Extras
{
    public class Extras : GenericPlugin
    {
        internal static readonly ILogger logger = LogManager.GetLogger();
        internal static Extras Instance { get; private set; }

        internal const string ExtensionName = "ThemeExtras";
        internal const string UserRatingElement = "UserRating";
        internal const string CommunityRatingElement = "CommunityRating";
        internal const string CriticRatingElement = "CriticRating";
        internal const string SettableCompletionStatus = "SettableCompletionStatus";
        internal const string CompletionStatusComboBox = "CompletionStatusComboBox";
        internal const string SettableFavorite = "SettableFavorite";
        internal const string SettableHidden = "SettableHidden";
        internal const string SettableUserScore = "SettableUserScore";
        internal const string BannerElement = "Banner";
        internal const string LinksElement = "Links";
        internal const string MediaControlsElement = "MediaElementControls";

        internal const string ExtrasManifestFileName = "themeExtras.yaml";
        internal const string ThemeManifestFileName = "theme.yaml";

        public string UserLinkIconDir => Path.Combine(GetPluginUserDataPath(), "LinkIcons");

        public ExtrasSettings Settings => settingsViewModel?.Settings;
        public ExtrasSettingsViewModel settingsViewModel { get; set; }

        public override Guid Id { get; } = Guid.Parse("d2039edd-78f5-47c5-b190-72afef560fbe");

        public readonly BannerCache BannerCache;

        public Extras(IPlayniteAPI api) : base(api)
        {
            Instance = this;
            settingsViewModel = new ExtrasSettingsViewModel(this);
            var extendedThemes = ExtendedTheme.CreateExtendedManifests();
            settingsViewModel.ExtendedThemesViewModel = new ViewModels.ThemeExtrasManifestViewModel(extendedThemes);

            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };

            AddCustomElementSupport(new AddCustomElementSupportArgs
            {
                SourceName = ExtensionName,
                ElementList = new List<string>
                {
                    SettableCompletionStatus,
                    SettableFavorite,
                    SettableHidden,
                    SettableUserScore,
                    UserRatingElement,
                    CommunityRatingElement,
                    CriticRatingElement,
                    CompletionStatusComboBox,
                    BannerElement,
                    LinksElement,
                    //MediaControlsElement,
                }.SelectMany(e => Enumerable.Range(0, 3).Select(i => e + (i == 0 ? "" : i.ToString()))).ToList()
            });
            AddSettingsSupport(new AddSettingsSupportArgs { SourceName = ExtensionName, SettingsRoot = "settingsViewModel.Settings" });

            AddPropertiesAsResources<ICommand>(Settings.Commands);

            if (Settings.BackupAndRestore)
            {
                foreach (var theme in extendedThemes)
                {
                    if (!theme.IsDevTheme)
                    {
                        theme.Restore();
                    }
                    else
                    {
                        if (File.Exists(theme.LastChangeFilePath))
                        {
                            File.Delete(theme.LastChangeFilePath);
                        }
                    }
                }
            }
            BannerCache = new BannerCache(extendedThemes.Where(t => t.IsCurrentTheme).ToArray());
        }

        private void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (PlayniteApi.MainView.SelectedGames?.FirstOrDefault() is Game current)
            {
                if (GameSettingsProperties.FirstOrDefault(p => p.Name == e.PropertyName) is PropertyInfo property)
                {
                    if (GameProperties.FirstOrDefault(p => p.Name == property.Name) is PropertyInfo gameProperty)
                    {
                        PlayniteApi.Database.Games.ItemUpdated -= Games_ItemUpdated;
                        Settings.Game.PropertyChanged -= Settings_PropertyChanged;
                        try
                        {
                            var prevSelected = PlayniteApi.MainView.SelectedGames.ToList();
                            var wasSet = false;
                            foreach (var game in PlayniteApi.MainView.SelectedGames)
                            {
                                if (property.PropertyType == gameProperty.PropertyType)
                                {
                                    var currentValue = gameProperty.GetValue(game);
                                    var newValue = property.GetValue(Settings.Game);
                                    if (!object.Equals(currentValue, newValue))
                                    {
                                        gameProperty.SetValue(game, newValue);
                                        wasSet = true;
                                    }
                                }
                            }
                            if (wasSet)
                            {
                                PlayniteApi.Database.Games.Update(PlayniteApi.MainView.SelectedGames);
                                var newSelection = PlayniteApi.MainView.SelectedGames.ToList();
                                if (!Enumerable.SequenceEqual(prevSelected, newSelection))
                                {
                                    PlayniteApi.MainView.SelectGame(prevSelected.First().Id);
                                    PlayniteApi.MainView.SelectGames(prevSelected.Select(g => g.Id));
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Debug(ex, $"Failed to update property {gameProperty.Name} for {current.Name}.");
                        }
                        Settings.Game.PropertyChanged += Settings_PropertyChanged;
                        PlayniteApi.Database.Games.ItemUpdated += Games_ItemUpdated;
                    }
                }
            }
        }

        private void Games_ItemUpdated(object sender, ItemUpdatedEventArgs<Game> e)
        {
            if (PlayniteApi.MainView.SelectedGames?.FirstOrDefault() is Game currentGame)
            {
                if (e?.UpdatedItems?.FirstOrDefault(g => g.NewData.Id == currentGame.Id) is ItemUpdateEvent<Game> update)
                {
                    var editedGame = update.NewData;
                    foreach (var property in GameSettingsProperties)
                    {
                        if (GameProperties.FirstOrDefault(p => p.Name == property.Name) is PropertyInfo gameProperty)
                        {
                            Settings.Game.PropertyChanged -= Settings_PropertyChanged;
                            try
                            {
                                var newValue = gameProperty.GetValue(editedGame);
                                var currentValue = property.GetValue(Settings.Game);
                                if (property.PropertyType == gameProperty.PropertyType
                                    && !object.Equals(currentValue, newValue))
                                {
                                    property.SetValue(Settings.Game, newValue);
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.Debug(ex, $"Failed to update property {gameProperty.Name} for {editedGame.Name}.");
                            }
                            Settings.Game.PropertyChanged += Settings_PropertyChanged;
                        }
                    }
                }
            }
        }

        private void AddPropertiesAsResources<T>(object source)
        {
            if (Application.Current is Application app)
            {
                ResourceDictionary resourceDictionary = new ResourceDictionary();
                var settingsType = source.GetType();
                var properties = settingsType.GetProperties();
                var typedProperties = properties.Where(p => p.PropertyType == typeof(T));
                var window = app.Windows.OfType<Window>().FirstOrDefault(w => w.Name == "WindowMain");
                foreach (var typedProperty in typedProperties)
                {
                    try
                    {
                        if (typedProperty.GetValue(source) is T value)
                        {
                            resourceDictionary[typedProperty.Name] = value;
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Debug(ex, $"Failed to add {typedProperty.Name} as resource.");
                    }
                }
                app.Resources.MergedDictionaries.Add(resourceDictionary);
            }
        }

        DesktopView lastView;
        IEnumerable<Game> lastSelected;

        public static readonly PropertyInfo[] GameSettingsProperties
            = typeof(GameProperties)
            .GetProperties()
            .ToArray();
        public static readonly PropertyInfo[] GameProperties = typeof(Game).GetProperties()
            .Where(p => p.CanRead && p.CanWrite)
            .Where(p => GameSettingsProperties.Any(o => o.Name == p.Name))
            .ToArray();

        public override void OnGameSelected(OnGameSelectedEventArgs args)
        {
            //if (args.NewValue.Count <= 1)
            //{
            //    Settings.Menus.OnPropertyChanged(nameof(Menus.EMLGameMenuItems));
            //    Settings.Menus.OnPropertyChanged(nameof(Menus.BackgroundChangerGameMenuItems));
            //}
            Settings.Game.PropertyChanged -= Settings_PropertyChanged;
            var prevSelected = lastSelected;
            var prevMode = lastView;
            lastSelected = args.NewValue;
            lastView = PlayniteApi.MainView.ActiveDesktopView;
            if (prevMode != lastView && lastSelected == null && prevSelected != null && Settings.EnableSelectionPreservation)
            {
                PlayniteApi.MainView.SelectGames(prevSelected.Select(g => g.Id));
            }

            if (args.NewValue?.FirstOrDefault() is Game current)
            {
                foreach (var property in GameSettingsProperties)
                {
                    if (GameProperties.FirstOrDefault(p => p.Name == property.Name) is PropertyInfo gameProperty)
                    {
                        if (property.PropertyType == gameProperty.PropertyType)
                        {
                            var newValue = gameProperty.GetValue(current);
                            var currentValue = property.GetValue(Settings.Game);
                            if (!object.Equals(currentValue, newValue))
                                property.SetValue(Settings.Game, newValue);
                        }
                    }
                }
            }
            Settings.Game.PropertyChanged += Settings_PropertyChanged;
        }

        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            Game selectedGame = args.Games.First();

            if (Settings.EnableGameMenuRating)
            {
                yield return new GameMenuItem
                {
                    Description = "1 Star",
                    MenuSection = $"Theme Extras | Ratings | Set User Rating",

                    Action = (mainMenuItem) =>
                    {
                        var games = args.Games.Distinct();

                        foreach (Game game in games)
                        {
                            game.UserScore = 20;
                            Playnite.SDK.API.Instance.Database.Games.Update(game);
                        }
                    }
                };

                yield return new GameMenuItem
                {
                    Description = "2 Stars",
                    MenuSection = $"Theme Extras | Ratings | Set User Rating",

                    Action = (mainMenuItem) =>
                    {
                        var games = args.Games.Distinct();

                        foreach (Game game in games)
                        {
                            game.UserScore = 40;
                            Playnite.SDK.API.Instance.Database.Games.Update(game);
                        }
                    }
                };

                yield return new GameMenuItem
                {
                    Description = "3 Stars",
                    MenuSection = $"Theme Extras | Ratings | Set User Rating",

                    Action = (mainMenuItem) =>
                    {
                        var games = args.Games.Distinct();

                        foreach (Game game in games)
                        {
                            game.UserScore = 60;
                            Playnite.SDK.API.Instance.Database.Games.Update(game);
                        }
                    }
                };

                yield return new GameMenuItem
                {
                    Description = "4 Stars",
                    MenuSection = $"Theme Extras | Ratings | Set User Rating",

                    Action = (mainMenuItem) =>
                    {
                        var games = args.Games.Distinct();

                        foreach (Game game in games)
                        {
                            game.UserScore = 80;
                            Playnite.SDK.API.Instance.Database.Games.Update(game);
                        }
                    }
                };

                yield return new GameMenuItem
                {
                    Description = "5 Stars",
                    MenuSection = $"Theme Extras | Ratings | Set User Rating",

                    Action = (mainMenuItem) =>
                    {
                        var games = args.Games.Distinct();

                        foreach (Game game in games)
                        {
                            game.UserScore = 100;
                            Playnite.SDK.API.Instance.Database.Games.Update(game);
                        }
                    }
                };
            }
        }

        public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
        {
#if DEBUG
            yield return new MainMenuItem()
            {
                Description = "Test Notification",
                Action = m => PlayniteApi.Notifications.Add(
                    new NotificationMessage(
                        Guid.NewGuid().ToString(), 
                        "Hallo, das ist eine mittellange Benachrichtigung.", 
                        NotificationType.Info, 
                        () => API.Instance.Dialogs.ShowMessage("Hello, I'm a notification."))
                    ),
                MenuSection = ""
            };
#else
            return null;
#endif
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
            if (!Directory.Exists(UserLinkIconDir))
            {
                Directory.CreateDirectory(UserLinkIconDir);
            }
            // Add code to be executed when Playnite is initialized.
            settingsViewModel.Settings.PropertyChanged += Settings_PropertyChanged;
            PlayniteApi.Database.Games.ItemUpdated += Games_ItemUpdated;
            if (PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop)
            {

            }
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
            if (!Application.Current.Resources.Contains("Extras_CompletionTextColor"))
            {
                Application.Current.Resources.Add("Extras_CompletionTextColor", new SolidColorBrush(Colors.White) { Opacity = 1 });
            }
            string name = args.Name;
            if (name.EndsWith("1") || name.EndsWith("2"))
            {
                name = name.Substring(0, name.Length - 1);
            }
            return GenerateCustomElement(name);
        }

        private Control GenerateCustomElement(string name)
        {
            switch (name)
            {
                case SettableCompletionStatus:
                    return new Controls.StylableUserControl(new ViewModels.CompletionStatusViewModel());
                case CompletionStatusComboBox:
                    return new Controls.CompletionStatus(new ViewModels.CompletionStatusViewModel());
                case SettableFavorite:
                    return new Controls.StylableUserControl(new ViewModels.FavoriteViewModel());
                case SettableHidden:
                    return new Controls.StylableUserControl(new ViewModels.GamePropertyViewModel<bool>(nameof(Game.Hidden), g => g.Hidden, (g, v) => g.Hidden = v));
                case SettableUserScore:
                    return new Controls.StylableUserControl(new ViewModels.GamePropertyViewModel<int?>(nameof(Game.UserScore), g => g.UserScore, (g, v) => g.UserScore = v));
                case UserRatingElement:
                    return new Controls.UserRating();
                case CommunityRatingElement:
                    return new Controls.CommunityRating();
                case CriticRatingElement:
                    return new Controls.CriticRating();
                case BannerElement:
                    return new Controls.Banner(BannerCache);
                case LinksElement:
                    return new Controls.Links();
                case MediaControlsElement:
                    return new Controls.MediaElementControls();
                default:
                    return null;
            }
        }

        public override void OnApplicationStopped(OnApplicationStoppedEventArgs args)
        {
            // Add code to be executed when Playnite is shutting down.
            var extendedThemes = settingsViewModel.ExtendedThemesViewModel.Themes;
            if (Settings.BackupAndRestore)
            {
                foreach (var theme in extendedThemes)
                {
                    if (!theme.IsDevTheme)
                    {
                        theme.Backup();
                    }
                    else
                    {
                        if (File.Exists(theme.LastChangeFilePath))
                        {
                            File.Delete(theme.LastChangeFilePath);
                        }
                    }
                }
            }
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