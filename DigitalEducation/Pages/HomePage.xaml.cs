using System;
using System.Windows;
using System.Windows.Controls;

namespace DigitalEducation
{
    public partial class HomePage : UserControl
    {
        public event EventHandler<string> CategoryButtonClicked;

        public HomePage()
        {
            InitializeComponent();
            InitializeEventHandlers();
            this.Unloaded += OnHomePageUnloaded;
            this.Loaded += HomePage_Loaded;
        }

        private void HomePage_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateStatistics();
            UpdateProgress();
        }

        private void InitializeEventHandlers()
        {
            btnFiles.Click += OnCategoryButtonClick;
            btnSystem.Click += OnCategoryButtonClick;
        }

        private void OnCategoryButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                string category = GetCategoryFromButton(button);
                if (!string.IsNullOrEmpty(category))
                {
                    CategoryButtonClicked?.Invoke(this, category);
                }
            }
        }

        private string GetCategoryFromButton(Button button)
        {
            if (button.Tag is string tag && !string.IsNullOrEmpty(tag))
            {
                return tag;
            }

            string buttonName = button.Name;
            if (buttonName.StartsWith("btn"))
            {
                return buttonName.Substring(3);
            }

            return string.Empty;
        }

        private void OnHomePageUnloaded(object sender, RoutedEventArgs e)
        {
            CleanupEventHandlers();
        }

        private void CleanupEventHandlers()
        {
            btnFiles.Click -= OnCategoryButtonClick;
            btnSystem.Click -= OnCategoryButtonClick;

            this.Unloaded -= OnHomePageUnloaded;
            this.Loaded -= HomePage_Loaded;

            if (CategoryButtonClicked != null)
            {
                foreach (Delegate d in CategoryButtonClicked.GetInvocationList())
                {
                    CategoryButtonClicked -= (EventHandler<string>)d;
                }
            }
        }

        public void UpdateStatistics()
        {
            var stats = ProgressManager.GetStatistics();

            Dispatcher.BeginInvoke(new Action(() =>
            {
                UpdateStatCard("TotalCourses", stats.TotalCoursesCompleted);
                UpdateStatCard("LearningHours", (int)(stats.TotalTimeSpentMinutes / 60));
                UpdateStatCard("LessonsCompleted", stats.TotalLessonsCompleted);
                UpdateStatCard("StreakDays", stats.DaysInARow);
            }));
        }

        private void UpdateStatCard(string cardType, int value)
        {
            TextBlock valueText = null;
            TextBlock labelText = null;

            switch (cardType)
            {
                case "TotalCourses":
                    valueText = FindName("TotalCoursesValue") as TextBlock;
                    labelText = FindName("TotalCoursesLabel") as TextBlock;
                    break;
                case "LearningHours":
                    valueText = FindName("LearningHoursValue") as TextBlock;
                    labelText = FindName("LearningHoursLabel") as TextBlock;
                    break;
                case "LessonsCompleted":
                    valueText = FindName("LessonsCompletedValue") as TextBlock;
                    labelText = FindName("LessonsCompletedLabel") as TextBlock;
                    break;
                case "StreakDays":
                    valueText = FindName("StreakDaysValue") as TextBlock;
                    labelText = FindName("StreakDaysLabel") as TextBlock;
                    break;
            }

            if (valueText != null)
            {
                valueText.Text = value.ToString();
            }
        }

        public void UpdateProgress()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                var filesProgress = ProgressManager.GetCourseProgress("Files");
                var systemProgress = ProgressManager.GetCourseProgress("System");
                var officeProgress = ProgressManager.GetCourseProgress("Office");
                var internetProgress = ProgressManager.GetCourseProgress("Internet");

                double overallProgress = CalculateOverallProgress(
                    filesProgress.CompletionPercentage,
                    systemProgress.CompletionPercentage,
                    officeProgress.CompletionPercentage,
                    internetProgress.CompletionPercentage
                );

                UpdateProgressBars(overallProgress,
                    filesProgress.CompletionPercentage,
                    systemProgress.CompletionPercentage,
                    officeProgress.CompletionPercentage,
                    internetProgress.CompletionPercentage);
            }));
        }

        private double CalculateOverallProgress(double files, double system, double office, double internet)
        {
            int totalCourses = 4;
            return (files + system + office + internet) / totalCourses;
        }

        private void UpdateProgressBars(double overallProgress,
                                       double filesProgress,
                                       double systemProgress,
                                       double officeProgress,
                                       double internetProgress)
        {
            if (OverallProgressBar != null)
            {
                overallProgress = Math.Max(0, Math.Min(100, overallProgress));

                var progressBarBorder = OverallProgressBar.Parent as Border;
                if (progressBarBorder != null)
                {
                    double maxWidth = progressBarBorder.ActualWidth;
                    if (maxWidth > 0)
                    {
                        OverallProgressBar.Width = (overallProgress / 100) * maxWidth;
                    }
                }

                UpdateProgressText("OverallProgressText", $"{overallProgress:F0}% завершено");
                UpdateProgressText("FilesProgressText", $"{filesProgress:F0}%");
                UpdateProgressText("SystemProgressText", $"{systemProgress:F0}%");
                UpdateProgressText("OfficeProgressText", $"{officeProgress:F0}%");
                UpdateProgressText("InternetProgressText", $"{internetProgress:F0}%");
            }
        }

        private void UpdateProgressText(string elementName, string text)
        {
            var textBlock = FindName(elementName) as TextBlock;
            if (textBlock != null)
            {
                textBlock.Text = text;
            }
        }

        public double GetCurrentProgress()
        {
            if (OverallProgressBar != null)
            {
                var parent = OverallProgressBar.Parent as Border;
                if (parent != null && parent.ActualWidth > 0)
                {
                    return (OverallProgressBar.Width / parent.ActualWidth) * 100;
                }
            }
            return 0;
        }

        public void ResetProgressToDefault()
        {
            UpdateProgress();
        }

        private void btnFiles_Click(object sender, RoutedEventArgs e)
        {
        }

        public void RefreshAll()
        {
            UpdateStatistics();
            UpdateProgress();
        }
    }
}