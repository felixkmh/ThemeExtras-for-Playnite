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
        private bool mouseDown = false;
        private bool isDragging = false;
        private Point mouseDownPos = new Point();

        public UserRating()
        {
            InitializeComponent();
            IsVisibleChanged += UserRating_IsVisibleChanged;
        }

        private void UserRating_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(e.NewValue is Visibility.Visible))
            {
                if (mouseDown)
                {
                    isDragging = false;
                    mouseDown = false;
                    Playnite.SDK.API.Instance.Database.Games.Update(GameContext);
                }
            }
        }

        public override void GameContextChanged(Game oldContext, Game newContext)
        {
            base.GameContextChanged(oldContext, newContext);
            RatingsBar.DataContext = newContext;
        }

        private void RatingsBar_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var wasClick = !isDragging;
            mouseDown = true;
            mouseDownPos = e.GetPosition(this);
            if (GameContext is Game game && sender is ProgressBar progressBar)
            {
                progressBar.CaptureMouse();
                SetUserScore(e, wasClick, game, progressBar);
            }
        }

        private static void SetUserScore(MouseEventArgs e, bool round, Game game, ProgressBar progressBar)
        {
            var pos = e.GetPosition(progressBar);
            var score = pos.X / progressBar.ActualWidth * 100;
            if (round)
            {
                score = Math.Ceiling(score / 10) * 10;
            }
            game.UserScore = Math.Min(100, Math.Max(0, (int)score));
            if (game.UserScore == 0)
            {
                game.UserScore = null;
            }
        }

        private void RatingsBar_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            mouseDown = false;
            if (GameContext is Game game && sender is ProgressBar progressBar)
            {
                if(progressBar.IsMouseCaptured)
                {
                    progressBar.ReleaseMouseCapture();
                    Playnite.SDK.API.Instance.Database.Games.Update(GameContext);
                }
            }
        }

        private void RatingsBar_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown)
            {
                if (!isDragging)
                {
                    var delta = Point.Subtract(mouseDownPos, e.GetPosition(this));
                    if (SystemParameters.MinimumHorizontalDragDistance <= Math.Abs(delta.X) ||
                        SystemParameters.MinimumVerticalDragDistance <= Math.Abs(delta.Y))
                    {
                        isDragging = true;
                    }
                }
                if (isDragging)
                {
                    if (GameContext is Game game && sender is ProgressBar progressBar)
                    {
                        SetUserScore(e, false, game, progressBar);
                    }
                }
            }
        }
    }
}
