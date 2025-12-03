using System;
using System.Windows;
using System.Windows.Controls;

namespace DigitalEducation
{
    public partial class SettingsPage : UserControl
    {
        public event EventHandler<string> SettingsButtonClicked;

        public SettingsPage()
        {
            InitializeComponent();
            Loaded += OnSettingsPageLoaded;
        }

        private void OnSettingsPageLoaded(object sender, RoutedEventArgs e)
        {
            InitializeEventHandlers();
            LoadSettingsData();
            Loaded -= OnSettingsPageLoaded;
        }

        private void InitializeEventHandlers()
        {
            btnClearProgress.Click -= OnClearProgressClick;
            btnClearProgress.Click += OnClearProgressClick;

            btnResetSettings.Click -= OnResetSettingsClick;
            btnResetSettings.Click += OnResetSettingsClick;
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
        }
    }
}