using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace Extras
{
    [System.AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public class GamePropertyAttribute : DontSerializeAttribute {}

    public class GameProperties : ObservableObject
    {
        public static readonly GameProperties Instance = new GameProperties();

        private GameProperties() {}

        private bool hidden;
        public bool Hidden { get => hidden; set => SetValue(ref hidden, value); }

        private string notes;
        public string Notes { get => notes; set => SetValue(ref notes, value); }

        private bool favorite;
        public bool Favorite { get => favorite; set => SetValue(ref favorite, value); }
    }

    public class ExtrasSettings : ObservableObject
    {
        private bool enableGameMenuRating = false;
        public bool EnableGameMenuRating { get => enableGameMenuRating; set => SetValue(ref enableGameMenuRating, value); }

        [DontSerialize]
        public CommandSettings Commands { get; } = CommandSettings.Instance;

        [DontSerialize]
        public GameProperties Game { get; } = GameProperties.Instance;

        [DontSerialize]
        public ObservableCollection<Game> RunningGames { get; } = new ObservableCollection<Game>();

        private bool isAnyGameRunning = false;
        [DontSerialize]
        public bool IsAnyGameRunning { get => isAnyGameRunning; set => SetValue(ref isAnyGameRunning, value); }

        [DontSerialize]
        public IValueConverter IntToRatingBrushConverter { get; } = new Converters.IntToRatingBrushConverter();

        
    }

    public class ExtrasSettingsViewModel : ObservableObject, ISettings
    {
        private readonly Extras plugin;
        private ExtrasSettings editingClone { get; set; }

        private ExtrasSettings settings;

        public ExtrasSettings Settings
        {
            get => settings;
            set
            {
                settings = value;
                OnPropertyChanged();
            }
        }

        public ExtrasSettingsViewModel(Extras plugin)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            this.plugin = plugin;

            // Load saved settings.
            var savedSettings = plugin.LoadPluginSettings<ExtrasSettings>();

            // LoadPluginSettings returns null if not saved data is available.
            if (savedSettings != null)
            {
                Settings = savedSettings;
            }
            else
            {
                Settings = new ExtrasSettings();
            }
        }

        public void BeginEdit()
        {
            // Code executed when settings view is opened and user starts editing values.
            editingClone = Serialization.GetClone(Settings);
        }

        public void CancelEdit()
        {
            // Code executed when user decides to cancel any changes made since BeginEdit was called.
            // This method should revert any changes made to Option1 and Option2.
            Settings = editingClone;
        }

        public void EndEdit()
        {
            // Code executed when user decides to confirm changes made since BeginEdit was called.
            // This method should save settings made to Option1 and Option2.
            plugin.SavePluginSettings(Settings);
        }

        public bool VerifySettings(out List<string> errors)
        {
            // Code execute when user decides to confirm changes made since BeginEdit was called.
            // Executed before EndEdit is called and EndEdit is not called if false is returned.
            // List of errors is presented to user if verification fails.
            errors = new List<string>();
            return true;
        }
    }

    public class CommandSettings
    {
        public static readonly CommandSettings Instance = new CommandSettings();

        private CommandSettings() {}

        public static void UpdateGames(object sender, EventArgs args)
        {
            API.Instance.Database.Games.Update(API.Instance.MainView.SelectedGames);
        }

        public ICommand UpdateGamesCommand { get; }
            = new RelayCommand(() =>
            {
                UpdateGames(null, null);
            }, () => API.Instance?.MainView?.SelectedGames?.Count() > 0);

        public ICommand ResetScoreCommand { get; }
            = new RelayCommand<string>(kinds =>
            {
                foreach (var game in API.Instance.MainView.SelectedGames)
                {
                    if (kinds.Contains("User"))
                    {
                        game.UserScore = null;
                    }
                    if (kinds.Contains("Community"))
                    {
                        game.CommunityScore = null;
                    }
                    if (kinds.Contains("Critic"))
                    {
                        game.CriticScore = null;
                    }
                }
                API.Instance.Database.Games.Update(API.Instance.MainView.SelectedGames);
            }, _ => API.Instance?.MainView?.SelectedGames?.Count() > 0);

        public ICommand OpenGameAssetFolderCommand { get; }
            = new RelayCommand(
            () =>
            {
                var api = API.Instance;
                if (api?.MainView?.SelectedGames?.FirstOrDefault() is Game selected)
                {
                    var path = Path.Combine(api.Database.DatabasePath, "files", selected.Id.ToString());
                    if (Directory.Exists(path))
                    {
                        System.Diagnostics.Process.Start(path);
                    }
                }
            },
            () =>
            {
                var api = API.Instance;
                if (api?.MainView?.SelectedGames?.FirstOrDefault() is Game selected)
                {
                    var path = Path.Combine(api.Database.DatabasePath, "files", selected.Id.ToString());
                    return Directory.Exists(path);
                }
                return false;
            });

        public static void OpenPlayniteSettings(object sender, EventArgs args)
        {
            if (Application.Current?.Windows?.OfType<Window>().FirstOrDefault(w => w.Name == "WindowMain") is Window mainWindow)
            {
                if (mainWindow.InputBindings.OfType<KeyBinding>().FirstOrDefault(b => b.Key == Key.F4) is KeyBinding binding)
                {
                    if (binding.Command.CanExecute(null))
                    {
                        binding.Command.Execute(null);
                    }
                }
            }
        }

        public ICommand OpenPlayniteSettingsCommand { get; }
            = new RelayCommand(
            () =>
            {
                OpenPlayniteSettings(null, null);
            },
            () => true);

        public ICommand OpenAddonWindowCommand { get; }
            = new RelayCommand(
            () =>
            {
                if (Application.Current?.Windows?.OfType<Window>().FirstOrDefault(w => w.Name == "WindowMain") is Window mainWindow)
                {
                    if (mainWindow.InputBindings.OfType<KeyBinding>().FirstOrDefault(b => b.Key == Key.F9) is KeyBinding binding)
                    {
                        if (binding.Command.CanExecute(null))
                        {
                            binding.Command.Execute(null);
                        }
                    }
                }
            },
            () => true);

        public ICommand SwitchModeCommand { get; }
            = new RelayCommand(
            () =>
            {
                if (Application.Current?.Windows?.OfType<Window>().FirstOrDefault(w => w.Name == "WindowMain") is Window mainWindow)
                {
                    if (mainWindow.InputBindings.OfType<KeyBinding>().FirstOrDefault(b => b.Key == Key.F11) is KeyBinding binding)
                    {
                        if (binding.Command.CanExecute(null))
                        {
                            binding.Command.Execute(null);
                        }
                    }
                }
            },
            () => true);

        public ICommand OpenPluginSettingsCommand { get; }
            = new RelayCommand<string>(
            id =>
            {
                var api = API.Instance;
                if (api.Addons?.Plugins?.FirstOrDefault(p => string.Equals(p.Id.ToString(), id, System.StringComparison.InvariantCultureIgnoreCase)) is Plugin plugin)
                {
                    try
                    {
                        plugin.OpenSettingsView();
                    }
                    catch (System.Exception ex)
                    {
                        Extras.logger.Debug(ex, $"Failed to open plugin settings for plugin with Id: {id}");
                    }
                }
            },
            id =>
            {
                var api = API.Instance;
                if (api.Addons?.Plugins?.FirstOrDefault(p => string.Equals(p.Id.ToString(), id, System.StringComparison.InvariantCultureIgnoreCase)) is Plugin plugin)
                {
                    if (plugin is GenericPlugin genericPlugin) return genericPlugin.Properties?.HasSettings ?? false;
                    if (plugin is LibraryPlugin libraryPlugin) return libraryPlugin.Properties?.HasSettings ?? false;
                    if (plugin is MetadataPlugin metadataPlugin) return metadataPlugin.Properties?.HasSettings ?? false;
                }
                return false;
            });

        public ICommand OpenPluginConfigDirCommand { get; }
            = new RelayCommand<string>(
            id =>
            {
                var api = API.Instance;
                if (api.Addons?.Plugins?.FirstOrDefault(p => string.Equals(p.Id.ToString(), id, System.StringComparison.InvariantCultureIgnoreCase)) is Plugin plugin)
                {
                    try
                    {
                        System.Diagnostics.Process.Start(plugin.GetPluginUserDataPath());
                    }
                    catch (System.Exception ex)
                    {
                        Extras.logger.Debug(ex, $"Failed to open config directory for plugin with Id: {id}");
                    }
                }
            },
            id =>
            {
                var api = API.Instance;
                if (api.Addons?.Plugins?.FirstOrDefault(p => string.Equals(p.Id.ToString(), id, System.StringComparison.InvariantCultureIgnoreCase)) is Plugin plugin)
                {
                    return string.IsNullOrEmpty(plugin.GetPluginUserDataPath());
                }
                return false;
            });

        public ICommand OpenPlayniteLogCommand { get; }
            = new RelayCommand<string>(
            id =>
            {
                var api = API.Instance;
                if (api?.Paths?.ConfigurationPath is string path)
                {
                    var logPath = Path.Combine(path, "playnite.log");
                    try
                    {
                        if (File.Exists(logPath))
                        {
                            System.Diagnostics.Process.Start(logPath);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Extras.logger.Debug(ex, $"Failed to open {logPath}");
                    }
                }
            },
            id => true);

        public ICommand OpenExtensionsLogCommand { get; }
            = new RelayCommand<string>(
            id =>
            {
                var api = API.Instance;
                if (api?.Paths?.ConfigurationPath is string path)
                {
                    var logPath = Path.Combine(path, "extensions.log");
                    try
                    {
                        if (File.Exists(logPath))
                        {
                            System.Diagnostics.Process.Start(logPath);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Extras.logger.Debug(ex, $"Failed to open {logPath}");
                    }
                }
            },
            id => true);
    }
}