using DigitalEducation.Core.Managers;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace DigitalEducation.UI.Pages
{
    public partial class Settings : Window
    {
        private Base _baseLogic;
        private FontSize _fontSize;
        private AppSettings _appSettings;

        public Settings()
        {
            InitializeComponent();
            _baseLogic = new Base(this, "settings");

            App app = (App)Application.Current;
            _fontSize = app.GetFontSizeService();
            _appSettings = app.GetSettingsService();

            _fontSize.FontSizeChanged += OnFontSizeChanged;
        }

        protected override void OnClosed(EventArgs e)
        {
            _baseLogic.Cleanup();
            if (_fontSize != null)
            {
                _fontSize.FontSizeChanged -= OnFontSizeChanged;
            }
            base.OnClosed(e);
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            UpdateButtonsState();
            UpdatePaletteButtonsState();
            FontSizeSlider.Value = _fontSize.CurrentSize;
            SettingsPathText.Text = _appSettings.GetSettingsPath();
        }

        private void OnFontSizeChanged(object sender, double size)
        {
            FontSizeSlider.Value = size;
        }

        private void FontSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_fontSize != null && IsLoaded)
            {
                double size = Math.Round(e.NewValue / 2) * 2;
                if (size >= 12 && size <= 20)
                {
                    _fontSize.SetSize(size);
                }
            }
        }

        private void UpdatePaletteButtonsState()
        {
            PaletteDefault.IsChecked = Palette.IsDefaultPalette;
            PaletteOcean.IsChecked = Palette.IsOceanPalette;
            PaletteSunset.IsChecked = Palette.IsSunsetPalette;
            PaletteEmber.IsChecked = Palette.IsEmberPalette;
        }

        private void PaletteDefault_Click(object sender, RoutedEventArgs e)
        {
            Palette.SetPalette(Palette.DefaultPalette);
        }

        private void PaletteOcean_Click(object sender, RoutedEventArgs e)
        {
            Palette.SetPalette(Palette.OceanPalette);
        }

        private void PaletteSunset_Click(object sender, RoutedEventArgs e)
        {
            Palette.SetPalette(Palette.SunsetPalette);
        }

        private void PaletteEmber_Click(object sender, RoutedEventArgs e)
        {
            Palette.SetPalette(Palette.EmberPalette);
        }

        private void UpdateButtonsState()
        {
            DarkThemeBtn.IsChecked = Theme.IsDarkTheme;
            LightThemeBtn.IsChecked = Theme.IsLightTheme;
        }

        private void DarkThemeBtn_Checked(object sender, RoutedEventArgs e)
        {
            if (DarkThemeBtn.IsChecked == true)
            {
                Theme.SetTheme(Theme.DarkTheme);
            }
        }

        private void LightThemeBtn_Checked(object sender, RoutedEventArgs e)
        {
            if (LightThemeBtn.IsChecked == true)
            {
                Theme.SetTheme(Theme.LightTheme);
            }
        }

        private void ExportBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "JSON files (*.json)|*.json",
                    DefaultExt = ".json",
                    FileName = "DigitalEducation_settings_backup.json"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    string json = _appSettings.ExportSettings();
                    File.WriteAllText(saveDialog.FileName, json);

                    _baseLogic.ShowModal(
                        title: "Экспорт данных",
                        message: "Данные успешно экспортированы",
                        confirmText: "OK",
                        cancelText: null
                    );
                }
            }
            catch (Exception ex)
            {
                _baseLogic.ShowModal(
                    title: "Ошибка экспорта",
                    message: $"Не удалось экспортировать данные: {ex.Message}",
                    confirmText: "OK",
                    cancelText: null
                );
            }
        }

        private void ImportBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openDialog = new OpenFileDialog
                {
                    Filter = "JSON files (*.json)|*.json",
                    DefaultExt = ".json"
                };

                if (openDialog.ShowDialog() == true)
                {
                    string json = File.ReadAllText(openDialog.FileName);
                    bool success = _appSettings.ImportSettings(json);

                    if (success)
                    {
                        _appSettings.ApplyAllSettings();

                        _baseLogic.ShowModal(
                            title: "Импорт данных",
                            message: "Данные успешно импортированы",
                            confirmText: "OK",
                            cancelText: null
                        );
                    }
                    else
                    {
                        _baseLogic.ShowModal(
                            title: "Ошибка импорта",
                            message: "Неверный формат файла",
                            confirmText: "OK",
                            cancelText: null
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                _baseLogic.ShowModal(
                    title: "Ошибка импорта",
                    message: $"Не удалось импортировать данные: {ex.Message}",
                    confirmText: "OK",
                    cancelText: null
                );
            }
        }

        private void ResetProgressBtn_Click(object sender, RoutedEventArgs e)
        {
            _baseLogic.ShowModal(
                title: "Сброс прогресса",
                message: "Вы уверены, что хотите удалить весь прогресс обучения? Это действие нельзя отменить.",
                confirmText: "Сбросить",
                cancelText: "Отмена",
                onConfirm: () =>
                {
                    _baseLogic.ShowModal(
                        title: "Успешно",
                        message: "Прогресс успешно сброшен",
                        confirmText: "OK",
                        cancelText: null
                    );
                }
            );
        }

        private void ResetSettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            _baseLogic.ShowModal(
                title: "Сброс настроек",
                message: "Вы уверены, что хотите сбросить все настройки к значениям по умолчанию?",
                confirmText: "Сбросить",
                cancelText: "Отмена",
                onConfirm: () =>
                {
                    _appSettings.Reset();
                    _appSettings.ApplyAllSettings();
                    _baseLogic.ShowModal(
                        title: "Успешно",
                        message: "Настройки успешно сброшены",
                        confirmText: "OK",
                        cancelText: null
                    );
                }
            );
        }

        private void FeedbackBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string subject = Uri.EscapeDataString("Обратная связь DigitalEducation");
                string body = Uri.EscapeDataString("Введите ваше сообщение здесь...");
                string mailto = $"mailto:official.royce@yandex.ru?subject={subject}&body={body}";

                System.Diagnostics.Process.Start(mailto);
            }
            catch (Exception ex)
            {
                _baseLogic.ShowModal(
                    title: "Ошибка",
                    message: $"Не удалось открыть почтовую программу: {ex.Message}",
                    confirmText: "OK",
                    cancelText: null
                );
            }
        }

        private void FaqBtn_Click(object sender, RoutedEventArgs e)
        {
            _baseLogic.ShowModal(
                title: "Часто задаваемые вопросы",
                message: "Здесь будут отображаться часто задаваемые вопросы",
                confirmText: "Закрыть",
                cancelText: null
            );
        }
    }
}