using System;
using System.Windows;
using System.Windows.Controls;

namespace DigitalEducation
{
    public partial class CoursesPage : UserControl
    {
        public event EventHandler<string> CourseButtonClicked;

        public CoursesPage()
        {
            InitializeComponent();
            Loaded += OnCoursesPageLoaded;
            ProgressManager.ProgressChanged += OnProgressChanged;
            ThemeManager.ThemeChanged += OnThemeChanged;
        }

        private void OnCoursesPageLoaded(object sender, RoutedEventArgs e)
        {
            InitializeEventHandlers();
            UpdateCoursesProgress();
            UpdateIcons();
            Loaded -= OnCoursesPageLoaded;
        }

        private void OnCoursesPageUnloaded(object sender, RoutedEventArgs e)
        {
            ProgressManager.ProgressChanged -= OnProgressChanged;
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

        private void InitializeEventHandlers()
        {
            var allButtons = FindVisualChildren<Button>(this);

            foreach (var button in allButtons)
            {
                if (button.Tag != null && !string.IsNullOrEmpty(button.Tag.ToString()))
                {
                    button.Click -= OnCourseButtonClick;
                    button.Click += OnCourseButtonClick;
                }
            }
        }

        private void OnCourseButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string courseTag)
            {
                string eventTag = courseTag == "Files" ? "OpenFilesLessons" : courseTag;
                CourseButtonClicked?.Invoke(this, eventTag);
            }
        }

        public void SubscribeToButton(Button button)
        {
            if (button != null && button.Tag != null)
            {
                button.Click += OnCourseButtonClick;
            }
        }

        private System.Collections.Generic.IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = System.Windows.Media.VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        if (childOfChild != null)
                        {
                            yield return childOfChild;
                        }
                    }
                }
            }
        }

        private void OnProgressChanged(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                UpdateAllCoursesProgress();
            }));
        }

        private void UpdateCoursesProgress()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                UpdateCourseProgress("Files", btnFilesCourse);
                UpdateAllCoursesProgress();
            }));
        }

        public void RefreshProgress()
        {
            UpdateCoursesProgress();
        }

        private void UpdateCourseProgress(string courseId, Button courseButton)
        {
            var progress = ProgressManager.GetCourseProgress(courseId);

            if (courseButton != null)
            {
                var parentGrid = courseButton.Parent as Grid;
                if (parentGrid != null)
                {
                    var stackPanel = parentGrid.Children[1] as StackPanel;
                    if (stackPanel != null)
                    {
                        UpdateProgressInStackPanel(stackPanel, progress);
                    }
                }
            }
        }

        private void UpdateProgressInStackPanel(StackPanel stackPanel, CourseProgress progress)
        {
            foreach (var child in stackPanel.Children)
            {
                if (child is StackPanel innerStackPanel)
                {
                    UpdateProgressInStackPanel(innerStackPanel, progress);
                }
                else if (child is TextBlock textBlock)
                {
                    if (textBlock.Text?.Contains("%") == true)
                    {
                        textBlock.Text = $"{progress.CompletionPercentage}%";
                    }
                    else if (textBlock.Text?.Contains("урок") == true)
                    {
                        textBlock.Text = $"{progress.CompletedLessons} из {progress.TotalLessons} уроков";
                    }
                    else if (textBlock.Text?.Contains("обучения") == true)
                    {
                        int hours = (int)(progress.TotalTimeMinutes / 60);
                        textBlock.Text = hours > 0 ? $"{hours} часов обучения" : "0 часов обучения";
                    }
                }
                else if (child is Grid grid)
                {
                    foreach (var gridChild in grid.Children)
                    {
                        if (gridChild is Border border)
                        {
                            UpdateProgressInBorder(border, progress);
                        }
                    }
                }
                else if (child is WrapPanel wrapPanel)
                {
                    UpdateProgressInWrapPanel(wrapPanel, progress);
                }
            }
        }

        private void UpdateProgressInBorder(Border border, CourseProgress progress)
        {
            if (border.Child is StackPanel panel)
            {
                UpdateProgressInStackPanel(panel, progress);
            }
            else if (border.Child is Border innerBorder)
            {
                UpdateProgressInBorder(innerBorder, progress);
            }
        }

        private void UpdateProgressInWrapPanel(WrapPanel wrapPanel, CourseProgress progress)
        {
            foreach (var child in wrapPanel.Children)
            {
                if (child is Border border)
                {
                    UpdateProgressInBorder(border, progress);
                }
            }
        }

        private void UpdateAllCoursesProgress()
        {
            string[] courseIds = { "Files", "System", "Office", "Internet", "Custom" };

            foreach (string courseId in courseIds)
            {
                UpdateCourseCardProgress(courseId);
            }
        }

        private void UpdateCourseCardProgress(string courseId)
        {
            var progress = ProgressManager.GetCourseProgress(courseId);

            string progressPercentName = $"{courseId}ProgressPercent";
            string lessonsCountName = $"{courseId}LessonsCount";
            string learningTimeName = $"{courseId}LearningTime";

            UpdateTextBlock(progressPercentName, $"{progress.CompletionPercentage}%");

            if (courseId == "Custom")
            {
                UpdateTextBlock(lessonsCountName, $"{progress.CompletedLessons} созданных уроков");
                UpdateTextBlock(learningTimeName, "Персонализированное обучение");
            }
            else
            {
                UpdateTextBlock(lessonsCountName, $"{progress.CompletedLessons} из {progress.TotalLessons} уроков");
                int hours = (int)(progress.TotalTimeMinutes / 60);
                UpdateTextBlock(learningTimeName, hours > 0 ? $"{hours} часов обучения" : "0 часов обучения");
            }

            UpdateProgressBar(courseId, progress.CompletionPercentage);
        }

        private void UpdateTextBlock(string elementName, string text)
        {
            var textBlock = FindName(elementName) as TextBlock;
            if (textBlock != null)
            {
                textBlock.Text = text;
            }
        }

        private void UpdateProgressBar(string courseId, int percentage)
        {
            string progressBarName = $"{courseId}ProgressBar";
            var progressBar = FindName(progressBarName) as Border;

            if (progressBar != null)
            {
                if (progressBar.Child is Grid grid && grid.Children.Count > 0)
                {
                    if (grid.Children[0] is Border fillBar)
                    {
                        double maxWidth = progressBar.ActualWidth;
                        if (maxWidth > 0)
                        {
                            fillBar.Width = (percentage / 100.0) * maxWidth;
                        }
                    }
                }
            }
        }
    }
}