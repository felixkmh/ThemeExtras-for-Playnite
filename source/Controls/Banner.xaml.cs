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

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            var parent = Parent;
            if (parent is DependencyObject)
            {
                SetBinding(TagProperty, new Binding("Tag") { Source = parent });
            }
            else
            {
                BindingOperations.ClearBinding(this, TagProperty);
            }
            base.OnVisualParentChanged(oldParent);
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == TagProperty)
            {
                var oldValue = e.OldValue as Game ?? GameContext;
                var newValue = e.NewValue as Game ?? GameContext;

                OnGameContextChanged(oldValue, newValue);
            }
            if (e.Property == GameContextProperty && !(Tag is Game))
            {
                var oldValue = e.OldValue as Game;
                var newValue = e.NewValue as Game;

                OnGameContextChanged(oldValue, newValue);
            }

            base.OnPropertyChanged(e);
        }

        public void OnGameContextChanged(Game oldContext, Game newContext)
        {
            if (oldContext is Game)
            {
                oldContext.PropertyChanged -= Game_PropertyChanged;
            }
            if (newContext is Game)
            {
                if (newContext?.PluginId != oldContext?.PluginId || newContext.SourceId != oldContext.SourceId || !newContext.PlatformIds.IsListEqual(oldContext.PlatformIds))
                {
                    var bitmapImage = bannerCache.GetBanner(newContext);
                    if (bitmapImage != BannerImage.Source)
                    {
                        BannerImage.Source = bitmapImage;
                    }
                }
                newContext.PropertyChanged += Game_PropertyChanged;
            }
            else
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
                    if (Dispatcher.CheckAccess())
                    {
                        BannerImage.Source = bannerCache.GetBanner(GameContext);
                    } else 
                    {
                        Dispatcher.BeginInvoke(new Action(() => {
                            BannerImage.Source = bannerCache.GetBanner(GameContext);
                        }));
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
