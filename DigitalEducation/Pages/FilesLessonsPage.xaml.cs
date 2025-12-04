using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DigitalEducation
{
    public partial class FilesLessonsPage : UserControl
    {
        public FilesLessonsPage()
        {
            InitializeComponent();
            this.Loaded += FilesLessonsPage_Loaded;
            ThemeManager.ThemeChanged += OnThemeChanged;
        }

        private void FilesLessonsPage_Loaded(object sender, RoutedEventArgs e)
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
            }
        }

        private void FilesLessonsPage_Unloaded(object sender, RoutedEventArgs e)
        {
            ThemeManager.ThemeChanged -= OnThemeChanged;
        }

        private void OnThemeChanged(object sender, string themeName)
        {
            UpdateIcons();
        }

        private void UpdateIcons()
        {
            ThemeManager.UpdateAllIconsInContainer(this);
        }

        private void FindAndSubscribeToLessonButtons()
        {
            ProcessVisualTree(this);
        }

        private void ProcessVisualTree(DependencyObject parent)
        {
            if (parent == null) return;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is Button button)
                {
                    ProcessButton(button);
                }

                ProcessVisualTree(child);
            }
        }

        private void ProcessButton(Button button)
        {
            if (button.Tag == null) return;

            string tag = button.Tag.ToString();

            if (tag.StartsWith("FilesLesson"))
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

                if (LessonManager.LessonExists(lessonTag))
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
            }
        }

        private void ShowLessonNotAvailable(string lessonTag)
        {
            string lessonName = GetLessonName(lessonTag);
        }

        private string GetLessonName(string lessonTag)
        {
            switch (lessonTag)
            {
                case "FilesLesson1":
                    return "Работа с папками";
                case "FilesLesson2":
                    return "Корзина и восстановление";
                case "FilesLesson3":
                    return "Типы файлов и расширения";
                case "FilesLesson4":
                    return "Проводник Windows";
                case "FilesLesson5":
                    return "Копирование и перемещение файлов";
                default:
                    return "Неизвестный урок";
            }
        }

        public void UpdateLessonStatus(string lessonTag, bool isCompleted)
        {
            if (isCompleted)
            {
                Console.WriteLine($"Урок {lessonTag} завершен");
            }
        }

        private void UpdateLessonCards()
        {
            string[] lessonIds = { "FilesLesson1", "FilesLesson2", "FilesLesson3", "FilesLesson4", "FilesLesson5" };

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

                bool isCompleted = ProgressManager.IsLessonCompleted(lessonId);
                var completionInfo = ProgressManager.GetLessonCompletion(lessonId);

                UpdateStatusTextBlock(stackPanel, isCompleted, completionInfo);
                UpdateButtonAppearance(button, isCompleted);
                UpdateLessonNumber(mainGrid, isCompleted);
            }));
        }

        private Button FindButtonByTag(string tag)
        {
            return FindVisualChild<Button>(this, btn => btn.Tag?.ToString() == tag);
        }

        private T FindVisualChild<T>(DependencyObject parent, Func<T, bool> predicate) where T : DependencyObject
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T t && predicate(t))
                {
                    return t;
                }

                var result = FindVisualChild(child, predicate);
                if (result != null) return result;
            }

            return null;
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
                var courseProgress = ProgressManager.GetCourseProgress("Files");

                var progressText = FindVisualChild<TextBlock>(this, tb => tb.Text?.Contains("уроков") == true);
                if (progressText != null)
                {
                    progressText.Text = $"{courseProgress.CompletedLessons}/{courseProgress.TotalLessons} уроков";
                }

                var timeText = FindVisualChild<TextBlock>(this, tb => tb.Text?.Contains("минут") == true);
                if (timeText != null)
                {
                    timeText.Text = $"{courseProgress.TotalTimeMinutes:F0} минут обучения";
                }
            }));
        }
    }
}