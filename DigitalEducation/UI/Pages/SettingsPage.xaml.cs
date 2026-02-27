using System;
using System.Windows;
using System.Windows.Controls;

namespace DigitalEducation
{
    public partial class SettingsPage : UserControl, IPage
    {
        public event EventHandler<string> SettingsButtonClicked;

        private string _currentTheme;
        private string _currentScale = "Medium";
        private string _currentPosition;

        public SettingsPage()
        {
            InitializeComponent();
            AppThemeManager.ThemeChanged += OnThemeChanged;
            Loaded += OnSettingsPageLoaded;
        }

        private void OnSettingsPageLoaded(object sender, RoutedEventArgs e)
        {
            InitializeEventHandlers();
            LoadCurrentTheme();
            LoadSettingsData();
            UpdateAllButtons();
            LoadCurrentPosition();
            AppThemeManager.UpdateAllIconsInContainer(this);
        }

        private void LoadCurrentPosition()
        {
            _currentPosition = AppThemeManager.GetOverlayPosition();
            UpdatePositionButtons();
        }

        private void UpdatePositionButtons()
        {
            var activeStyle = (Style)TryFindResource("ActiveNavigationButtonStyle");
            var defaultStyle = (Style)TryFindResource("NavigationButtonStyle");
            btnTopLeft.Style = _currentPosition == "TopLeft" ? activeStyle : defaultStyle;
            btnTopRight.Style = _currentPosition == "TopRight" ? activeStyle : defaultStyle;
            btnBottomLeft.Style = _currentPosition == "BottomLeft" ? activeStyle : defaultStyle;
            btnBottomRight.Style = _currentPosition == "BottomRight" ? activeStyle : defaultStyle;
        }

        private void OnPositionButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string position)
            {
                if (_currentPosition != position)
                {
                    _currentPosition = position;
                    AppThemeManager.SetOverlayPosition(position);
                    UpdatePositionButtons();
                }
            }
        }

        private void LoadCurrentTheme()
        {
            _currentTheme = AppThemeManager.GetCurrentTheme();
        }

        private void OnThemeChanged(object sender, string themeName)
        {
            _currentTheme = themeName;
            UpdateThemeButtons();
            AppThemeManager.UpdateAllIconsInContainer(this);
        }

        private void InitializeEventHandlers()
        {
            btnClearProgress.Click += OnClearProgressClick;
            btnResetSettings.Click += OnResetSettingsClick;
            btnLightTheme.Click += OnLightThemeClick;
            btnDarkTheme.Click += OnDarkThemeClick;
        }

        private void LoadSettingsData()
        {
            try
            {
                var progressFilePath = UserProgressManager.GetProgressFilePath();
                txtProgressPath.Text = $"Прогресс: {progressFilePath}\n";

                var themeFilePath = AppThemeManager.GetThemeConfigFilePath();
                txtProgressPath.Text += $"Тема: {themeFilePath}";
            }
            catch (Exception ex)
            {
                txtProgressPath.Text = $"Ошибка загрузки: {ex.Message}";
            }
        }

        private void UpdateAllButtons()
        {
            UpdateThemeButtons();
        }

        private void OnLightThemeClick(object sender, RoutedEventArgs e)
        {
            if (_currentTheme != "Light")
            {
                _currentTheme = "Light";
                AppThemeManager.ApplyTheme("Light");
                UpdateThemeButtons();
                SettingsButtonClicked?.Invoke(this, "ThemeChanged:Light");
            }
        }

        private void OnDarkThemeClick(object sender, RoutedEventArgs e)
        {
            if (_currentTheme != "Dark")
            {
                _currentTheme = "Dark";
                AppThemeManager.ApplyTheme("Dark");
                UpdateThemeButtons();
                SettingsButtonClicked?.Invoke(this, "ThemeChanged:Dark");
            }
        }

        private void UpdateThemeButtons()
        {
            var activeStyle = (Style)TryFindResource("ActiveNavigationButtonStyle");
            var defaultStyle = (Style)TryFindResource("NavigationButtonStyle");

            if (btnLightTheme != null) btnLightTheme.Style = _currentTheme == "Light" ? activeStyle : defaultStyle;
            if (btnDarkTheme != null) btnDarkTheme.Style = _currentTheme == "Dark" ? activeStyle : defaultStyle;
        }

        private void OnClearProgressClick(object sender, RoutedEventArgs e)
        {
            var result = AppDialogService.ShowConfirmDialog(
                "Очистка прогресса",
                "Вы уверены, что хотите очистить весь прогресс?\n\n" +
                "Это действие удалит:\n" +
                "• Все завершенные уроки\n" +
                "• Статистику обучения\n" +
                "• Прогресс по всем курсам\n\n" +
                "Это действие нельзя отменить.",
                "Очистить",
                "Отмена",
                Window.GetWindow(this)
            );

            if (result == true)
            {
                ExecuteClearProgress();
            }
        }

        private void ExecuteClearProgress()
        {
            try
            {
                UserProgressManager.ResetProgress();
                LoadSettingsData();

                AppDialogService.ShowSuccessDialog(
                    "Все данные о вашем обучении были успешно удалены.\nТеперь вы можете начать обучение заново.",
                    Window.GetWindow(this)
                );

                SettingsButtonClicked?.Invoke(this, "ProgressReset");
            }
            catch (Exception ex)
            {
                AppDialogService.ShowErrorDialog(
                    $"Ошибка при очистке прогресса: {ex.Message}",
                    Window.GetWindow(this)
                );
            }
        }

        private void OnResetSettingsClick(object sender, RoutedEventArgs e)
        {
            var result = AppDialogService.ShowConfirmDialog(
                "Сброс настроек",
                "Вы уверены, что хотите сбросить все настройки приложения?\n\n" +
                "Это действие вернет:\n" +
                "• Тему на светлую\n" +
                "• Масштаб на средний\n\n" +
                "Ваш прогресс обучения не будет затронут.",
                "Сбросить",
                "Отмена",
                Window.GetWindow(this)
            );

            if (result == true)
            {
                ExecuteResetSettings();
            }
        }

        private void ExecuteResetSettings()
        {
            try
            {
                _currentTheme = "Light";
                _currentScale = "Medium";

                AppThemeManager.ApplyTheme("Light");

                UpdateAllButtons();
                AppThemeManager.UpdateAllIconsInContainer(this);

                SettingsButtonClicked?.Invoke(this, "ThemeChanged:Light");

                AppDialogService.ShowSuccessDialog(
                    "Все настройки приложения были успешно сброшены к значениям по умолчанию.",
                    Window.GetWindow(this)
                );
            }
            catch (Exception ex)
            {
                AppDialogService.ShowErrorDialog(
                    $"Ошибка при сбросе настроек: {ex.Message}",
                    Window.GetWindow(this)
                );
            }
        }
    }
}