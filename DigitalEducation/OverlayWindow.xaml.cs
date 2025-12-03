using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace DigitalEducation
{
    public partial class OverlayWindow : Window
    {
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        private const int SW_MINIMIZE = 6;
        private const int SW_FORCEMINIMIZE = 11;
        private const int SW_SHOWNOACTIVATE = 4;
        private const int HWND_TOPMOST = -1;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint SWP_SHOWWINDOW = 0x0040;

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
                Application.Current.MainWindow.ShowInTaskbar = false;
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
                MinimizeAllWindows();
                this.Topmost = true;
                this.Focus();
                var screen = SystemParameters.WorkArea;
                this.Left = screen.Width - this.ActualWidth - 40;
                this.Top = 40;
            };
        }

        private void MinimizeAllWindows()
        {
            try
            {
                IntPtr desktopHandle = GetDesktopWindow();
                EnumWindows(new EnumWindowsProc(EnumWindowCallback), IntPtr.Zero);
                IntPtr taskbarHandle = FindWindow("Shell_TrayWnd", null);
                if (taskbarHandle != IntPtr.Zero)
                {
                    ShowWindow(taskbarHandle, SW_MINIMIZE);
                }
                IntPtr secondaryTaskbar = FindWindow("NotifyIconOverflowWindow", null);
                if (secondaryTaskbar != IntPtr.Zero)
                {
                    ShowWindow(secondaryTaskbar, SW_MINIMIZE);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сворачивании окон: {ex.Message}");
            }
        }

        private bool EnumWindowCallback(IntPtr hWnd, IntPtr lParam)
        {
            if (hWnd == IntPtr.Zero || hWnd == new System.Windows.Interop.WindowInteropHelper(this).Handle)
                return true;

            if (IsWindowVisible(hWnd))
            {
                StringBuilder sb = new StringBuilder(256);
                GetWindowText(hWnd, sb, sb.Capacity);
                string title = sb.ToString();

                if (!string.IsNullOrEmpty(title) &&
                    !title.Contains("Program Manager") &&
                    !title.Contains("Microsoft Text Input Application"))
                {
                    ShowWindowAsync(hWnd, SW_MINIMIZE);
                }
            }

            return true;
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
                this.Topmost = true;
                var screen = SystemParameters.WorkArea;
                this.Left = screen.Width - this.ActualWidth - 40;
                this.Top = 40;
                this.Focus();
                UpdateProgressBar();
                _lessonTimer.Start();
            }), System.Windows.Threading.DispatcherPriority.Background);
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
                CompleteLesson();
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

            var doneButton = new Button
            {
                Content = "Готово",
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20),
                Style = (Style)FindResource("NavigationButtonStyle")
            };

            doneButton.Click += (s, args) =>
            {
                CloseLesson(true);
            };

            if (MainContent != null && !MainContent.Children.Contains(doneButton))
            {
                MainContent.Children.Add(doneButton);
            }

            var headerGrid = BtnClose.Parent as Grid;
            if (headerGrid != null)
            {
                Grid.SetColumn(BtnClose, 2);
                Grid.SetColumnSpan(BtnClose, 1);
                BtnClose.HorizontalAlignment = HorizontalAlignment.Right;
                BtnClose.VerticalAlignment = VerticalAlignment.Top;
                BtnClose.Margin = new Thickness(0);
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var source = e.OriginalSource as DependencyObject;

            if (source != null && !IsChildOfLessonContainer(source))
            {
                if (_isCompleted)
                {
                    CloseLesson(true);
                }
                else
                {
                    CloseLesson(false);
                }
            }
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
            Dispatcher.BeginInvoke(new Action(() =>
            {
                ShowCompletionMessage();
            }), System.Windows.Threading.DispatcherPriority.Background);
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
        }

        private void CloseLesson(bool completed)
        {
            if (Application.Current.MainWindow != null)
            {
                Application.Current.MainWindow.WindowState = WindowState.Normal;
                Application.Current.MainWindow.ShowInTaskbar = true;
                Application.Current.MainWindow.Visibility = Visibility.Visible;
                Application.Current.MainWindow.Activate();
            }

            RestoreWindows();

            try
            {
                this.DialogResult = completed;
            }
            catch { }

            this.Close();
        }

        private void RestoreWindows()
        {
            try
            {
                IntPtr taskbarHandle = FindWindow("Shell_TrayWnd", null);
                if (taskbarHandle != IntPtr.Zero)
                {
                    ShowWindow(taskbarHandle, 1);
                }
                IntPtr secondaryTaskbar = FindWindow("NotifyIconOverflowWindow", null);
                if (secondaryTaskbar != IntPtr.Zero)
                {
                    ShowWindow(secondaryTaskbar, 1);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при восстановлении окон: {ex.Message}");
            }
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

        private void ProgressBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
}