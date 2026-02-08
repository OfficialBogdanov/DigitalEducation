using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DigitalEducation
{
    public static class ThemeManager
    {
        private static string _currentTheme = "Light";
        private static readonly Dictionary<string, string> _iconMapping = new Dictionary<string, string>();
        private static readonly string _themeConfigFilePath;

        public static event EventHandler<string> ThemeChanged;


        static ThemeManager()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appFolder = Path.Combine(appDataPath, "DigitalEducation");

            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }

            _themeConfigFilePath = Path.Combine(appFolder, "theme_config.json");
        }

        public static void Initialize()
        {
            LoadThemeFromConfig();
            InitializeIconMapping();
        }

        public static string GetCurrentTheme()
        {
            return _currentTheme;
        }

        public static bool IsDarkTheme()
        {
            return _currentTheme == "Dark";
        }

        public static string GetThemeConfigFilePath()
        {
            return _themeConfigFilePath;
        }

        public static void ApplyTheme(string themeName)
        {
            if (string.IsNullOrEmpty(themeName) || themeName == _currentTheme)
                return;

            var app = Application.Current;
            if (app == null) return;

            try
            {
                var mainWindow = app.MainWindow as MainWindow;

                app.Resources.MergedDictionaries.Clear();

                ResourceDictionary newTheme = new ResourceDictionary();

                if (themeName == "Dark")
                {
                    newTheme.Source = new Uri("Themes/DarkTheme.xaml", UriKind.Relative);
                }
                else
                {
                    newTheme.Source = new Uri("Themes/LightTheme.xaml", UriKind.Relative);
                }

                app.Resources.MergedDictionaries.Add(newTheme);

                _currentTheme = themeName;
                SaveThemeToConfig(themeName);

                ThemeChanged?.Invoke(null, themeName);

                if (mainWindow != null)
                {
                    mainWindow.Resources = null;
                    mainWindow.Resources = app.Resources;

                    if (mainWindow.MainLayout != null)
                    {
                        mainWindow.MainLayout.Resources = null;
                        mainWindow.MainLayout.Resources = app.Resources;

                        UpdateAllIconsInContainer(mainWindow.MainLayout);
                        ForceUpdateAllStyles();
                    }

                    if (mainWindow.MainLayout is MasterLayout layout && layout.Content != null)
                    {
                        var currentContent = layout.Content;
                        layout.Content = null;
                        layout.Content = currentContent;
                    }
                }

                foreach (Window window in app.Windows)
                {
                    if (window != null && window != mainWindow)
                    {
                        window.Resources = null;
                        window.Resources = app.Resources;
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        private static void LoadThemeFromConfig()
        {
            try
            {
                if (File.Exists(_themeConfigFilePath))
                {
                    string json = File.ReadAllText(_themeConfigFilePath);
                    var config = JsonSerializer.Deserialize<ThemeConfig>(json);
                    if (config != null && !string.IsNullOrEmpty(config.ThemeName))
                    {
                        _currentTheme = config.ThemeName;
                        ApplyTheme(_currentTheme);
                    }
                }
                else
                {
                    SaveThemeToConfig("Light");
                }
            }
            catch (Exception ex)
            {
                _currentTheme = "Light";
            }
        }

        private static void SaveThemeToConfig(string themeName)
        {
            try
            {
                var config = new ThemeConfig { ThemeName = themeName };
                string json = JsonSerializer.Serialize(config);
                File.WriteAllText(_themeConfigFilePath, json);
            }
            catch (Exception ex)
            {
            }
        }

        private static void InitializeIconMapping()
        {
            _iconMapping["Book"] = "Book";
            _iconMapping["Chart"] = "Chart";
            _iconMapping["ChevronLeft"] = "ChevronLeft";
            _iconMapping["ChevronRight"] = "ChevronRight";
            _iconMapping["Close"] = "Close";
            _iconMapping["Document"] = "Document";
            _iconMapping["Folder"] = "Folder";
            _iconMapping["Globe"] = "Globe";
            _iconMapping["Home"] = "Home";
            _iconMapping["Info"] = "Info";
            _iconMapping["List"] = "List";
            _iconMapping["Monitor"] = "Monitor";
            _iconMapping["Moon"] = "Moon";
            _iconMapping["Refresh"] = "Refresh";
            _iconMapping["Right"] = "Right";
            _iconMapping["Settings"] = "Settings";
            _iconMapping["Success"] = "Success";
            _iconMapping["Sun"] = "Sun";
            _iconMapping["Trash"] = "Trash";
            _iconMapping["TrendingUp"] = "TrendingUp";
            _iconMapping["User"] = "User";
            _iconMapping["ZoomIn"] = "ZoomIn";
            _iconMapping["ZoomOut"] = "ZoomOut";
            _iconMapping["Search"] = "Search";
            _iconMapping["Plus"] = "Plus";
            _iconMapping["Edit"] = "Edit";
            _iconMapping["Calendar"] = "Calendar";
        }

        public static BitmapImage GetIcon(string iconName)
        {
            if (!_iconMapping.ContainsKey(iconName))
                return null;

            string iconPath = _currentTheme == "Dark"
                ? $"/Icons/White/{_iconMapping[iconName]}.png"
                : $"/Icons/Black/{_iconMapping[iconName]}.png";

            try
            {
                return new BitmapImage(new Uri(iconPath, UriKind.Relative));
            }
            catch
            {
                return null;
            }
        }

        public static void UpdateImageSource(Image image, string iconName)
        {
            if (image == null) return;

            var icon = GetIcon(iconName);
            if (icon != null)
            {
                image.Source = icon;
            }
        }

        public static void UpdateAllIconsInContainer(DependencyObject container)
        {
            if (container == null) return;

            var images = FindVisualChildren<Image>(container);
            foreach (var image in images)
            {
                var iconName = GetIconNameFromImage(image);
                if (!string.IsNullOrEmpty(iconName))
                {
                    UpdateImageSource(image, iconName);
                }
            }
        }

        public static void ForceUpdateAllStyles()
        {
            var app = Application.Current;
            if (app == null) return;

            foreach (Window window in app.Windows)
            {
                if (window != null)
                {
                    UpdateAllStylesInContainer(window);
                }
            }
        }

        private static void UpdateAllStylesInContainer(DependencyObject container)
        {
            if (container == null) return;

            var buttons = FindVisualChildren<Button>(container);
            foreach (var button in buttons)
            {
                if (button.Style != null)
                {
                    var currentStyle = button.Style;
                    button.Style = null;
                    button.Style = currentStyle;
                }
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(container); i++)
            {
                var child = VisualTreeHelper.GetChild(container, i);
                UpdateAllStylesInContainer(child);
            }
        }

        private static string GetIconNameFromImage(Image image)
        {
            if (image.Tag is string tag && !string.IsNullOrEmpty(tag))
                return tag;

            if (!string.IsNullOrEmpty(image.Name))
            {
                var name = image.Name;
                if (name.EndsWith("Icon") && name.Length > 4)
                    return name.Substring(0, name.Length - 4);
            }

            return null;
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        private class ThemeConfig
        {
            public string ThemeName { get; set; } = "Light";
        }
    }
}