using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static Extras.BannerCache;

namespace Extras
{
    public class ExtendedTheme : ObservableObject, IBannerProvider
    {
        private const string DeletedFileExtension = ".deleted";

        public static ExtendedTheme Current { get; private set; }

        protected DirectoryBannerProvider DirectoryBannerProvider { get; set; }

        public string Name => ThemeManifest.Name;
        public string Id => ThemeManifest.Id;
        public string RootPath { get; private set; }
        public string LastChangeFilePath { get; private set; }
        public Models.ThemeManifest ThemeManifest { get; private set; }
        public Models.ThemeExtrasManifest ThemeExtrasManifest { get; private set; }
        public string BackupPath { get; private set; }
        public int BackedUpFilesCount => Directory.Exists(BackupPath) ? Directory.GetFiles(BackupPath, "*", SearchOption.AllDirectories).Count() : 0;
        public int PersistentFilesCount => GetPersistentFileCount();
        public bool IsCurrentTheme { get; private set; } = false;

        public int? DecodeHeight => ThemeExtrasManifest.BannerDecodeHeight;
        public bool IsDevTheme => CheckDevTheme();

        public Dictionary<Platform, string> PlatformBanners => DirectoryBannerProvider.PlatformBanners;
        public Dictionary<Guid, string> PluginBanners => DirectoryBannerProvider.PluginBanners;
        public Dictionary<GameSource, string> SourceBanners => DirectoryBannerProvider.SourceBanners;
        public string DefaultBanner => DirectoryBannerProvider.DefaultBanner;


        private bool CheckDevTheme()
        {
            var files = Directory.GetFiles(RootPath);
            if (files.Any(f => f.EndsWith(".sln"))) return true;
            if (files.Any(f => f.EndsWith(".csproj"))) return true;
            if (files.Any(f => f.EndsWith("App.xaml"))) return true;
            var directories = Directory.GetDirectories(RootPath);
            if (directories.Any(d => d.EndsWith("bin"))) return true;
            if (directories.Any(d => d.EndsWith(".vs"))) return true;
            return false;
        }

        public static IEnumerable<ExtendedTheme> CreateExtendedManifests()
        {
            var api = Playnite.SDK.API.Instance;
            var destopThemeDirectory = Path.Combine(api.Paths.ConfigurationPath, "Themes", "Desktop");
            if (Directory.Exists(destopThemeDirectory))
            {
                var themeDirectories = Directory.GetDirectories(destopThemeDirectory);
                foreach (var themeDirectory in themeDirectories)
                {
                    if (TryCreate(themeDirectory, out var extendedTheme))
                    {
                        yield return extendedTheme;
                    }
                }
            }
        }

        public static bool TryCreate(string themeRootPath, out ExtendedTheme extendedTheme)
        {
            extendedTheme = new ExtendedTheme();
            extendedTheme.RootPath = themeRootPath;
            extendedTheme.LastChangeFilePath = Path.Combine(themeRootPath, "lastChanged.json");

            var extraManifestPath = Path.Combine(themeRootPath, Extras.ExtrasManifestFileName);
            if (!File.Exists(extraManifestPath))
            {
                return false;
            }

            if (!Playnite.SDK.Data.Serialization.TryFromYamlFile<Models.ThemeExtrasManifest>(extraManifestPath, out var extrasManifest))
            {
                return false;
            }

            extendedTheme.ThemeExtrasManifest = extrasManifest;

            extendedTheme.DirectoryBannerProvider = new DirectoryBannerProvider 
            { 
                DefaultBanner = extrasManifest.DefaultBannerPath is string ? Path.Combine(themeRootPath, extrasManifest.DefaultBannerPath) : null,
                BannersBySourceNamePath = extrasManifest.BannersBySourceNamePath is string ? Path.Combine(themeRootPath, extrasManifest.BannersBySourceNamePath) : null,
                BannersByPlatformNamePath = extrasManifest.BannersByPlatformNamePath is string ? Path.Combine(themeRootPath, extrasManifest.BannersByPlatformNamePath) : null,
                BannersByPluginIdPath = extrasManifest.BannersByPluginIdPath is string ? Path.Combine(themeRootPath, extrasManifest.BannersByPluginIdPath) : null,
                BannersBySpecIdPath = extrasManifest.BannersBySpecIdPath is string ? Path.Combine(themeRootPath, extrasManifest.BannersBySpecIdPath) : null,
            };

            var themManifestPath = Path.Combine(themeRootPath, Extras.ThemeManifestFileName);
            if (!File.Exists(themManifestPath))
            {
                return false;
            }
            if (!Playnite.SDK.Data.Serialization.TryFromYamlFile<Models.ThemeManifest>(themManifestPath, out var manifest))
            {
                return false;
            }

            extendedTheme.ThemeManifest = manifest;
            extendedTheme.BackupPath = Path.Combine(Extras.Instance.GetPluginUserDataPath(), "ThemeBackups", manifest.Id);

            var currentThemeId = Playnite.SDK.API.Instance.ApplicationSettings.DesktopTheme;
            extendedTheme.IsCurrentTheme = extendedTheme.Id == currentThemeId;
            if (extendedTheme.IsCurrentTheme)
            {
                Current = extendedTheme;
            }

            return true;
        }

