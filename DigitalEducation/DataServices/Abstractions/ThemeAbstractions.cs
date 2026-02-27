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
        string LoadOverlayPosition();
        void SaveOverlayPosition(string position);
        string GetConfigPath();
    }

    public interface IIconProvider
    {
        BitmapImage GetIcon(string iconName, bool isDarkTheme);
        void UpdateImageSource(System.Windows.Controls.Image image, string iconName, bool isDarkTheme);
    }

    public class FileSystemThemeRepository : IThemeRepository
    {
        private readonly string _configPath;
        private readonly JsonSerializerOptions _jsonOptions;

        public FileSystemThemeRepository()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appFolder = Path.Combine(appDataPath, "DigitalEducation");
            if (!Directory.Exists(appFolder))
                Directory.CreateDirectory(appFolder);
            _configPath = Path.Combine(appFolder, "theme_config.json");
            _jsonOptions = new JsonSerializerOptions { WriteIndented = true };
        }

        private class ThemeConfig
        {
            public string ThemeName { get; set; } = "Light";
            public string OverlayPosition { get; set; } = "TopRight";
        }

        private ThemeConfig LoadConfig()
        {
            if (!File.Exists(_configPath))
                return new ThemeConfig();
            try
            {
                string json = File.ReadAllText(_configPath);
                return JsonSerializer.Deserialize<ThemeConfig>(json) ?? new ThemeConfig();
            }
            catch
            {
                return new ThemeConfig();
            }
        }

        private void SaveConfig(ThemeConfig config)
        {
            try
            {
                string json = JsonSerializer.Serialize(config, _jsonOptions);
                File.WriteAllText(_configPath, json);
            }
            catch { }
        }

        public string LoadTheme()
        {
            return LoadConfig().ThemeName;
        }

        public void SaveTheme(string themeName)
        {
            var config = LoadConfig();
            config.ThemeName = themeName;
            SaveConfig(config);
        }

        public string LoadOverlayPosition()
        {
            return LoadConfig().OverlayPosition;
        }

        public void SaveOverlayPosition(string position)
        {
            var config = LoadConfig();
            config.OverlayPosition = position;
            SaveConfig(config);
        }

        public string GetConfigPath()
        {
            return _configPath;
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
            ["Calendar"] = "Calendar",
            ["VK"] = "VK",
            ["GitHub"] = "GitHub",
            ["Square"] = "Square",
            ["Copy"] = "Copy",
            ["Git"] = "Git",
            ["Layers"] = "Layers",
            ["Layout"] = "Layout",
            ["Maximize"] = "Maximize",
            ["Image"] = "Image"
        };

        public BitmapImage GetIcon(string iconName, bool isDarkTheme)
        {
            if (!_iconMapping.ContainsKey(iconName))
                return null;

            string path = isDarkTheme
                ? $"/Assets/Icons/White/{_iconMapping[iconName]}.png"
                : $"/Assets/Icons/Black/{_iconMapping[iconName]}.png";

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