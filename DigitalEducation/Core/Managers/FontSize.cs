using System;
using System.Windows;

namespace DigitalEducation.Core.Managers
{
    public class FontSize
    {
        private const double DefaultSize = 16;
        private const double MinSize = 12;
        private const double MaxSize = 20;
        private const double Step = 2;

        private double _currentSize;
        private readonly AppSettings _settingsService;

        public event EventHandler<double> FontSizeChanged;

        public double CurrentSize => _currentSize;

        public FontSize(AppSettings settingsService)
        {
            _settingsService = settingsService;
            _currentSize = LoadFromSettings();
            ApplySize(_currentSize);
        }

        public void SetSize(double size)
        {
            if (size < MinSize) size = MinSize;
            if (size > MaxSize) size = MaxSize;
            size = Math.Round(size / Step) * Step;

            if (Math.Abs(size - _currentSize) < 0.01)
                return;

            _currentSize = size;
            SaveToSettings(size);
            ApplySize(size);
            FontSizeChanged?.Invoke(this, size);
        }

        public void Increase()
        {
            double newSize = Math.Min(_currentSize + Step, MaxSize);
            SetSize(newSize);
        }

        public void Decrease()
        {
            double newSize = Math.Max(_currentSize - Step, MinSize);
            SetSize(newSize);
        }

        public void Reset()
        {
            SetSize(DefaultSize);
        }

        private double LoadFromSettings()
        {
            try
            {
                double saved = _settingsService.Current.FontSize;
                if (saved >= MinSize && saved <= MaxSize)
                    return saved;
            }
            catch
            {
            }
            return DefaultSize;
        }

        private void SaveToSettings(double size)
        {
            try
            {
                _settingsService.Current.FontSize = size;
                _settingsService.Save();
            }
            catch
            {
            }
        }

        private void ApplySize(double size)
        {
            if (Application.Current?.Resources == null)
                return;

            try
            {
                Application.Current.Resources["FontSizeXs"] = size * 0.75;
                Application.Current.Resources["FontSizeSm"] = size * 0.875;
                Application.Current.Resources["FontSizeBase"] = size;
                Application.Current.Resources["FontSizeLg"] = size * 1.125;
                Application.Current.Resources["FontSizeXl"] = size * 1.25;
                Application.Current.Resources["FontSize2Xl"] = size * 1.5;
                Application.Current.Resources["FontSize3Xl"] = size * 1.875;
                Application.Current.Resources["FontSize4Xl"] = size * 2.25;
                Application.Current.Resources["FontSize5Xl"] = size * 2.75;
                Application.Current.Resources["FontSize6Xl"] = size * 3.375;

                Thickness padding = new Thickness(20, size * 0.5, 20, size * 0.5);
                Application.Current.Resources["PaddingFilterButton"] = padding;

                foreach (Window window in Application.Current.Windows)
                {
                    if (window.IsLoaded)
                    {
                        window.InvalidateVisual();
                        window.UpdateLayout();
                    }
                }
            }
            catch
            {
            }
        }
    }
}