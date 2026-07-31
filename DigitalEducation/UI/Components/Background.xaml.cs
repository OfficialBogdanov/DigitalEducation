using DigitalEducation.Core.Managers;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace DigitalEducation.UI.Components
{
    public partial class Background : UserControl
    {
        public Background()
        {
            InitializeComponent();

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;

            Theme.BackgroundChanged += OnBackgroundChanged;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateBackground();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            Theme.BackgroundChanged -= OnBackgroundChanged;
        }

        private void OnBackgroundChanged(object sender, EventArgs e)
        {
            UpdateBackground();
        }

        private void UpdateBackground()
        {
            try
            {
                string path = Theme.GetBackgroundPath();

                if (File.Exists(path))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(path);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                    bitmap.EndInit();
                    bitmap.Freeze();

                    BackgroundImage.Source = bitmap;
                    System.Diagnostics.Debug.WriteLine($"Background: {path}");
                }
                else
                {
                    string fallbackPath = Path.Combine(Theme.GetAssetsPath(),
                        Theme.IsLightTheme ? "Base_light.png" : "Base_dark.png");

                    if (File.Exists(fallbackPath))
                    {
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(fallbackPath);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                        bitmap.EndInit();
                        bitmap.Freeze();
                        BackgroundImage.Source = bitmap;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Background error: {ex.Message}");
            }
        }

        public void Refresh()
        {
            UpdateBackground();
        }
    }
}