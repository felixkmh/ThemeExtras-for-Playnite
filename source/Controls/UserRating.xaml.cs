using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// Interaktionslogik für UserRating.xaml
    /// </summary>
    public partial class UserRating : Playnite.SDK.Controls.PluginUserControl
    {
        private bool isDragging = false;

        public UserRating()
        {
            InitializeComponent();
        }

        public override void GameContextChanged(Game oldContext, Game newContext)
        {
            base.GameContextChanged(oldContext, newContext);
            RatingsBar.DataContext = newContext;
        }

        private void RatingsBar_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;
            if (GameContext is Game game && sender is ProgressBar progressBar)
            {
                progressBar.CaptureMouse();
                var pos = e.GetPosition(progressBar);
                var score = pos.X / progressBar.ActualWidth * 100;
                game.UserScore = Math.Min(100, Math.Max(0, (int)score));
                if (game.UserScore == 0)
                {
                    game.UserScore = null;
                    
                }
            }
        }

        private void RatingsBar_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            if (GameContext is Game game && sender is ProgressBar progressBar)
            {
                progressBar.ReleaseMouseCapture();
                Playnite.SDK.API.Instance.Database.Games.Update(game);
            }
        }

        private void RatingsBar_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                if (GameContext is Game game && sender is ProgressBar progressBar)
                {
                    var pos = e.GetPosition(progressBar);
                    var score = pos.X / progressBar.ActualWidth * 100;
                    game.UserScore = Math.Min(100, Math.Max(0, (int)score));
                    if (game.UserScore == 0)
                    {
                        game.UserScore = null;
                        isDragging = false;
                    }
                }
            }
        }
    }
}
