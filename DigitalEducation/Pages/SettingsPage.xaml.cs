using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DigitalEducation
{
    public partial class SettingsPage : UserControl
    {
        public event EventHandler<string> SettingsButtonClicked;

        private string _currentTheme = "Light";
        private string _currentLanguage = "Russian";
        private string _currentScale = "Medium";

        public SettingsPage()
        {
            InitializeComponent();
            Loaded += OnSettingsPageLoaded;
        }

        private void OnSettingsPageLoaded(object sender, RoutedEventArgs e)
        {
            InitializeEventHandlers();
            LoadSettingsData();
            UpdateThemeButtons();
            Loaded -= OnSettingsPageLoaded;
        }

        private void InitializeEventHandlers()
        {
            btnClearProgress.Click -= OnClearProgressClick;
            btnClearProgress.Click += OnClearProgressClick;

            btnResetSettings.Click -= OnResetSettingsClick;
            btnResetSettings.Click += OnResetSettingsClick;

            btnLightTheme.Click -= OnLightThemeClick;
            btnLightTheme.Click += OnLightThemeClick;

            btnDarkTheme.Click -= OnDarkThemeClick;
            btnDarkTheme.Click += OnDarkThemeClick;

            btnRussian.Click -= OnRussianClick;
            btnRussian.Click += OnRussianClick;

            btnEnglish.Click -= OnEnglishClick;
            btnEnglish.Click += OnEnglishClick;

            btnSmallScale.Click -= OnSmallScaleClick;
            btnSmallScale.Click += OnSmallScaleClick;

            btnMediumScale.Click -= OnMediumScaleClick;
            btnMediumScale.Click += OnMediumScaleClick;

            btnLargeScale.Click -= OnLargeScaleClick;
            btnLargeScale.Click += OnLargeScaleClick;
        }

        private void LoadSettingsData()
        {
            try
            {
                var progressFilePath = ProgressManager.GetProgressFilePath();
                txtProgressPath.Text = progressFilePath;
            }
            catch (Exception ex)
            {
                txtProgressPath.Text = $"Ошибка загрузки: {ex.Message}";
            }
        }

        private void OnSmallScaleClick(object sender, RoutedEventArgs e)
        {
            if (_currentScale != "Small")
            {
                _currentScale = "Small";
                UpdateScaleButtons();
                SettingsButtonClicked?.Invoke(this, "ScaleChanged:Small");
            }
        }

        private void OnMediumScaleClick(object sender, RoutedEventArgs e)
        {
            if (_currentScale != "Medium")
            {
                _currentScale = "Medium";
                UpdateScaleButtons();
                SettingsButtonClicked?.Invoke(this, "ScaleChanged:Medium");
            }
        }

        private void OnLargeScaleClick(object sender, RoutedEventArgs e)
        {
            if (_currentScale != "Large")
            {
                _currentScale = "Large";
                UpdateScaleButtons();
                SettingsButtonClicked?.Invoke(this, "ScaleChanged:Large");
            }
        }

        private void UpdateScaleButtons()
        {
            var activeStyle = (Style)FindResource("ActiveNavigationButtonStyle");
            var defaultStyle = (Style)FindResource("NavigationButtonStyle");

            btnSmallScale.Style = defaultStyle;
            btnMediumScale.Style = defaultStyle;
            btnLargeScale.Style = defaultStyle;

            switch (_currentScale)
            {
                case "Small":
                    btnSmallScale.Style = activeStyle;
                    break;
                case "Medium":
                    btnMediumScale.Style = activeStyle;
                    break;
                case "Large":
                    btnLargeScale.Style = activeStyle;
                    break;
            }
        }

        private void OnRussianClick(object sender, RoutedEventArgs e)
        {
            if (_currentLanguage != "Russian")
            {
                _currentLanguage = "Russian";
                UpdateLanguageButtons();
                SettingsButtonClicked?.Invoke(this, "LanguageChanged:Russian");
            }
        }

        private void OnEnglishClick(object sender, RoutedEventArgs e)
        {
            if (_currentLanguage != "English")
            {
                _currentLanguage = "English";
                UpdateLanguageButtons();
                SettingsButtonClicked?.Invoke(this, "LanguageChanged:English");
            }
        }

        private void UpdateLanguageButtons()
        {
            var activeStyle = (Style)FindResource("ActiveNavigationButtonStyle");
            var defaultStyle = (Style)FindResource("NavigationButtonStyle");

            if (_currentLanguage == "Russian")
            {
                btnRussian.Style = activeStyle;
                btnEnglish.Style = defaultStyle;
            }
            else
            {
                btnEnglish.Style = activeStyle;
                btnRussian.Style = defaultStyle;
            }
        }

        private void OnLightThemeClick(object sender, RoutedEventArgs e)
        {
            if (_currentTheme != "Light")
            {
                _currentTheme = "Light";
                UpdateThemeButtons();
                SettingsButtonClicked?.Invoke(this, "ThemeChanged:Light");
            }
        }

        private void OnDarkThemeClick(object sender, RoutedEventArgs e)
        {
            if (_currentTheme != "Dark")
            {
                _currentTheme = "Dark";
                UpdateThemeButtons();
                SettingsButtonClicked?.Invoke(this, "ThemeChanged:Dark");
            }
        }

        private void UpdateThemeButtons()
        {
            var activeStyle = (Style)FindResource("ActiveNavigationButtonStyle");
            var defaultStyle = (Style)FindResource("NavigationButtonStyle");

            if (_currentTheme == "Light")
            {
                btnLightTheme.Style = activeStyle;
                btnDarkTheme.Style = defaultStyle;
            }
            else
            {
                btnDarkTheme.Style = activeStyle;
                btnLightTheme.Style = defaultStyle;
            }
        }

        private void OnClearProgressClick(object sender, RoutedEventArgs e)
        {
            var dialog = new ConfirmDialog();

            dialog.Title = "Очистка прогресса";
            dialog.Message = "Вы уверены, что хотите очистить весь прогресс?\n\n" +
                           "Это действие удалит:\n" +
                           "• Все завершенные уроки\n" +
                           "• Статистику обучения\n" +
                           "• Прогресс по всем курсам\n\n" +
                           "Это действие нельзя отменить.";
            dialog.ConfirmButtonText = "Очистить";
            dialog.CancelButtonText = "Отмена";

            if (Window.GetWindow(this) is MainWindow mainWindow)
            {
                mainWindow.ShowDialog(dialog, (s, result) =>
                {
                    if (result)
                    {
                        ExecuteClearProgress();
                    }
                });
            }
            else
            {
                var result = MessageBox.Show(
                    "Вы уверены, что хотите очистить весь прогресс?\n\n" +
                    "Это действие удалит:\n" +
                    "• Все завершенные уроки\n" +
                    "• Статистику обучения\n" +
                    "• Прогресс по всем курсам\n\n" +
                    "Это действие нельзя отменить.",
                    "Очистка прогресса",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning,
                    MessageBoxResult.No
                );

                if (result == MessageBoxResult.Yes)
                {
                    ExecuteClearProgress();
                }
            }
        }

        private void ExecuteClearProgress()
        {
            try
            {
                ProgressManager.ResetProgress();
                LoadSettingsData();

                ShowSuccessDialog("Прогресс очищен",
                    "Все данные о вашем обучении были успешно удалены.\n" +
                    "Теперь вы можете начать обучение заново.");

                SettingsButtonClicked?.Invoke(this, "ProgressReset");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка при очистке прогресса: {ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void ShowSuccessDialog(string title, string message)
        {
            var dialog = new ConfirmDialog();

            dialog.Title = title;
            dialog.Message = message;
            dialog.ConfirmButtonText = "Хорошо";
            dialog.CancelButtonText = null;

            if (dialog.FindName("CancelButton") is Button cancelButton)
            {
                cancelButton.Visibility = Visibility.Collapsed;
            }

            if (Window.GetWindow(this) is MainWindow mainWindow)
            {
                mainWindow.ShowDialog(dialog, (s, result) =>
                {
                });
            }
            else
            {
                MessageBox.Show(
                    message,
                    title,
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
        }

        private void OnResetSettingsClick(object sender, RoutedEventArgs e)
        {
        }

        public void RefreshSettings()
        {
            LoadSettingsData();
            UpdateThemeButtons();
        }
    }
}