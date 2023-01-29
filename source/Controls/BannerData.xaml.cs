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
    public partial class BannerData : PluginUserControl
    {
        public static IMultiValueConverter ProductConverter = new Converters.ProductConverter();

        public ImageSource BannerSource
        {
            get => (ImageSource)GetValue(BannerSourceProperty);
            set => SetValue(BannerSourceProperty, value);
        }

        // Using a DependencyProperty as the backing store for BannerSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BannerSourceProperty =
            DependencyProperty.Register(nameof(BannerSource), typeof(ImageSource), typeof(BannerData), new PropertyMetadata(default(ImageSource)));

        public double BannerHeight
        {
            get { return (double)GetValue(BannerHeightProperty); }
            set { SetValue(BannerHeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BannerHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BannerHeightProperty =
            DependencyProperty.Register(nameof(BannerHeight), typeof(double), typeof(BannerData), new PropertyMetadata(0.0));



        public double Ratio
        {
            get { return (double)GetValue(RatioProperty); }
            set { SetValue(RatioProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Ratio.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RatioProperty =
            DependencyProperty.Register(nameof(Ratio), typeof(double), typeof(BannerData), new PropertyMetadata(0.0));

        private BannerCache bannerCache;
        public BannerData(BannerCache bannerCache)
        {
            InitializeComponent();
            this.bannerCache = bannerCache;
            MultiBinding binding = new MultiBinding() { Converter = ProductConverter };
            binding.Bindings.Add(new Binding(nameof(ActualWidth)) { Source = this });
            binding.Bindings.Add(new Binding(nameof(Ratio)) { Source = this });
            SetBinding(BannerHeightProperty, binding);
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
                    if (bitmapImage != BannerSource)
                    {
                        BannerSource = bitmapImage;
                        Ratio = bitmapImage.Height / bitmapImage.Width;
                    }
                }
                newContext.PropertyChanged += Game_PropertyChanged;
            }
            else
            {
                BannerSource = null;
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
                        BannerSource = bannerCache.GetBanner(GameContext);
                    }
                    else
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            BannerSource = bannerCache.GetBanner(GameContext);
                        }));
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
