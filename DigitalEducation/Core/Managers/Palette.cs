using DigitalEducation.Core.Managers;
using System;
using System.Windows;

namespace DigitalEducation.Core.Managers
{
    public static class Palette
    {
        public const string DefaultPalette = "default";
        public const string OceanPalette = "ocean";
        public const string SunsetPalette = "sunset";
        public const string EmberPalette = "ember";

        private static AppSettings _settingsService;
        public static string CurrentPalette { get; private set; } = DefaultPalette;

        public static event EventHandler PaletteChanged;

        public static void Initialize(AppSettings settingsService)
        {
            _settingsService = settingsService;
            LoadFromStorage();
            ApplyPalette(CurrentPalette);
        }

        private static void LoadFromStorage()
        {
            try
            {
                string savedPalette = _settingsService?.Current?.Palette;
                CurrentPalette = string.IsNullOrEmpty(savedPalette) ? DefaultPalette : savedPalette;
            }
            catch
            {
                CurrentPalette = DefaultPalette;
            }
        }

        public static void SetPalette(string palette)
        {
            if (string.IsNullOrEmpty(palette) || palette == CurrentPalette)
                return;

            CurrentPalette = palette;

            if (_settingsService != null)
            {
                _settingsService.Current.Palette = palette;
                _settingsService.Save();
            }

            ApplyPalette(palette);
            Theme.NotifyBackgroundChanged();
            PaletteChanged?.Invoke(null, EventArgs.Empty);
        }

        public static void ApplyPalette(string palette)
        {
            if (Application.Current == null || Application.Current.Resources == null)
                return;

            string paletteUri = $"/UI/Palettes/Palette{palette}.xaml";

            try
            {
                ResourceDictionary newPaletteDict = new ResourceDictionary
                {
                    Source = new Uri(paletteUri, UriKind.RelativeOrAbsolute)
                };

                ResourceDictionary appResources = Application.Current.Resources;
                ResourceDictionary oldPaletteDict = FindPaletteDictionary(appResources);

                if (oldPaletteDict != null)
                {
                    appResources.MergedDictionaries.Remove(oldPaletteDict);
                }

                appResources.MergedDictionaries.Add(newPaletteDict);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Palette apply error: {ex.Message}");
            }
        }

        private static ResourceDictionary FindPaletteDictionary(ResourceDictionary resources)
        {
            foreach (ResourceDictionary dict in resources.MergedDictionaries)
            {
                if (dict.Source != null)
                {
                    string source = dict.Source.ToString().ToLowerInvariant();
                    if (source.Contains("/palettes/palette") && source.Contains(".xaml"))
                    {
                        return dict;
                    }
                }
            }
            return null;
        }

        public static bool IsDefaultPalette => CurrentPalette == DefaultPalette;
        public static bool IsOceanPalette => CurrentPalette == OceanPalette;
        public static bool IsSunsetPalette => CurrentPalette == SunsetPalette;
        public static bool IsEmberPalette => CurrentPalette == EmberPalette;
    }
}