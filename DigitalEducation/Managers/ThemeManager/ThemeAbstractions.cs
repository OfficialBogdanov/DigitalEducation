using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows.Media.Imaging;

namespace DigitalEducation
{
    public interface IThemeRepository
    {
        string LoadTheme();
        void SaveTheme(string themeName);
    }

    public interface IIconProvider
    {
        BitmapImage GetIcon(string iconName, bool isDarkTheme);
        void UpdateImageSource(System.Windows.Controls.Image image, string iconName, bool isDarkTheme);
    }

    public class FileSystemThemeRepository : IThemeRepository
    {
        private readonly string _configPath;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions();

        public FileSystemThemeRepository()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appFolder = Path.Combine(appDataPath, "DigitalEducation");
            if (!Directory.Exists(appFolder))
                Directory.CreateDirectory(appFolder);
            _configPath = Path.Combine(appFolder, "theme_config.json");
        }

        public string LoadTheme()
        {
            if (!File.Exists(_configPath))
                return "Light";
            try
            {
                string json = File.ReadAllText(_configPath);
                var config = JsonSerializer.Deserialize<ThemeConfig>(json);
                return config?.ThemeName ?? "Light";
            }
            catch
            {
                return "Light";
            }
        }

        public void SaveTheme(string themeName)
        {
            try
            {
                var config = new ThemeConfig { ThemeName = themeName };
                string json = JsonSerializer.Serialize(config);
                File.WriteAllText(_configPath, json);
            }
            catch { }
        }

        private class ThemeConfig
        {
            public string ThemeName { get; set; } = "Light";
        }
    }

    public class ResourceIconProvider : IIconProvider
    {
        private readonly Dictionary<string, string> _iconMapping = new Dictionary<string, string>
        {
            ["Book"] = "Book",
            ["Chart"] = "Chart",
            ["ChevronLeft"] = "ChevronLeft",
            ["ChevronRight"] = "ChevronRight",
            ["Close"] = "Close",
            ["Document"] = "Document",
            ["Folder"] = "Folder",
            ["Globe"] = "Globe",
            ["Home"] = "Home",
            ["Info"] = "Info",
            ["List"] = "List",
            ["Monitor"] = "Monitor",
            ["Moon"] = "Moon",
            ["Refresh"] = "Refresh",
            ["Right"] = "Right",
            ["Settings"] = "Settings",
            ["Success"] = "Success",
            ["Sun"] = "Sun",
            ["Trash"] = "Trash",
            ["TrendingUp"] = "TrendingUp",
            ["User"] = "User",
            ["ZoomIn"] = "ZoomIn",
            ["ZoomOut"] = "ZoomOut",
            ["Search"] = "Search",
            ["Plus"] = "Plus",
            ["Edit"] = "Edit",
            ["Calendar"] = "Calendar"
        };

        public BitmapImage GetIcon(string iconName, bool isDarkTheme)
        {
            if (!_iconMapping.ContainsKey(iconName))
                return null;

            string path = isDarkTheme
                ? $"/Icons/White/{_iconMapping[iconName]}.png"
                : $"/Icons/Black/{_iconMapping[iconName]}.png";

            try
            {
                return new BitmapImage(new Uri(path, UriKind.Relative));
            }
            catch
            {
                return null;
            }
        }

        public void UpdateImageSource(System.Windows.Controls.Image image, string iconName, bool isDarkTheme)
        {
            if (image == null) return;
            var icon = GetIcon(iconName, isDarkTheme);
            if (icon != null)
                image.Source = icon;
        }
    }
}