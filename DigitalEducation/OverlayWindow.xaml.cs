using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Diagnostics;

namespace DigitalEducation
{
    public partial class OverlayWindow : Window
    {
        private int _currentStep = 0;
        private LessonData _currentLesson;
        private int _totalSteps;
        private bool _isCompleted = false;
        private string _lessonId;
        private Stopwatch _lessonTimer;
        private DateTime _lessonStartTime;

        public OverlayWindow(string lessonId)
        {
            InitializeComponent();
            _lessonId = lessonId;

            if (Application.Current.MainWindow != null)
            {
                Application.Current.MainWindow.WindowState = WindowState.Minimized;
            }

            _currentLesson = LessonManager.GetLesson(lessonId);

            if (_currentLesson == null)
            {
                Close();
                return;
            }

            _totalSteps = _currentLesson.Steps?.Count ?? 0;
            this.Title = _currentLesson.Title;
            LessonTitleText.Text = _currentLesson.Title;

            if (_currentLesson.Steps == null || _totalSteps == 0)
            {
                Close();
                return;
            }

            _lessonTimer = new Stopwatch();
            _lessonStartTime = DateTime.Now;

            InitializeUI();
            ShowCurrentStep();

            this.Loaded += (s, e) =>
            {
                var screen = SystemParameters.WorkArea;
                this.Left = screen.Width - this.ActualWidth - 40;
                this.Top = 40;
            };
        }

        private void InitializeUI()
        {
            this.Loaded += Window_Loaded;
            this.KeyDown += OverlayWindow_KeyDown;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                var screen = SystemParameters.WorkArea;
                this.Left = screen.Width - this.ActualWidth - 40;
                this.Top = 40;
            }), System.Windows.Threading.DispatcherPriority.Background);

            this.Focus();
            this.Topmost = true;
            UpdateProgressBar();

            _lessonTimer.Start();
        }

        private void ShowCurrentStep()
        {
            if (_currentLesson == null || _currentStep < 0 || _currentStep >= _totalSteps)
                return;

            var step = _currentLesson.Steps[_currentStep];

            StepTitle.Text = step.Title ?? "Без заголовка";
            StepDescription.Text = step.Description ?? "";
            StepHint.Text = step.Hint ?? "";

            BtnBack.Visibility = _currentStep > 0 ? Visibility.Visible : Visibility.Collapsed;

            var btnNextText = BtnNext.FindName("BtnNextText") as TextBlock;
            if (btnNextText != null)
            {
                btnNextText.Text = _currentStep == _totalSteps - 1 ? "Завершить" : "Далее";
            }

            UpdateProgressBar();
        }

        private void UpdateProgressBar()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (_totalSteps <= 0) return;

                double progress = (double)(_currentStep + 1) / _totalSteps;

                double containerWidth = ProgressBarContainer.ActualWidth;

                if (containerWidth > 0)
                {
                    double fillWidth = progress * containerWidth;

                    var animation = new System.Windows.Media.Animation.DoubleAnimation(
                        fillWidth,
                        TimeSpan.FromMilliseconds(300));

                    ProgressBarFill.BeginAnimation(WidthProperty, animation);
                }

                UpdateProgressText(progress);
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void UpdateProgressText(double progress)
        {
            int percentage = (int)(progress * 100);
            StepProgress.Text = $"Шаг {_currentStep + 1} из {_totalSteps} ({percentage}%)";
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            if (_currentStep < _totalSteps - 1)
            {
                _currentStep++;
                ShowCurrentStep();
            }
            else
            {
                ShowCompletionMessage();
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (_currentStep > 0)
            {
                _currentStep--;
                ShowCurrentStep();
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            CloseLesson(false);
        }

        private void ShowCompletionMessage()
        {
            _isCompleted = true;

            StepHint.Visibility = Visibility.Collapsed;
            BtnBack.Visibility = Visibility.Collapsed;
            BtnNext.Visibility = Visibility.Collapsed;
            ProgressBarSection.Visibility = Visibility.Collapsed;
            StepProgress.Visibility = Visibility.Collapsed;

            StepTitle.Text = "Урок завершен!";
            StepTitle.FontSize = 16;
            StepTitle.HorizontalAlignment = HorizontalAlignment.Center;
            StepTitle.Margin = new Thickness(0, 0, 0, 12);

            StepDescription.Text = !string.IsNullOrEmpty(_currentLesson.CompletionMessage)
                ? _currentLesson.CompletionMessage
                : "Поздравляем! Урок успешно завершен!";
            StepDescription.FontSize = 14;
            StepDescription.TextAlignment = TextAlignment.Center;
            StepDescription.Margin = new Thickness(0, 0, 0, 24);

            var headerGrid = BtnClose.Parent as Grid;
            if (headerGrid != null)
            {
                Grid.SetColumn(BtnClose, 2);
                Grid.SetColumnSpan(BtnClose, 1);
                BtnClose.HorizontalAlignment = HorizontalAlignment.Right;
                BtnClose.VerticalAlignment = VerticalAlignment.Top;
                BtnClose.Margin = new Thickness(0);
            }

            CompleteLesson();
        }

        private void CompleteLesson()
        {
            _lessonTimer.Stop();
            double minutesSpent = _lessonTimer.Elapsed.TotalMinutes;

            string courseId = GetCourseIdFromLesson(_lessonId);

            ProgressManager.SaveLessonCompletion(
                lessonId: _lessonId,
                courseId: courseId,
                timeSpentMinutes: minutesSpent
            );

            UpdateLessonStatus();

            CloseLesson(true);
        }

        private string GetCourseIdFromLesson(string lessonId)
        {
            if (lessonId.StartsWith("FilesLesson")) return "Files";
            if (lessonId.StartsWith("OsLesson")) return "System";
            if (lessonId.StartsWith("OfficeLesson")) return "Office";
            if (lessonId.StartsWith("InternetLesson")) return "Internet";
            return "Other";
        }

        private void UpdateLessonStatus()
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.UpdateLessonCompletion(_lessonId, true);
            }

            var filesPage = Application.Current.MainWindow?.Content as FilesLessonsPage;
            if (filesPage != null)
            {
                filesPage.UpdateLessonStatus(_lessonId, true);
            }
        }

        private void CloseLesson(bool completed)
        {
            if (Application.Current.MainWindow != null)
            {
                Application.Current.MainWindow.WindowState = WindowState.Normal;
            }

            try
            {
                this.DialogResult = completed;
            }
            catch { }

            this.Close();
        }

        private bool IsChildOfLessonContainer(DependencyObject element)
        {
            while (element != null)
            {
                if (element == LessonContainer)
                    return true;

                element = VisualTreeHelper.GetParent(element);
            }
            return false;
        }

        private void OverlayWindow_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Right:
                case Key.Enter:
                case Key.Space:
                    if (_currentStep < _totalSteps - 1)
                    {
                        BtnNext_Click(null, null);
                        e.Handled = true;
                    }
                    else if (_currentStep == _totalSteps - 1)
                    {
                        BtnNext_Click(null, null);
                        e.Handled = true;
                    }
                    break;

                case Key.Left:
                    if (_currentStep > 0)
                    {
                        BtnBack_Click(null, null);
                        e.Handled = true;
                    }
                    break;

                case Key.Escape:
                    CloseLesson(false);
                    e.Handled = true;
                    break;
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var source = e.OriginalSource as DependencyObject;

            if (source != null && !IsChildOfLessonContainer(source))
            {
                CloseLesson(false);
            }
        }

        private void ProgressBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
}