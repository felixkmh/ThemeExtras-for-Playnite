using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        internal const string ExtensionName = "ThemeExtras";
        internal const string UserRatingElement = "UserRating";
        internal const string CommunityRatingElement = "CommunityRating";
        internal const string CriticRatingElement = "CriticRating";

        public ExtrasSettings Settings => settingsViewModel?.Settings;
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

            AddSettingsAsResources<ICommand>();
        }

        private void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (PlayniteApi.MainView.SelectedGames?.FirstOrDefault() is Game current)
            {
                if (SettingsProperties.FirstOrDefault(p => p.Name == e.PropertyName) is PropertyInfo property)
                {
                    if (GameProperties.FirstOrDefault(p => p.Name == property.Name) is PropertyInfo gameProperty)
                    {
                        PlayniteApi.Database.Games.ItemUpdated -= Games_ItemUpdated;
                        Settings.PropertyChanged -= Settings_PropertyChanged;
                        try
                        {
                            if (property.PropertyType == gameProperty.PropertyType)
                            {
                                var currentValue = gameProperty.GetValue(current);
                                var newValue = property.GetValue(Settings);
                                if (!object.Equals(currentValue, newValue))
                                {
                                    gameProperty.SetValue(current, newValue);
                                    PlayniteApi.Database.Games.Update(current);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Debug(ex, $"Failed to update property {gameProperty.Name} for {current.Name}.");
                        }
                        Settings.PropertyChanged += Settings_PropertyChanged;
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
                    foreach (var property in SettingsProperties)
                    {
                        if (GameProperties.FirstOrDefault(p => p.Name == property.Name) is PropertyInfo gameProperty)
                        {
                            Settings.PropertyChanged -= Settings_PropertyChanged;
                            try
                            {
                                var newValue = gameProperty.GetValue(editedGame);
                                var currentValue = property.GetValue(Settings);
                                if (property.PropertyType == gameProperty.PropertyType
                                    && !object.Equals(currentValue,newValue))
                                {
                                    property.SetValue(Settings, newValue);
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.Debug(ex, $"Failed to update property {gameProperty.Name} for {editedGame.Name}.");
                            }
                            Settings.PropertyChanged += Settings_PropertyChanged;
                        }
                    }
                }
            }
        }

        private void AddSettingsAsResources<T>()
        {
            if (Application.Current is Application app)
            {
                ResourceDictionary resourceDictionary = new ResourceDictionary();
                var settings = Settings ?? new ExtrasSettings();
                var settingsType = typeof(ExtrasSettings);
                var properties = settingsType.GetProperties();
                var typedProperties = properties.Where(p => p.PropertyType == typeof(T));
                var window = app.Windows.OfType<Window>().FirstOrDefault(w => w.Name == "WindowMain");
                foreach (var typedProperty in typedProperties)
                {
                    try
                    {
                        if (typedProperty.GetValue(settings) is T value)
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

        public static readonly PropertyInfo[] SettingsProperties 
            = typeof(ExtrasSettings)
            .GetProperties()
            .Where(p => p.GetCustomAttribute<GamePropertyAttribute>(false) != null)
            .ToArray();
        public static readonly PropertyInfo[] GameProperties = typeof(Game).GetProperties()
            .Where(p => p.CanRead && p.CanWrite)
            .Where(p => SettingsProperties.Any(o => o.Name == p.Name))
            .ToArray();

        public override void OnGameSelected(OnGameSelectedEventArgs args)
        {
            Settings.PropertyChanged -= Settings_PropertyChanged;
            var prevSelected = lastSelected;
            var prevMode = lastView;
            lastSelected = args.NewValue;
            lastView = PlayniteApi.MainView.ActiveDesktopView;
            if (prevMode != lastView && lastSelected == null && prevSelected != null)
            {
                PlayniteApi.MainView.SelectGames(prevSelected.Select(g => g.Id));
            }
            if (args.NewValue?.FirstOrDefault() is Game current)
            {
                foreach(var property in SettingsProperties)
                {
                    if (GameProperties.FirstOrDefault(p => p.Name == property.Name) is PropertyInfo gameProperty)
                    {
                        if (property.PropertyType == gameProperty.PropertyType)
                        {
                            var newValue = gameProperty.GetValue(current);
                            var currentValue = property.GetValue(Settings);
                            if (!object.Equals(currentValue, newValue))
                                property.SetValue(Settings, newValue);
                        }
                    }
                }
            }
            Settings.PropertyChanged += Settings_PropertyChanged;
        }

        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            Game GameMenu = args.Games.First();

            List<GameMenuItem> gameMenuItems = new List<GameMenuItem>();

            gameMenuItems.Add(new GameMenuItem
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
            });

            gameMenuItems.Add(new GameMenuItem
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
            });

            gameMenuItems.Add(new GameMenuItem
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
            });

            gameMenuItems.Add(new GameMenuItem
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
            });

            gameMenuItems.Add(new GameMenuItem
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
            });

            return gameMenuItems;
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

        public override async void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            // Add code to be executed when Playnite is initialized.
            settingsViewModel.Settings.PropertyChanged += Settings_PropertyChanged;
            PlayniteApi.Database.Games.ItemUpdated += Games_ItemUpdated;
            if (PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop)
            {
                lastView = PlayniteApi.MainView.ActiveDesktopView;
                await Task.Delay(5000);

                var themeId = PlayniteApi.ApplicationSettings.DesktopTheme;
                var themesDir = Path.Combine(PlayniteApi.Paths.ConfigurationPath, "Themes", "Desktop");
                var manifestPaths = Directory.GetFiles(themesDir, "theme.yaml", SearchOption.AllDirectories);
                var yaml = new YamlDotNet.Serialization.DeserializerBuilder()
                    .IgnoreUnmatchedProperties()
                    .Build();
                var manifests = await Task.Run(() =>
                {
                    return manifestPaths.ToDictionary(p => p, p =>
                    {
                        try
                        {
                            using (var reader = File.OpenText(p))
                            {
                                return yaml.Deserialize<Models.ThemeManifest>(reader);
                            }
                        }
                        catch (Exception ex){
                            Extras.logger.Debug(ex, $"Failed to deserialize manifest file at ${p}.");
                        }
                        return null;
                    });
                });

                Models.ThemeExtrasManifest extrasManifest = await Task.Run(() =>
                {
                    try
                    {
                        if (manifests?.FirstOrDefault(m => m.Value.Id == themeId) is var current && current.HasValue)
                        {
                            var themeDir = Path.GetDirectoryName(current.Value.Key);
                            if (!string.IsNullOrEmpty(themeDir) && Directory.GetFiles(themeDir, "themeExtras.yaml").FirstOrDefault() is string extraManifestPath)
                            {
                                using (var reader = File.OpenText(extraManifestPath))
                                {
                                        return yaml.Deserialize<Models.ThemeExtrasManifest>(reader);
                                    }
                                }
                            }
                        }
                    catch (Exception ex)
                    {
                        Extras.logger.Debug(ex, "Failed to deserialize extra manifest file.");
                    }
                    return null;
                });

                if (extrasManifest is object)
                {
                    var notInstalled = extrasManifest.Recommendations.Where(r => !PlayniteApi.Addons.Addons.Contains(r.AddonId)).ToHashSet();
                    if (notInstalled.Any())
                    {
                        // PlayniteApi.Dialogs.ShowMessage($"Found {notInstalled.Count} not installed recommendation{(notInstalled.Count > 1 ? "s" : "")} for your current theme:\n{string.Join("\n", notInstalled.Select(r => r.AddonName))}.\n\nDo you want to install them?", "Addon Recommendations", MessageBoxButton.YesNo);
                    }
                }
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