        public int GetPersistentFileCount()
        {
            if (ThemeExtrasManifest.PersistentPaths is IEnumerable<string> persistentPaths)
            {
                HashSet<string> files = new HashSet<string>();
                foreach(var persistentPath in persistentPaths)
                {
                    var fullPath = Path.Combine(RootPath, persistentPath);
                    if (File.Exists(fullPath))
                    {
                        files.Add(fullPath);
                    }
                    else if (Directory.Exists(fullPath))
                    {
                        foreach (var path in Directory.GetFiles(fullPath, "*", SearchOption.AllDirectories))
                        {
                            files.Add(path);
                        }
                    }
                }
                return files.Count;
            }
            return 0;
        }

        public IEnumerable<string> GetRelativeFilePaths(string basePath, string relativeDirectoryPath)
        {
            var directorySourcePath = Path.Combine(basePath, relativeDirectoryPath);

            foreach (var file in Directory.GetFiles(directorySourcePath, "*", SearchOption.AllDirectories))
            {
                var relativePath = file.Replace(basePath + Path.DirectorySeparatorChar, "");
                yield return relativePath;
            }
        }

        public Dictionary<string, string> GetResources()
        {
            Dictionary<string, string> resources = new Dictionary<string, string>();

            //if (ThemeExtrasManifest?.PersistentResources != null)
            //{
            //    foreach(var resourceKey in ThemeExtrasManifest.PersistentResources)
            //    {
            //        if (Application.Current.Resources.Contains(resourceKey))
            //        {
            //            resources[resourceKey] = Serialization.ToJson(Application.Current.Resources[resourceKey]);
            //        }
            //    }
            //}

            return resources;
        }

        public void SetResources(Dictionary<string, string> resources)
        {
            //foreach(var resource in resources)
            //{
            //    if (Application.Current.Resources.Contains(resource.Key))
            //    {
            //        var type = Application.Current.Resources[resource.Key]?.GetType();
            //        try
            //        {
            //            if (Newtonsoft.Json.JsonConvert.DeserializeObject(resource.Value, type) is object value)
            //            {
            //                Application.Current.Resources[resource.Key] = value;
            //            }
            //        }
            //        catch (Exception ex)
            //        {
            //            Extras.logger.Error(ex, $"Failed to deserialize resource with key {resource.Value} to type {type.Name}.");
            //        }
            //    }
            //}
        }

        public void ClearBackup()
        {
            if (Directory.Exists(BackupPath))
            {
                Directory.Delete(BackupPath, true);
                OnPropertyChanged(nameof(BackedUpFilesCount));
            }
        }

        public void BackupFiles()
        {
            if (ThemeExtrasManifest.PersistentPaths is IEnumerable<string> relativePaths)
            {
                var files = new List<string>();
                foreach(var path in relativePaths)
                {
                    string sourcePath = Path.Combine(RootPath, path);
                    if (File.Exists(sourcePath))
                    {
                        files.Add(path);
                    } 
                    else if (Directory.Exists(sourcePath))
                    {
                        files.AddRange(GetRelativeFilePaths(RootPath, path));
                    }
                }
                if (!File.Exists(LastChangeFilePath))
                {
                    var timestamps = new Dictionary<string, DateTime>();
                    foreach (var file in files)
                    {
                        var fullPath = Path.Combine(RootPath, file);
                        if (File.Exists(fullPath))
                        {
                            var lastWrite = File.GetLastWriteTime(fullPath);
                            timestamps[file] = lastWrite;
                        }
                    }
                    var json = Playnite.SDK.Data.Serialization.ToJson(timestamps, true);
                    File.WriteAllText(LastChangeFilePath, json);
                } else
                {
                    var timestamps = Playnite.SDK.Data.Serialization.FromJsonFile<Dictionary<string, DateTime>>(LastChangeFilePath);
                    var anyBackups = false;
                    foreach (var file in files)
                    {
                        var shouldBackup = true;
                        var fullPath = Path.Combine(RootPath, file);
                        var newLastChanged = File.GetLastWriteTime(fullPath);
                        if (timestamps.TryGetValue(file, out var lastChanged))
                        {
                            shouldBackup = lastChanged != newLastChanged;
                        }

                        if (shouldBackup)
                        {
                            var deleteFilePath = Path.Combine(BackupPath, file + DeletedFileExtension);
                            if (File.Exists(deleteFilePath))
                            {
                                File.Delete(deleteFilePath);
                            }
                            if (BackupFile(file))
                            {
                                timestamps[file] = newLastChanged;
                                anyBackups = true;
                            }
                        }
                    }
                    foreach(var file in timestamps.Keys)
                    {
                        var sourcePath = Path.Combine(RootPath, file);
                        var targetPath = Path.Combine(BackupPath, file);
                        var deleteFilePath = Path.Combine(BackupPath, file + DeletedFileExtension);
                        if (!File.Exists(sourcePath) && !File.Exists(deleteFilePath))
                        {
                            if (File.Exists(targetPath))
                            {
                                File.Delete(targetPath);
                            }
                            File.Create(deleteFilePath);
                            timestamps[file] = File.GetLastWriteTime(deleteFilePath);
                            anyBackups = true;
                        }
                    }
                    if (anyBackups)
                    {
                        var json = Playnite.SDK.Data.Serialization.ToJson(timestamps, true);
                        File.WriteAllText(LastChangeFilePath, json);
                    }
                }
            }
        }

