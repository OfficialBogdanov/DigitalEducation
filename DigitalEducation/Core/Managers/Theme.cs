using DigitalEducation.Core.Managers;
using System;
using System.IO;
using System.Windows;

namespace DigitalEducation.Core.Managers
{
    public static class Theme
    {
        public const string DarkTheme = "Dark";
        public const string LightTheme = "Light";

        private static AppSettings _settingsService;
        private static string _cachedAssetsPath;

        public static string CurrentTheme { get; private set; } = DarkTheme;

        public static event EventHandler ThemeChanged;
        public static event EventHandler BackgroundChanged;

        public static void Initialize(AppSettings settingsService)
        {
            _settingsService = settingsService;
            LoadFromStorage();
            ApplyTheme(CurrentTheme);
        }

        private static void LoadFromStorage()
        {
            try
            {
                string savedTheme = _settingsService?.Current?.Theme;
                CurrentTheme = string.IsNullOrEmpty(savedTheme) ? DarkTheme : savedTheme;
            }
            catch
            {
                CurrentTheme = DarkTheme;
            }
        }

        public static void SetTheme(string theme)
        {
            if (string.IsNullOrEmpty(theme) || theme == CurrentTheme)
                return;

            CurrentTheme = theme;

            if (_settingsService != null)
            {
                _settingsService.Current.Theme = theme;
                _settingsService.Save();
            }

            ApplyTheme(theme);
            NotifyBackgroundChanged();
            ThemeChanged?.Invoke(null, EventArgs.Empty);
        }

        public static void ApplyTheme(string theme)
        {
            if (Application.Current == null || Application.Current.Resources == null)
                return;

            string themeUri = theme == LightTheme
                ? "/UI/Themes/LightTheme.xaml"
                : "/UI/Themes/DarkTheme.xaml";

            try
            {
                ResourceDictionary newThemeDict = new ResourceDictionary
                {
                    Source = new Uri(themeUri, UriKind.RelativeOrAbsolute)
                };

                ResourceDictionary appResources = Application.Current.Resources;
                ResourceDictionary oldThemeDict = FindThemeDictionary(appResources);

                if (oldThemeDict != null)
                {
                    appResources.MergedDictionaries.Remove(oldThemeDict);
                }

                appResources.MergedDictionaries.Add(newThemeDict);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Theme apply error: {ex.Message}");
            }
        }

        public static void NotifyBackgroundChanged()
        {
            BackgroundChanged?.Invoke(null, EventArgs.Empty);
        }

        public static string GetAssetsPath()
        {
            if (!string.IsNullOrEmpty(_cachedAssetsPath))
                return _cachedAssetsPath;

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            string[] possiblePaths = new string[]
            {
                Path.Combine(baseDir, "Assets"),
                Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "Assets")),
                Path.GetFullPath(Path.Combine(baseDir, "..", "..", "Assets")),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets")
            };

            foreach (string path in possiblePaths)
            {
                if (Directory.Exists(path))
                {
                    _cachedAssetsPath = path;
                    System.Diagnostics.Debug.WriteLine($"[Theme] Найден путь к Assets: {path}");
                    return path;
                }
            }

            string fallbackPath = Path.Combine(baseDir, "Assets");
            Directory.CreateDirectory(fallbackPath);
            _cachedAssetsPath = fallbackPath;
            System.Diagnostics.Debug.WriteLine($"[Theme] Создан путь к Assets: {fallbackPath}");
            return fallbackPath;
        }

        public static string GetBackgroundPath()
        {
            string theme = IsLightTheme ? "light" : "dark";
            string palette = Palette.CurrentPalette;

            string fileName;
            if (palette == "default")
            {
                fileName = $"Base_{theme}";
            }
            else
            {
                string paletteName = char.ToUpper(palette[0]) + palette.Substring(1);
                fileName = $"{paletteName}_{theme}";
            }

            string assetsPath = GetAssetsPath();
            string pngPath = Path.Combine(assetsPath, $"{fileName}.png");

            if (!File.Exists(pngPath))
            {
                string fallbackFileName = $"Base_{theme}";
                string fallbackPath = Path.Combine(assetsPath, $"{fallbackFileName}.png");
                if (File.Exists(fallbackPath))
                {
                    System.Diagnostics.Debug.WriteLine($"[Theme] Используем fallback: {fallbackPath}");
                    return fallbackPath;
                }
            }

            return pngPath;
        }

        public static void ToggleTheme()
        {
            string newTheme = CurrentTheme == DarkTheme ? LightTheme : DarkTheme;
            SetTheme(newTheme);
        }

        private static ResourceDictionary FindThemeDictionary(ResourceDictionary resources)
        {
            foreach (ResourceDictionary dict in resources.MergedDictionaries)
            {
                if (dict.Source != null)
                {
                    string source = dict.Source.ToString().ToLowerInvariant();
                    if (source.Contains("darktheme.xaml") || source.Contains("lighttheme.xaml"))
                    {
                        return dict;
                    }
                }
            }
            return null;
        }

        public static bool IsDarkTheme => CurrentTheme == DarkTheme;
        public static bool IsLightTheme => CurrentTheme == LightTheme;

        public static void ClearCache()
        {
            _cachedAssetsPath = null;
        }
    }
}