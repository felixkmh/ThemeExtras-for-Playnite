using Extras.ViewModels;
using Playnite.SDK.Controls;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
    /// Interaktionslogik für CompletionStatusContentControl.xaml
    /// </summary>
    public partial class StylableContentControl : PluginUserControl
    {
        public ViewModels.IStylableViewModel ViewModel { get; private set; }

        private StylableContentControl()
        {
            InitializeComponent();
        }

        public StylableContentControl(IStylableViewModel viewModel) : this()
        {
            ViewModel = viewModel;
            ContentControl.DataContext = viewModel;
        }

        public override void GameContextChanged(Game oldContext, Game newContext)
        {
            ViewModel.Game = newContext;
        }

        ~StylableContentControl()
        {
            try
            {
                ViewModel.Dispose();
            } catch (Exception) {}
        }
    }
}
