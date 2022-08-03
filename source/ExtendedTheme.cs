using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extras
{
    public class ExtendedTheme
    {
        public string Name => ThemeManifest.Name;
        public string Id => ThemeManifest.Id;
        public string RootPath { get; private set; }
        public Models.ThemeManifest ThemeManifest { get; private set; }
        public Models.ThemeExtrasManifest ThemeExtrasManifest { get; private set; }
        public string BackupPath { get; private set; }
        public DateTime? LastBackup => ThemeExtrasManifest.LastBackup;

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

            return true;
        }

        public void Backup()
        {
            if (ThemeExtrasManifest.PersistentPaths is IEnumerable<string> relativePaths)
            {
                var backedUp = false;
                foreach(var path in relativePaths)
                {
                    string sourcePath = Path.Combine(RootPath, path);
                    if (File.Exists(sourcePath))
                    {
                        if (LastBackup is null || File.GetLastWriteTime(sourcePath) > LastBackup)
                        {
                            BackupFile(path);
                            backedUp = true;
                        }
                    } 
                    else if (Directory.Exists(sourcePath))
                    {
                        if (BackupDirectory(path) > 0)
                            backedUp = true;
                    }
                }
                if (backedUp)
                {
                    ThemeExtrasManifest.LastBackup = DateTime.Now;
                    var yaml = Playnite.SDK.Data.Serialization.ToYaml(ThemeExtrasManifest);
                    try
                    {
                        File.WriteAllText(Path.Combine(RootPath, Extras.ExtrasManifestFileName), yaml);
                    }
                    catch (Exception ex)
                    {
                        Extras.logger.Error(ex, $"Failed to update {Extras.ExtrasManifestFileName} for theme {Name}.");
                    }
                }
            }
        }

        public void BackupFile(string relativeFilePath)
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
            }
            catch (Exception ex)
            {
                Extras.logger.Error(ex, $"Failed to backup {relativeFilePath} of theme {Name}.");
            }
        }

        public int BackupDirectory(string realativeDirectoryPath)
        {
            var updated = 0;
            try
            {
                var directorySourcePath = Path.Combine(RootPath, realativeDirectoryPath);
                var directoyBackupPath = Path.Combine(BackupPath, realativeDirectoryPath);
                if (!Directory.Exists(directoyBackupPath))
                {
                    Directory.CreateDirectory(directoyBackupPath);
                }
                foreach(var file in Directory.GetFiles(directorySourcePath, "*", SearchOption.AllDirectories))
                {
                    var fileInfo = new FileInfo(file);
                    if (LastBackup is null || fileInfo.LastWriteTime > LastBackup)
                    {
                        var fileName = Path.GetFileName(file);
                        var newPath = file.Replace(RootPath, BackupPath);
                        var newDir = Path.GetDirectoryName(newPath);
                        if (!Directory.Exists(newDir))
                        {
                            Directory.CreateDirectory(newDir);
                        }
                        File.Copy(file, Path.Combine(newDir, fileName), true);
                        ++updated;
                    }
                }
            }
            catch (Exception ex)
            {
                Extras.logger.Error(ex, $"Failed to backup directoy {realativeDirectoryPath} of theme {Name}");
            }
            return updated;
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
                }
            }
            catch (Exception ex)
            {
                Extras.logger.Error(ex, $"Failed to restore {relativeFilePath} of theme {Name}.");
            }
        }

        public void RestoreDirectory(string realativeDirectoryPath)
        {
            try
            {
                var directorySourcePath = Path.Combine(BackupPath, realativeDirectoryPath);
                var directoryTarget = Path.Combine(RootPath, realativeDirectoryPath);
                if (!Directory.Exists(directoryTarget))
                {
                    Directory.CreateDirectory(directoryTarget);
                }
                foreach (var file in Directory.GetFiles(directorySourcePath, "*", SearchOption.AllDirectories))
                {
                    var fileName = Path.GetFileName(file);
                    var newPath = file.Replace(BackupPath, RootPath);
                    var newDir = Path.GetDirectoryName(newPath);
                    if (!Directory.Exists(newDir))
                    {
                        Directory.CreateDirectory(newDir);
                    }
                    File.Copy(file, Path.Combine(newDir, fileName), true);
                }
            }
            catch (Exception ex)
            {
                Extras.logger.Error(ex, $"Failed to restore directoy {realativeDirectoryPath} of theme {Name}");
            }
        }

        public void Restore()
        {
            if (ThemeExtrasManifest.LastBackup is null)
            {
                if (ThemeExtrasManifest.PersistentPaths is IEnumerable<string> relativePaths)
                {
                    foreach (var path in relativePaths)
                    {
                        string sourcePath = Path.Combine(BackupPath, path);
                        if (File.Exists(sourcePath))
                        {
                            RestoreFile(path);
                        }
                        else if (Directory.Exists(sourcePath))
                        {
                            RestoreDirectory(path);
                        }
                    }
                }
            }
        }
    }
}
