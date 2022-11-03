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
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Animation;

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

        private string completion;
        public string Completion { get => completion; set => SetValue(ref completion, value); }
    }

    public class ExtrasSettings : ObservableObject
    {
        private bool enableGameMenuRating = false;
        public bool EnableGameMenuRating { get => enableGameMenuRating; set => SetValue(ref enableGameMenuRating, value); }

        private bool enableSelectionPreservation = true;
        public bool EnableSelectionPreservation { get => enableSelectionPreservation; set => SetValue(ref enableSelectionPreservation, value); }

        private bool backupAndRestore = true;
        public bool BackupAndRestore { get => backupAndRestore; set => SetValue(ref backupAndRestore, value); }

        private Dictionary<string, Dictionary<string, string>> persistentResources = new Dictionary<string, Dictionary<string, string>>();
        public Dictionary<string, Dictionary<string, string>> PersistentResources { get => persistentResources; set => SetValue(ref persistentResources, value); }

        [DontSerialize]
        public CommandSettings Commands { get; } = CommandSettings.Instance;

        [DontSerialize]
        public GameProperties Game { get; } = GameProperties.Instance;

        [DontSerialize]
        public Menus Menus { get; } = Menus.Instance;

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

        public ViewModels.ThemeExtrasManifestViewModel ExtendedThemesViewModel { get; set; }

        public ICommand OpenUserLinkIconDir => new RelayCommand(() => System.Diagnostics.Process.Start(Extras.Instance.UserLinkIconDir));

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

    public class Menus : ObservableObject
    {
        public static Menus Instance = new Menus();

        private bool isOpen;
        public bool IsOpen 
        { 
            get => isOpen;
            set
            {
                SetValue(ref isOpen, value);
                if (value)
                {
                    OnPropertyChanged(nameof(EMLGameMenuItems));
                    OnPropertyChanged(nameof(BackgroundChangerGameMenuItems));
                }
            }
        }

        public IEnumerable<object> EMLGameMenuItems
        {
            get
            {
                var api = API.Instance;
                var id = "705fdbca-e1fc-4004-b839-1d040b8b4429";
                if (IsOpen && (api.MainView.SelectedGames?.Any() ?? false))
                    if (api.Addons?.Plugins?.FirstOrDefault(p => string.Equals(p.Id.ToString(), id, System.StringComparison.InvariantCultureIgnoreCase)) is Plugin plugin)
                    {
                        try
                        {
                            var items = CreateGameMenuItems(api, plugin);
                            var settingsItem = new MenuItem { Header = ResourceProvider.GetString("LOCSettingsLabel"), Command = Extras.Instance.Settings.Commands.OpenPluginSettingsCommand, CommandParameter = id };
                            if (items.Count > 0)
                            {
                                items.Insert(0, new Separator());
                            }
                            items.Insert(0, settingsItem);
                            return items;
                        }
                        catch (System.Exception ex)
                        {
                            Extras.logger.Debug(ex, $"Failed to create ExtraMetadata menu items.");
                        }
                    }
                return null;
            }
        }

        public IEnumerable<object> BackgroundChangerGameMenuItems
        {
            get
            {
                var api = API.Instance;
                var id = "3afdd02b-db6c-4b60-8faa-2971d6dfad2a";
                if (IsOpen && (api.MainView.SelectedGames?.Any() ?? false))
                    if (api.Addons?.Plugins?.FirstOrDefault(p => string.Equals(p.Id.ToString(), id, System.StringComparison.InvariantCultureIgnoreCase)) is Plugin plugin)
                    {
                        try
                        {
                            var items = CreateGameMenuItems(api, plugin);
                            var settingsItem = new MenuItem { Header = ResourceProvider.GetString("LOCSettingsLabel"), Command = Extras.Instance.Settings.Commands.OpenPluginSettingsCommand, CommandParameter = id };
                            if (items.Count > 0)
                            {
                                items.Insert(0, new Separator());
                            }
                            items.Insert(0, settingsItem);
                            return items;
                        }
                        catch (System.Exception ex)
                        {
                            Extras.logger.Debug(ex, $"Failed to create BackgroundChanger menu items.");
                        }
                    }
                return null;
            }
        }

        private static IList<object> CreateGameMenuItems(IPlayniteAPI api, Plugin plugin)
        {
            var menuItems = new ObservableCollection<object>();
            List<Game> games = api.MainView.SelectedGames?.Take(1).ToList() ?? new List<Game>();
            foreach (var item in plugin.GetGameMenuItems(new GetGameMenuItemsArgs() { Games = games }))
            {
                var path = item.MenuSection.Replace("@", "").Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries).Skip(1).ToList();
                var currentMenuItems = menuItems;
                for (int i = 0; i < path.Count; ++i)
                {
                    var section = path[i];
                    if (currentMenuItems.OfType<MenuItem>().FirstOrDefault(mi => mi.Header as string == section) is MenuItem menuItem)
                    {
                        currentMenuItems = menuItem.ItemsSource as ObservableCollection<object>;
                    }
                    else
                    {
                        var itemList = new ObservableCollection<object>();
                        currentMenuItems.Add(new MenuItem { Header = section, ItemsSource = itemList });
                        currentMenuItems = itemList;
                    }
                }
                if (item.Description is "-")
                {
                    currentMenuItems.Add(new Separator());
                } else
                {
                    currentMenuItems.Add(new MenuItem
                    {
                        Header = item.Description,
                        Command = new RelayCommand<GameMenuItemActionArgs>(item.Action),
                        CommandParameter = new GameMenuItemActionArgs { Games = games, SourceItem = item }
                    });
                }
            }
            return menuItems;
        }
    }

    public class CommandSettings : ObservableObject
    {
        public static readonly CommandSettings Instance = new CommandSettings();

        private CommandSettings() { }

        public ICommand SwitchToDetailsViewCommand { get; } = new RelayCommand<Game>(SwitchToDetailsView);

        private static void SwitchToDetailsView(Game game)
        {
            SwitchViewAndSelect(game, DesktopView.Details);
        }

        public ICommand SwitchToGridViewCommand { get; } = new RelayCommand<Game>(SwitchToGridView);

        private static void SwitchToGridView(Game game)
        {
            SwitchViewAndSelect(game, DesktopView.Grid);
        }

        public ICommand SwitchToListViewCommand { get; } = new RelayCommand<Game>(SwitchToListView);

        private static void SwitchToListView(Game game)
        {
            SwitchViewAndSelect(game, DesktopView.List);
        }

        private static void SwitchViewAndSelect(Game game, DesktopView view)
        {
            if (API.Instance.MainView.ActiveDesktopView != view)
            {
                if (game is Game)
                {
                    if (API.Instance.MainView.SelectedGames?.FirstOrDefault() != game)
                    {
                        API.Instance.MainView.SelectGame(game.Id);
                    }
                }
                API.Instance.MainView.ActiveDesktopView = view;
            }
        }

        private static void OpenUrl(object param)
        {
            try
            {
                string url = null;
                switch (param)
                {
                    case string urlString:
                        url = urlString;
                        break;
                    case Uri uri:
                        url = uri.ToString();
                        break;
                    case Link link:
                        url = link.Url;
                        break;
                    default:
                        break;
                }
                if (!string.IsNullOrEmpty(url))
                {
                    System.Diagnostics.Process.Start(url);
                }
            }
            catch (Exception ex)
            {
                Extras.logger.Error(ex, $"Failed to open url {param?.ToString()}.");
            }
        }

        public ICommand OpenUrlCommand { get; } 
            = new RelayCommand<object>(OpenUrl);

        public static void DiscardNotification(NotificationMessage notificationMessage)
        {
            if (notificationMessage?.Id is string)
            {
                API.Instance.Notifications.Remove(notificationMessage.Id);
                if (API.Instance.Notifications.Count == 0)
                {
                    var mainWindow = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.Name == "WindowMain");
                    if (mainWindow is Window)
                    {
                        var notifcationPanel = PlayniteCommon.UI.UiHelper.FindVisualChildren<FrameworkElement>(mainWindow, "PART_Notifications").FirstOrDefault();
                        if (notifcationPanel is FrameworkElement)
                        {
                            if (PlayniteCommon.UI.UiHelper.FindVisualChildren<Button>(notifcationPanel, "PART_ButtonClose").FirstOrDefault() is Button closeButton)
                            {
                                if (closeButton.Command?.CanExecute(null) ?? false)
                                {
                                    closeButton.Command.Execute(null);
                                }
                            }
                        }
                    }
                }
            }
        }

        public ICommand DiscardNotificationCommand { get; } 
            = new RelayCommand<NotificationMessage>(DiscardNotification);

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
            = new RelayCommand<Game>(
            game =>
            {
                string path = null;
                var api = API.Instance;
                if (game is Game)
                {
                    path = Path.Combine(api.Database.DatabasePath, "files", game.Id.ToString());
                } else
                {
                    if (api?.MainView?.SelectedGames?.FirstOrDefault() is Game selected)
                    {
                        path = Path.Combine(api.Database.DatabasePath, "files", selected.Id.ToString());
                    }
                }
                if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
                {
                    System.Diagnostics.Process.Start(path);
                }
            },
            game =>
            {
                string path = null;
                var api = API.Instance;
                if (game is Game)
                {
                    path = Path.Combine(api.Database.DatabasePath, "files", game.Id.ToString());
                }
                else
                {
                    if (api?.MainView?.SelectedGames?.FirstOrDefault() is Game selected)
                    {
                        path = Path.Combine(api.Database.DatabasePath, "files", selected.Id.ToString());
                    }
                }
                return !string.IsNullOrEmpty(path) && Directory.Exists(path);
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

        public ICommand BackCommand { get; } = new RaisableCommand(Extras.Instance.NavigateBack, () => Extras.Instance.Navigation.CanGoBack);

        public ICommand ForwardCommand { get; } = new RaisableCommand(Extras.Instance.NavigateForward, () => Extras.Instance.Navigation.CanGoForward);
    }
}