        public bool BackupFile(string relativeFilePath)
        {
            try
            {
                var fileBackupPath = Path.Combine(BackupPath, relativeFilePath);
                var directoryBackupPath = Path.GetDirectoryName(fileBackupPath);
                if (!Directory.Exists(directoryBackupPath))
                {
                    Directory.CreateDirectory(directoryBackupPath);
                }

                string sourceFilePath = Path.Combine(RootPath, relativeFilePath);
                File.Copy(sourceFilePath, fileBackupPath, true);
                Extras.logger.Debug($"Backed {relativeFilePath} from {RootPath} to {BackupPath}.");
                return true;
            }
            catch (Exception ex)
            {
                Extras.logger.Error(ex, $"Failed to backup {relativeFilePath} of theme {Name}.");
                return false;
            }
        }

        public void RestoreFile(string relativeFilePath)
        {
            try
            {
                var fileTargetPath = Path.Combine(RootPath, relativeFilePath);
                var directoryTargetPath = Path.GetDirectoryName(fileTargetPath);
                if (!Directory.Exists(directoryTargetPath))
                {
                    Directory.CreateDirectory(directoryTargetPath);
                }

                string sourceFilePath = Path.Combine(BackupPath, relativeFilePath);
                if (File.Exists(sourceFilePath))
                {
                    File.Copy(sourceFilePath, fileTargetPath, true);
                    Extras.logger.Debug($"Restored {relativeFilePath} from {BackupPath} to {RootPath}.");
                }
            }
            catch (Exception ex)
            {
                if (System.Diagnostics.Debugger.IsAttached && ex is UnauthorizedAccessException)
                {
                    Playnite.SDK.API.Instance.Notifications.Add(new Playnite.SDK.NotificationMessage(
                        "FileRestoreFailed",
                        $"Failed to restore {relativeFilePath} of theme {Name} because file access was denied. If this file is an image used as a theme resource, make sure to set \"CacheOption=\"OnLoad\"\".", 
                        Playnite.SDK.NotificationType.Error
                    ));
                }
                Extras.logger.Error(ex, $"Failed to restore {relativeFilePath} of theme {Name}.");
            }
        }

        public void RestoreFiles()
        {
            if (ThemeExtrasManifest.PersistentPaths is IEnumerable<string> relativePaths)
            {
                if (File.Exists(LastChangeFilePath))
                {
                    // Theme wasn't updated, no need to restore
                    return;
                }

                foreach (var path in relativePaths)
                {
                    string sourcePath = Path.Combine(BackupPath, path);
                    if (File.Exists(sourcePath))
                    {
                        RestoreFile(path);
                    }
                    else if (Directory.Exists(sourcePath))
                    {
                        foreach(var relativePath in GetRelativeFilePaths(BackupPath, path))
                        {
                            if (relativePath.EndsWith(DeletedFileExtension)) continue;
                            var deleteFilePath = Path.Combine(BackupPath, relativePath + DeletedFileExtension);
                            if (File.Exists(deleteFilePath))
                            {
                                var sourceFilePath = Path.Combine(RootPath, relativePath);
                                if (File.Exists(sourceFilePath))
                                {
                                    File.Delete(sourceFilePath);
                                }
                            } else
                            {
                                RestoreFile(relativePath);
                            }
                        }
                    }
                }
            }
        }
    }
}
