using Playnite.SDK.Controls;
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
    /// Interaktionslogik für MediaElementControls.xaml
    /// </summary>
    public partial class MediaElementControls : PluginUserControl
    {
        public MediaElementControls()
        {
            InitializeComponent();
            DataContext = this;
        }

        public double Volume { get => (double)GetValue(VolumeProperty); set => SetValue(VolumeProperty, value); }
        public double Progress { get => (double)GetValue(ProgressProperty); set => SetValue(ProgressProperty, value); }
        public double ProgressMax { get => (double)GetValue(ProgressMaxProperty); set => SetValue(ProgressMaxProperty, value); }
        public bool IsScrubbing { get => (bool)GetValue(IsScrubbingProperty); set => SetValue(IsScrubbingProperty, value); }
        public bool IsMediaLoaded { get => (bool)GetValue(IsMediaLoadedProperty); set => SetValue(IsMediaLoadedProperty, value); }
        public bool IsPlaying { get => (bool)GetValue(IsPlayingProperty); set => SetValue(IsPlayingProperty, value); }

        public MediaElement MediaElement { get; set; }

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);
            var cc = (ContentControl)Parent;
            if (cc.Tag is DependencyObject dependencyObject)
            {
                if (PlayniteCommon.UI.UiHelper.FindVisualChildren<MediaElement>(dependencyObject).FirstOrDefault() is MediaElement mediaElement)
                {
                    MediaElement = mediaElement;
                    SetBinding(VolumeProperty, new Binding(nameof(Volume))
                    {
                        Mode = BindingMode.TwoWay,
                        Converter = Converters.PowConverter.Instance,
                        ConverterParameter = 0.5,
                        Source = mediaElement
                    });
                    mediaElement.ScrubbingEnabled = true;
                    mediaElement.MediaOpened += MediaElement_MediaOpened;
                    mediaElement.MediaFailed += MediaElement_MediaFailed;
                    mediaElement.MediaEnded += MediaElement_MediaEnded;
                }
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (FindName("ScrubbingSlider") is Slider slider)
            {
                SetBinding(IsScrubbingProperty, new Binding("IsMouseCaptureWithin") { Source = slider, Mode = BindingMode.OneWay });
            }
        }

        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            IsPlaying = MediaElement.NaturalDuration.TimeSpan.TotalSeconds == 0;
        }

        private void MediaElement_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            IsMediaLoaded = false;
        }

        private void MediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            IsMediaLoaded = true;
            IsPlaying = MediaElement.LoadedBehavior == MediaState.Play;
            Progress = 0;
            ProgressMax = MediaElement.NaturalDuration.TimeSpan.TotalSeconds;
        }

        private void MediaElement_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            var mediaElement = (MediaElement)sender;
            if (mediaElement.Source != null)
            {
                Progress = 0;
                ProgressMax = mediaElement.NaturalDuration.TimeSpan.TotalSeconds;
            }
        }

        protected static void OnIsScrubbingChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var control = (MediaElementControls)sender;
            var newValue = (bool)args.NewValue;
            if (newValue is false && control.IsMediaLoaded)
            {
                control.MediaElement.Position = TimeSpan.FromSeconds(control.Progress);
            }
        }

        protected static void OnIsPlayingChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var control = (MediaElementControls)sender;
            var newValue = (bool)args.NewValue;
            if (control.IsMediaLoaded)
            {
                if (newValue is true)
                {
                    control.MediaElement.Play();
                } else
                {
                    control.MediaElement.Stop();
                }

            }
        }

        public static DependencyProperty VolumeProperty 
            = DependencyProperty.Register(nameof(Volume), typeof(double), typeof(MediaElementControls), new PropertyMetadata(0.0));
        public static DependencyProperty ProgressProperty
            = DependencyProperty.Register(nameof(Progress), typeof(double), typeof(MediaElementControls), new PropertyMetadata(0.0));
        public static DependencyProperty ProgressMaxProperty
            = DependencyProperty.Register(nameof(ProgressMax), typeof(double), typeof(MediaElementControls), new PropertyMetadata(1.0));
        public static DependencyProperty IsScrubbingProperty
            = DependencyProperty.Register(nameof(IsScrubbing), typeof(bool), typeof(MediaElementControls), new PropertyMetadata(false, OnIsScrubbingChanged));
        public static DependencyProperty IsMediaLoadedProperty
            = DependencyProperty.Register(nameof(IsMediaLoaded), typeof(bool), typeof(MediaElementControls), new PropertyMetadata(false));
        public static DependencyProperty IsPlayingProperty
            = DependencyProperty.Register(nameof(IsPlaying), typeof(bool), typeof(MediaElementControls), new PropertyMetadata(false, OnIsPlayingChanged));
    }
}
