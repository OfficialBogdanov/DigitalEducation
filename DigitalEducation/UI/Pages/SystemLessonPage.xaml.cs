using DigitalEducation.Utilities;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DigitalEducation
{
    public partial class SystemLessonsPage : UserControl, IPage
    {
        public SystemLessonsPage()
        {
            InitializeComponent();
            this.Loaded += SystemLessonsPage_Loaded;
            AppThemeManager.ThemeChanged += OnThemeChanged;
        }

        private void SystemLessonsPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                FindAndSubscribeToLessonButtons();
                UpdateLessonCards();
                UpdateCourseProgress();
                UpdateIcons();
            }
            catch (Exception ex)
            {
                AppDialogService.ShowErrorDialog(
                    $"Ошибка при загрузке страницы уроков: {ex.Message}",
                    Window.GetWindow(this)
                );
            }
        }

        private void SystemLessonsPage_Unloaded(object sender, RoutedEventArgs e)
        {
            AppThemeManager.ThemeChanged -= OnThemeChanged;
        }

        private void OnThemeChanged(object sender, string themeName)
        {
            UpdateIcons();
        }

        private void UpdateIcons()
        {
            AppThemeManager.UpdateAllIconsInContainer(this);
        }

        private void FindAndSubscribeToLessonButtons()
        {
            var buttons = VisualTreeHelperExtensions.FindVisualChildren<Button>(this);
            foreach (var button in buttons)
            {
                ProcessButton(button);
            }
        }

        private void ProcessButton(Button button)
        {
            if (button.Tag == null) return;

            string tag = button.Tag.ToString();

            if (tag.StartsWith("OsLesson"))
            {
                button.Click -= LessonButton_Click;
                button.Click += LessonButton_Click;
            }
        }

        private void LessonButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag != null)
            {
                string lessonTag = button.Tag.ToString();

                bool isCompleted = UserProgressManager.IsLessonCompleted(lessonTag);

                if (isCompleted)
                {
                    var result = AppDialogService.ShowConfirmDialog(
                        "Повторное прохождение",
                        $"Вы уже завершили этот урок.\nХотите пройти его снова?",
                        "Повторить",
                        "Отмена",
                        Window.GetWindow(this)
                    );

                    if (result != true)
                    {
                        return;
                    }
                }

                if (LessonRegistry.LessonExists(lessonTag))
                {
                    LaunchLesson(lessonTag);
                }
                else
                {
                    ShowLessonNotAvailable(lessonTag);
                }
            }
        }

        private void LaunchLesson(string lessonId)
        {
            try
            {
                OverlayWindow lessonWindow = new OverlayWindow(lessonId);

                Window mainWindow = Application.Current.MainWindow;
                if (mainWindow != null && mainWindow.IsLoaded)
                {
                    lessonWindow.Owner = mainWindow;
                    lessonWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                }
                else
                {
                    lessonWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                }

                bool? result = lessonWindow.ShowDialog();

                if (result == true)
                {
                    UpdateLessonStatus(lessonId, true);
                    UpdateLessonCards();
                    UpdateCourseProgress();
                }
            }
            catch (Exception ex)
            {
                AppDialogService.ShowErrorDialog(
                    $"Не удалось запустить урок: {ex.Message}",
                    Window.GetWindow(this)
                );
            }
        }

        private void ShowLessonNotAvailable(string lessonTag)
        {
            AppDialogService.ShowMessageDialog(
                "Урок недоступен",
                "Этот урок временно недоступен.\nПожалуйста, попробуйте позже.",
                "OK",
                Window.GetWindow(this)
            );
        }

        public void UpdateLessonStatus(string lessonTag, bool isCompleted)
        {
        }

        private void UpdateLessonCards()
        {
            string[] lessonIds = { "OsLesson1", "OsLesson2", "OsLesson3", "OsLesson4", "OsLesson5", "OsLesson6", "OsLesson7" };

            foreach (string lessonId in lessonIds)
            {
                UpdateLessonCardUI(lessonId);
            }
        }

        private void UpdateLessonCardUI(string lessonId)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                var button = FindButtonByTag(lessonId);
                if (button == null) return;

                var parentGrid = button.Parent as Grid;
                if (parentGrid == null) return;

                var cardBorder = parentGrid.Parent as Border;
                if (cardBorder == null) return;

                var mainGrid = cardBorder.Child as Grid;
                if (mainGrid == null) return;

                var stackPanel = mainGrid.Children[1] as StackPanel;
                if (stackPanel == null) return;

                bool isCompleted = UserProgressManager.IsLessonCompleted(lessonId);
                var completionInfo = UserProgressManager.GetLessonCompletion(lessonId);

                UpdateStatusTextBlock(stackPanel, isCompleted, completionInfo);
                UpdateButtonAppearance(button, isCompleted);
                UpdateLessonNumber(mainGrid, isCompleted);
            }));
        }

        private Button FindButtonByTag(string tag)
        {
            return VisualTreeHelperExtensions.FindVisualChild<Button>(this, btn => btn.Tag?.ToString() == tag);
        }

        private void UpdateStatusTextBlock(StackPanel stackPanel, bool isCompleted, CompletedLesson completionInfo)
        {
            var statusGrid = stackPanel.Children[2] as Grid;
            if (statusGrid == null) return;

            if (statusGrid.Children.Count > 1 && statusGrid.Children[1] is TextBlock statusText)
            {
                if (isCompleted && completionInfo != null)
                {
                    statusText.Text = $"✓ Завершено ({completionInfo.TimeSpentMinutes:F1} мин)";
                    statusText.Foreground = new SolidColorBrush(Colors.Green);
                    statusText.FontWeight = FontWeights.Bold;
                }
                else
                {
                    statusText.Text = "Не пройден";
                    statusText.Foreground = new SolidColorBrush(Colors.Gray);
                    statusText.FontWeight = FontWeights.Normal;
                }
            }
        }

        private void UpdateButtonAppearance(Button button, bool isCompleted)
        {
            var stackPanel = button.Content as StackPanel;
            if (stackPanel == null) return;

            if (stackPanel.Children[1] is TextBlock buttonText)
            {
                buttonText.Text = isCompleted ? "Повторить" : "Начать";
            }
        }

        private void UpdateLessonNumber(Grid mainGrid, bool isCompleted)
        {
            var numberBorder = mainGrid.Children[0] as Border;
            if (numberBorder == null) return;

            if (isCompleted)
            {
                numberBorder.Background = new SolidColorBrush(Color.FromRgb(40, 167, 69));
            }
        }

        private void UpdateCourseProgress()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                var courseProgress = UserProgressManager.GetCourseProgress("System");
                if (CourseProgressText != null)
                {
                    CourseProgressText.Text = $"{courseProgress.CompletedLessons}/{courseProgress.TotalLessons} уроков";
                }
            }));
        }
    }
}