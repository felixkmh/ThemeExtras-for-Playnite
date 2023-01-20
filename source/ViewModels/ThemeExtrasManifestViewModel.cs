using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Extras.ViewModels
{
    public class ThemeExtrasManifestViewModel : ObservableObject
    {
        private ObservableCollection<ExtendedTheme> theme;
        public ObservableCollection<ExtendedTheme> Themes { get => theme; set => SetValue(ref theme, value); }

        public ICommand ClearCommand { get; } = new RelayCommand<ExtendedTheme>(theme => theme?.ClearBackup());
        public ICommand OpenDirectoryCommand { get; } = new RelayCommand<ExtendedTheme>(theme => System.Diagnostics.Process.Start(theme.BackupPath), theme => !string.IsNullOrEmpty(theme?.BackupPath) && Directory.Exists(theme.BackupPath));
        public ICommand OpenThemeDirectoryCommand { get; } = new RelayCommand<ExtendedTheme>(theme => System.Diagnostics.Process.Start(theme.RootPath), theme => !string.IsNullOrEmpty(theme?.RootPath) && Directory.Exists(theme.RootPath));

        public ThemeExtrasManifestViewModel(IEnumerable<ExtendedTheme> extendedThemes)
        {
            Themes = extendedThemes.ToObservable();
        }
    }
}
