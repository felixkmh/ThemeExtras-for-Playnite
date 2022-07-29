using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Extras
{
    public class ExtrasSettings : ObservableObject
    {
        [DontSerialize]
        public ObservableCollection<Game> RunningGames { get; } = new ObservableCollection<Game>();

        private bool isAnyGameRunning = false;

        [DontSerialize]
        public bool IsAnyGameRunning { get => isAnyGameRunning; set => SetValue(ref isAnyGameRunning, value); }

        [DontSerialize]
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

        [DontSerialize]
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

        [DontSerialize]
        public ICommand OpenPlayniteSettingsCommand { get; }
            = new RelayCommand(
            () =>
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
            },
            () => true);

        [DontSerialize]
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

        [DontSerialize]
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

        [DontSerialize]
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
}