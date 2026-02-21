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

        public SettingsPage()
        {
            InitializeComponent();
            ThemeManager.ThemeChanged += OnThemeChanged;
            Loaded += OnSettingsPageLoaded;
        }

        private void OnSettingsPageLoaded(object sender, RoutedEventArgs e)
        {
            InitializeEventHandlers();
            LoadCurrentTheme();
            LoadSettingsData();
            UpdateAllButtons();
            ThemeManager.UpdateAllIconsInContainer(this);
        }

        private void LoadCurrentTheme()
        {
            _currentTheme = ThemeManager.GetCurrentTheme();
        }

        private void OnThemeChanged(object sender, string themeName)
        {
            _currentTheme = themeName;
            UpdateThemeButtons();
            ThemeManager.UpdateAllIconsInContainer(this);
        }

        private void InitializeEventHandlers()
        {
            btnClearProgress.Click += OnClearProgressClick;
            btnResetSettings.Click += OnResetSettingsClick;
            btnLightTheme.Click += OnLightThemeClick;
            btnDarkTheme.Click += OnDarkThemeClick;
            btnSmallScale.Click += OnSmallScaleClick;
            btnMediumScale.Click += OnMediumScaleClick;
            btnLargeScale.Click += OnLargeScaleClick;
        }

        private void LoadSettingsData()
        {
            try
            {
                var progressFilePath = ProgressManager.GetProgressFilePath();
                txtProgressPath.Text = $"Прогресс: {progressFilePath}\n";

                var themeFilePath = ThemeManager.GetThemeConfigFilePath();
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
            UpdateScaleButtons();
        }

        private void OnLightThemeClick(object sender, RoutedEventArgs e)
        {
            if (_currentTheme != "Light")
            {
                _currentTheme = "Light";
                ThemeManager.ApplyTheme("Light");
                UpdateThemeButtons();
                SettingsButtonClicked?.Invoke(this, "ThemeChanged:Light");
            }
        }

        private void OnDarkThemeClick(object sender, RoutedEventArgs e)
        {
            if (_currentTheme != "Dark")
            {
                _currentTheme = "Dark";
                ThemeManager.ApplyTheme("Dark");
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

        private void OnSmallScaleClick(object sender, RoutedEventArgs e)
        {
            if (_currentScale != "Small")
            {
                _currentScale = "Small";
                UpdateScaleButtons();
            }
        }

        private void OnMediumScaleClick(object sender, RoutedEventArgs e)
        {
            if (_currentScale != "Medium")
            {
                _currentScale = "Medium";
                UpdateScaleButtons();
            }
        }

        private void OnLargeScaleClick(object sender, RoutedEventArgs e)
        {
            if (_currentScale != "Large")
            {
                _currentScale = "Large";
                UpdateScaleButtons();
            }
        }

        private void UpdateScaleButtons()
        {
            var activeStyle = (Style)TryFindResource("ActiveNavigationButtonStyle");
            var defaultStyle = (Style)TryFindResource("NavigationButtonStyle");

            if (btnSmallScale != null) btnSmallScale.Style = _currentScale == "Small" ? activeStyle : defaultStyle;
            if (btnMediumScale != null) btnMediumScale.Style = _currentScale == "Medium" ? activeStyle : defaultStyle;
            if (btnLargeScale != null) btnLargeScale.Style = _currentScale == "Large" ? activeStyle : defaultStyle;
        }

        private void OnClearProgressClick(object sender, RoutedEventArgs e)
        {
            var result = DialogService.ShowConfirmDialog(
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
                ProgressManager.ResetProgress();
                LoadSettingsData();

                DialogService.ShowSuccessDialog(
                    "Все данные о вашем обучении были успешно удалены.\nТеперь вы можете начать обучение заново.",
                    Window.GetWindow(this)
                );

                SettingsButtonClicked?.Invoke(this, "ProgressReset");
            }
            catch (Exception ex)
            {
                DialogService.ShowErrorDialog(
                    $"Ошибка при очистке прогресса: {ex.Message}",
                    Window.GetWindow(this)
                );
            }
        }

        private void OnResetSettingsClick(object sender, RoutedEventArgs e)
        {
            var result = DialogService.ShowConfirmDialog(
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

                ThemeManager.ApplyTheme("Light");

                UpdateAllButtons();
                ThemeManager.UpdateAllIconsInContainer(this);

                SettingsButtonClicked?.Invoke(this, "ThemeChanged:Light");

                DialogService.ShowSuccessDialog(
                    "Все настройки приложения были успешно сброшены к значениям по умолчанию.",
                    Window.GetWindow(this)
                );
            }
            catch (Exception ex)
            {
                DialogService.ShowErrorDialog(
                    $"Ошибка при сбросе настроек: {ex.Message}",
                    Window.GetWindow(this)
                );
            }
        }
    }
}