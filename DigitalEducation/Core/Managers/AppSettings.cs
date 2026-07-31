using System;
using System.Collections.Generic;
using System.IO;
using DigitalEducation.Core.Models;
using Newtonsoft.Json;

namespace DigitalEducation.Core.Managers
{
    public class AppSettings
    {
        private const string SettingsFileName = "settings.json";
        private readonly string _settingsPath;
        private AppSettingsModel _settings;
        private bool _isLoaded;

        public event EventHandler<AppSettingsModel> SettingsChanged;

        public AppSettingsModel Current => _settings;

        public AppSettings()
        {
            _settingsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "DigitalEducation",
                SettingsFileName
            );

            _settings = new AppSettingsModel();
            Load();
        }

        public void Load()
        {
            try
            {
                string directory = Path.GetDirectoryName(_settingsPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                if (File.Exists(_settingsPath))
                {
                    string json = File.ReadAllText(_settingsPath);
                    AppSettingsModel loaded = JsonConvert.DeserializeObject<AppSettingsModel>(json);
                    if (loaded != null)
                    {
                        _settings = loaded;
                    }
                }
                else
                {
                    Save();
                }

                _isLoaded = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load settings: {ex.Message}");
                _settings = new AppSettingsModel();
                Save();
            }
        }

        public void Save()
        {
            try
            {
                string directory = Path.GetDirectoryName(_settingsPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string json = JsonConvert.SerializeObject(_settings, Formatting.Indented);
                File.WriteAllText(_settingsPath, json);

                SettingsChanged?.Invoke(this, _settings);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save settings: {ex.Message}");
            }
        }

        public T GetValue<T>(string key, T defaultValue = default(T))
        {
            if (_settings.CustomSettings.TryGetValue(key, out object value))
            {
                try
                {
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                catch
                {
                    return defaultValue;
                }
            }
            return defaultValue;
        }

        public void SetValue<T>(string key, T value)
        {
            _settings.CustomSettings[key] = value;
            Save();
        }

        public void Reset()
        {
            _settings = new AppSettingsModel();
            Save();
        }

        public string GetSettingsPath()
        {
            return _settingsPath;
        }

        public string ExportSettings()
        {
            return JsonConvert.SerializeObject(_settings, Formatting.Indented);
        }

        public bool ImportSettings(string json)
        {
            try
            {
                AppSettingsModel imported = JsonConvert.DeserializeObject<AppSettingsModel>(json);
                if (imported != null)
                {
                    _settings = imported;
                    Save();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public void ApplyAllSettings()
        {
            string theme = Current.Theme;
            Theme.SetTheme(theme);
            Theme.ApplyTheme(theme);

            string palette = Current.Palette;
            Palette.SetPalette(palette);
            Palette.ApplyPalette(palette);

            double fontSize = Current.FontSize;
            App app = System.Windows.Application.Current as App;
            FontSize fontService = app?.GetFontSizeService();
            fontService?.SetSize(fontSize);

            Theme.NotifyBackgroundChanged();

            foreach (System.Windows.Window window in System.Windows.Application.Current.Windows)
            {
                if (window.IsLoaded)
                {
                    window.InvalidateVisual();
                    window.UpdateLayout();
                }
            }
        }
    }
}