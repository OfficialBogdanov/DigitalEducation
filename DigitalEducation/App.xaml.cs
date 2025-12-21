using System;
using System.IO;
using System.Windows;

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

            CheckVisionTemplatesFolder();

            base.OnStartup(e);
        }

        private void CheckVisionTemplatesFolder()
        {
            string templatesPath = GetTemplatesPath();

            if (!Directory.Exists(templatesPath))
            {
                try
                {
                    Directory.CreateDirectory(templatesPath);
                    Directory.CreateDirectory(Path.Combine(templatesPath, "Desktop"));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Не удалось создать папку шаблонов: {ex.Message}");
                }
            }
        }

        public static string GetTemplatesPath()
        {
            return Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "ComputerVision",
                "Templates"
            );
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
            }
        }
    }
}