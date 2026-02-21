using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DigitalEducation
{
    public static class ThemeManager
    {
        private static string _currentTheme;
        private static IThemeRepository _repository;
        private static IIconProvider _iconProvider;

        public static event EventHandler<string> ThemeChanged;

        public static IThemeRepository Repository
        {
            get
            {
                if (_repository == null)
                    _repository = new FileSystemThemeRepository();
                return _repository;
            }
            set => _repository = value ?? throw new ArgumentNullException(nameof(value));
        }

        public static IIconProvider IconProvider
        {
            get
            {
                if (_iconProvider == null)
                    _iconProvider = new ResourceIconProvider();
                return _iconProvider;
            }
            set => _iconProvider = value ?? throw new ArgumentNullException(nameof(value));
        }

        static ThemeManager()
        {
            try
            {
                LoadThemeFromRepository();
            }
            catch (Exception ex)
            {
                LogError($"Критическая ошибка инициализации ThemeManager: {ex}");
                _currentTheme = "Light";
            }
        }

        private static void LogError(string message)
        {
            Debug.WriteLine($"[ThemeManager] {message}");
        }

        public static void Initialize() { }

        public static string GetCurrentTheme() => _currentTheme;

        public static bool IsDarkTheme() => _currentTheme == "Dark";

        public static string GetThemeConfigFilePath() => Repository.GetType().Name;

        public static void ApplyTheme(string themeName)
        {
            if (string.IsNullOrEmpty(themeName) || themeName == _currentTheme)
                return;

            var app = Application.Current;
            if (app == null)
            {
                LogError("ApplyTheme: Application.Current is null");
                return;
            }

            try
            {
                app.Resources.MergedDictionaries.Clear();
                var newTheme = new ResourceDictionary();
                newTheme.Source = new Uri(themeName == "Dark" ? "Assets/Themes/DarkTheme.xaml" : "Assets/Themes/LightTheme.xaml", UriKind.Relative);
                app.Resources.MergedDictionaries.Add(newTheme);

                _currentTheme = themeName;
                Repository.SaveTheme(themeName);

                var mainWindow = app.MainWindow as MainWindow;
                if (mainWindow?.MainLayout != null)
                {
                    UpdateAllIconsInContainer(mainWindow.MainLayout);
                    ForceUpdateAllStyles();
                }

                ThemeChanged?.Invoke(null, themeName);
            }
            catch (Exception ex)
            {
                LogError($"Ошибка при применении темы {themeName}: {ex}");
            }
        }

        public static BitmapImage GetIcon(string iconName)
        {
            return IconProvider.GetIcon(iconName, IsDarkTheme());
        }

        public static void UpdateImageSource(Image image, string iconName)
        {
            IconProvider.UpdateImageSource(image, iconName, IsDarkTheme());
        }

        public static void UpdateAllIconsInContainer(DependencyObject container)
        {
            if (container == null) return;

            try
            {
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
            catch (Exception ex)
            {
                LogError($"Ошибка при обновлении иконок: {ex}");
            }
        }

        public static void ForceUpdateAllStyles()
        {
            var app = Application.Current;
            if (app == null) return;

            try
            {
                foreach (Window window in app.Windows)
                {
                    if (window != null)
                        UpdateAllStylesInContainer(window);
                }
            }
            catch (Exception ex)
            {
                LogError($"Ошибка при обновлении стилей: {ex}");
            }
        }

        private static void LoadThemeFromRepository()
        {
            string theme = Repository.LoadTheme();
            ApplyTheme(theme); 
        }

        private static string GetIconNameFromImage(Image image)
        {
            if (image.Tag is string tag && !string.IsNullOrEmpty(tag))
                return tag;
            if (!string.IsNullOrEmpty(image.Name) && image.Name.EndsWith("Icon") && image.Name.Length > 4)
                return image.Name.Substring(0, image.Name.Length - 4);
            return null;
        }

        private static void UpdateAllStylesInContainer(DependencyObject container)
        {
            if (container == null) return;
            try
            {
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
            catch (Exception ex)
            {
                LogError($"Ошибка в UpdateAllStylesInContainer: {ex}");
            }
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null) yield break;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                if (child is T t)
                    yield return t;
                foreach (T childOfChild in FindVisualChildren<T>(child))
                    yield return childOfChild;
            }
        }
    }
}