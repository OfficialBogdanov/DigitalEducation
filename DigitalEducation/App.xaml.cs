using System.Windows;
using System;

namespace DigitalEducation
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            ThemeManager.Initialize();

            string savedTheme = ThemeManager.GetCurrentTheme();
            if (!string.IsNullOrEmpty(savedTheme))
            {
                ApplyThemeOnStartup(savedTheme);
            }

            base.OnStartup(e);
        }

        private void ApplyThemeOnStartup(string themeName)
        {
            try
            {
                Resources.MergedDictionaries.Clear();

                ResourceDictionary newTheme = new ResourceDictionary();

                if (themeName == "Dark")
                {
                    newTheme.Source = new Uri("Themes/DarkTheme.xaml", UriKind.Relative);
                }
                else
                {
                    newTheme.Source = new Uri("Themes/LightTheme.xaml", UriKind.Relative);
                }

                Resources.MergedDictionaries.Add(newTheme);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error applying theme on startup: {ex.Message}");
            }
        }
    }
}