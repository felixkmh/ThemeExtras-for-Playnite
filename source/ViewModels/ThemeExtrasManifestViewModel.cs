using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public ICommand OpenDirectoryCommand { get; } = new RelayCommand<ExtendedTheme>(theme => System.Diagnostics.Process.Start(theme.BackupPath));
        public ICommand OpenThemeDirectoryCommand { get; } = new RelayCommand<ExtendedTheme>(theme => System.Diagnostics.Process.Start(theme.RootPath));

        public ThemeExtrasManifestViewModel(IEnumerable<ExtendedTheme> extendedThemes)
        {
            Themes = extendedThemes.ToObservable();
        }
    }
}
