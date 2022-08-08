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
    /// Interaktionslogik für Banner.xaml
    /// </summary>
    public partial class Banner : PluginUserControl
    {
        private BannerCache bannerCache;
        public Banner(BannerCache bannerCache)
        {
            InitializeComponent();
            this.bannerCache = bannerCache;
        }

        public override void GameContextChanged(Game oldContext, Game newContext)
        {
            if (oldContext is Game)
            {
                oldContext.PropertyChanged -= Game_PropertyChanged;
            }
            if (newContext is Game && 
                (newContext?.PluginId != oldContext?.PluginId || newContext.PlatformIds?.FirstOrDefault() != oldContext?.PlatformIds?.FirstOrDefault()))
            {
                var bitmapImage = bannerCache.GetBanner(newContext);
                if (bitmapImage != BannerImage.Source)
                {
                    BannerImage.Source = bitmapImage;
                }
                newContext.PropertyChanged += Game_PropertyChanged;
            } else if (newContext is null)
            {
                BannerImage.Source = null;
            }
        }

        private void Game_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Game.PlatformIds):
                case nameof(Game.PluginId):
                    BannerImage.Source = bannerCache.GetBanner(GameContext);
                    break;
                default:
                    break;
            }
        }
    }
}
