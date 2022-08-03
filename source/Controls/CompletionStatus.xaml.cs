using Extras.ViewModels;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Extras.Controls
{
    /// <summary>
    /// For CompletionStatus.xaml
    /// </summary>
    public partial class CompletionStatus : Playnite.SDK.Controls.PluginUserControl
    {
        public CompletionStatus(CompletionStatusViewModel args)
        {

            InitializeComponent();
            CompletionComboBox.DataContext = args;

            IsVisibleChanged += CompletionStatus_IsVisibleChanged;
        }

        private void CompletionStatus_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

            if (!(e.NewValue is Visibility.Visible))
            {
                Playnite.SDK.API.Instance.Database.Games.Update(GameContext);
            }
        }

        public override void GameContextChanged(Game oldContext, Game newContext)
        {
            if (CompletionComboBox.DataContext is ViewModels.IStylableViewModel vm)
            {
                vm.Game = newContext;
            }

            base.GameContextChanged(oldContext, newContext);
        }

    }
}
