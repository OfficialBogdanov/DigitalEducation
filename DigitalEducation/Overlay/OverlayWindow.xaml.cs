using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DigitalEducation
{
    public partial class OverlayWindow : Window
    {
        private OverlayLessonController _lessonController;
        private OverlayVisionController _visionController;
        private OverlayWindowManager _windowManager;
        private bool _isCompleted = false;


        public OverlayWindow(string lessonId)
        {
            InitializeComponent();

            _lessonController = new OverlayLessonController(lessonId, this);
            _visionController = new OverlayVisionController(_lessonController, this);
            _windowManager = new OverlayWindowManager();

            if (!_lessonController.Initialize())
            {
                this.DialogResult = false;
                this.Close();
                return;
            }

            if (Application.Current.MainWindow != null)
            {
                Application.Current.MainWindow.WindowState = WindowState.Minimized;
                Application.Current.MainWindow.ShowInTaskbar = false;
                Application.Current.MainWindow.Visibility = Visibility.Collapsed;
            }

            var hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            _windowManager.MinimizeAllWindows(hwnd);

            InitializeUI();
            _lessonController.ShowCurrentStep();
            _visionController.StartVisionChecks();

            ThemeManager.ThemeChanged += OnThemeChanged;

            this.Loaded += (s, e) =>
            {
                // Находим кнопку закрытия
                if (BtnClose != null)
                {
                    // Ищем Image внутри кнопки
                    var image = FindVisualChild<Image>(BtnClose);
                    if (image != null)
                    {
                        // Вручную загружаем иконку
                        var iconSource = ThemeManager.GetIcon("Close");
                        if (iconSource != null)
                        {
                            image.Source = iconSource;
                        }
                    }
                }
            };
        }

        private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T result)
                    return result;

                var childResult = FindVisualChild<T>(child);
                if (childResult != null)
                    return childResult;
            }

            return null;
        }

        private void InitializeUI()
        {
            this.Loaded += (s, e) =>
            {
                this.Topmost = true;
                this.Focus();
                var screen = SystemParameters.WorkArea;
                this.Left = screen.Width - this.ActualWidth - 40;
                this.Top = 40;
                _lessonController.StartLessonTimer();
            };

            this.KeyDown += OverlayWindow_KeyDown;
            this.Closed += Window_Closed;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            _windowManager.MinimizeAllWindows(hwnd);

            this.Dispatcher.BeginInvoke((Action)(() =>
            {
                this.Topmost = true;
                var screen = SystemParameters.WorkArea;
                this.Left = screen.Width - this.ActualWidth - 40;
                this.Top = 40;
                this.Focus();
                _lessonController.UpdateProgressBar();
                _lessonController.StartLessonTimer();
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            ThemeManager.ThemeChanged -= OnThemeChanged;
            _visionController.Dispose();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var source = e.OriginalSource as DependencyObject;

            if (source != null && !IsChildOfLessonContainer(source))
            {
                CloseLesson();
            }
        }

        private void OverlayWindow_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Right:
                case Key.Enter:
                case Key.Space:
                    if (_lessonController.CanProceedToNextStep())
                    {
                        BtnNext_Click(null, null);
                        e.Handled = true;
                    }
                    break;

                case Key.Left:
                    if (_lessonController.CanGoToPreviousStep())
                    {
                        BtnBack_Click(null, null);
                        e.Handled = true;
                    }
                    break;

                case Key.Escape:
                    CloseLesson();
                    e.Handled = true;
                    break;
            }
        }

        private async void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            await _visionController.ClearHint();
            _visionController.StopVisionCheck();

            if (_lessonController.IsLastStep())
            {
                _lessonController.CompleteLesson();
                _isCompleted = true;
            }
            else
            {
                _lessonController.GoToNextStep();
                _visionController.StartVisionChecks();
            }
        }

        private async void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            await _visionController.ClearHint();
            _visionController.StopVisionCheck();
            _lessonController.GoToPreviousStep();
            _visionController.StartVisionChecks();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            CloseLesson();
        }

        private void OnThemeChanged(object sender, string themeName)
        {
            UpdateIcons();
        }

        private void UpdateIcons()
        {
            ThemeManager.UpdateAllIconsInContainer(this);
        }

        private void CloseLesson()
        {
            _visionController.Dispose();
            _windowManager.RestoreWindows();

            if (Application.Current.MainWindow != null)
            {
                Application.Current.MainWindow.WindowState = WindowState.Maximized;
                Application.Current.MainWindow.WindowStyle = WindowStyle.None;
                Application.Current.MainWindow.ResizeMode = ResizeMode.NoResize;
                Application.Current.MainWindow.ShowInTaskbar = true;
                Application.Current.MainWindow.Visibility = Visibility.Visible;
                Application.Current.MainWindow.Activate();
                Application.Current.MainWindow.Topmost = true;
                Application.Current.MainWindow.Topmost = false;
            }

            try
            {
                this.DialogResult = _isCompleted;
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

        public void SetStepContent(string title, string description, string hint)
        {
            StepTitle.Text = title ?? "Без заголовка";
            StepDescription.Text = description ?? "";
            StepHint.Text = hint ?? "";
        }

        public void SetNavigationButtons(bool showBack, string nextButtonText, bool showNext = true)
        {
            BtnBack.Visibility = showBack ? Visibility.Visible : Visibility.Collapsed;
            BtnNext.Visibility = showNext ? Visibility.Visible : Visibility.Collapsed;

            var btnNextText = BtnNext.FindName("BtnNextText") as TextBlock;
            if (btnNextText != null)
            {
                btnNextText.Text = nextButtonText ?? "Далее";
            }
        }

        public void UpdateProgress(double progress, string progressText)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                double containerWidth = ProgressBarContainer.ActualWidth;
                if (containerWidth > 0)
                {
                    double fillWidth = progress * containerWidth;
                    var animation = new System.Windows.Media.Animation.DoubleAnimation(
                        fillWidth,
                        TimeSpan.FromMilliseconds(300));
                    ProgressBarFill.BeginAnimation(WidthProperty, animation);
                }

                StepProgress.Text = progressText;
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        public void ShowLessonCompletion(string title, string message)
        {
            StepHint.Visibility = Visibility.Collapsed;
            BtnBack.Visibility = Visibility.Collapsed;
            BtnNext.Visibility = Visibility.Collapsed;
            ProgressBarSection.Visibility = Visibility.Collapsed;
            StepProgress.Visibility = Visibility.Collapsed;

            StepTitle.Text = title;
            StepTitle.FontSize = 16;
            StepTitle.HorizontalAlignment = HorizontalAlignment.Center;
            StepTitle.Margin = new Thickness(0, 0, 0, 12);

            StepDescription.Text = message;
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
                _isCompleted = true;
                CloseLesson();
            };

            if (MainContent != null && !MainContent.Children.Contains(doneButton))
            {
                MainContent.Children.Add(doneButton);
            }
        }

        public void SetLessonTitle(string title)
        {
            this.Title = title;
            LessonTitleText.Text = title;
        }

        public Canvas GetHintCanvas() => HintCanvas;
    }
}