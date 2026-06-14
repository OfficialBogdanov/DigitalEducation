using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace DigitalEducation
{
    public partial class SplashWindow : Window
    {
        private Storyboard _rotationStoryboard;

        public SplashWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            AppThemeManager.ThemeChanged += OnThemeChanged;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateIcons();
            var rotation = new DoubleAnimation
            {
                From = 0,
                To = 360,
                Duration = TimeSpan.FromSeconds(1),
                RepeatBehavior = RepeatBehavior.Forever
            };
            _rotationStoryboard = new Storyboard();
            Storyboard.SetTarget(rotation, LoaderImage);
            Storyboard.SetTargetProperty(rotation, new PropertyPath("(UIElement.RenderTransform).(RotateTransform.Angle)"));
            _rotationStoryboard.Children.Add(rotation);
            _rotationStoryboard.Begin();
        }

        private void UpdateIcons()
        {
            AppThemeManager.UpdateImageSource(LogoImage, "Book");
            AppThemeManager.UpdateImageSource(LoaderImage, "Loader");
        }

        private void OnThemeChanged(object sender, string themeName)
        {
            Dispatcher.BeginInvoke(new Action(UpdateIcons));
        }

        protected override void OnClosed(EventArgs e)
        {
            AppThemeManager.ThemeChanged -= OnThemeChanged;
            _rotationStoryboard?.Stop();
            base.OnClosed(e);
        }
    }
}