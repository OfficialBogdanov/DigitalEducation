using DigitalEducation.ComputerVision.Services;
using DigitalEducation.Utilities;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DigitalEducation
{
    public partial class OverlayWindow : Window
    {
        private LessonOverlayController _lessonController;
        private LessonVisionController _visionController;
        private IWindowManager _windowManager;
        private bool _isCompleted = false;
        private ComputerVisionService _visionService;
        private readonly ILessonLoader _lessonLoader;
        private readonly ICourseIdResolver _courseIdResolver;
        private readonly IProgressSaver _progressSaver;
        private readonly IErrorPresenter _errorPresenter;
        private readonly IHintRenderer _hintRenderer;
        private readonly IVisionServiceFactory _visionServiceFactory;
        private readonly ILessonLogger _logger;

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOACTIVATE = 0x0010;

        public OverlayWindow(string lessonId) : this(
            lessonId,
            new LessonDataLoader(),
            new LessonCourseIdResolver(),
            new LessonProgressSaver(),
            new ErrorMessagePresenter(Application.Current?.MainWindow),
            new VisionServiceFactory(),
            new WindowsApiWindowManager(),
            new DebugLessonLogger())
        {
        }

        public OverlayWindow(
            string lessonId,
            ILessonLoader lessonLoader,
            ICourseIdResolver courseIdResolver,
            IProgressSaver progressSaver,
            IErrorPresenter errorPresenter,
            IVisionServiceFactory visionServiceFactory,
            IWindowManager windowManager,
            ILessonLogger logger)
        {
            InitializeComponent();

            _lessonLoader = lessonLoader;
            _courseIdResolver = courseIdResolver;
            _progressSaver = progressSaver;
            _errorPresenter = errorPresenter;
            _visionServiceFactory = visionServiceFactory;
            _windowManager = windowManager;
            _logger = logger;

            _visionService = _visionServiceFactory.Create(_logger);
            _hintRenderer = new VisionHintRenderer(_visionService, HintCanvas, OverlayRect, Dispatcher);

            _lessonController = new LessonOverlayController(
                lessonId,
                this,
                id => _lessonLoader.LoadLesson(id),
                (lid, cid, minutes) => _progressSaver.Save(lid, cid, minutes),
                id => _courseIdResolver.GetCourseId(id),
                _logger);

            _visionController = new LessonVisionController(_lessonController, this, _visionService, _logger);

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

            var hwnd = new WindowInteropHelper(this).Handle;
            _windowManager.MinimizeAllWindows(hwnd);

            InitializeUI();
            _lessonController.ShowCurrentStep();
            _visionController.StartVisionChecks();

            AppThemeManager.ThemeChanged += OnThemeChanged;

            this.Loaded += (s, e) =>
            {
                if (BtnClose != null)
                {
                    var image = VisualTreeHelperExtensions.FindVisualChild<Image>(BtnClose);
                    if (image != null)
                    {
                        var iconSource = AppThemeManager.GetIcon("Close");
                        if (iconSource != null)
                            image.Source = iconSource;
                    }
                }
            };
        }

        private void InitializeUI()
        {
            this.Loaded += (s, e) =>
            {
                this.Topmost = true;
                this.Focus();
                _lessonController.StartLessonTimer();
            };

            this.KeyDown += OverlayWindow_KeyDown;
            this.Closed += Window_Closed;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            _windowManager.MinimizeAllWindows(hwnd);

            this.Dispatcher.BeginInvoke((Action)(() =>
            {
                this.Topmost = true;
                this.Focus();
                _lessonController.UpdateProgressBar();
                _lessonController.StartLessonTimer();
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            AppThemeManager.ThemeChanged -= OnThemeChanged;
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
            AppThemeManager.UpdateAllIconsInContainer(this);
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

        public void ShowError(string message)
        {
            _errorPresenter.ShowError(message);
            _logger.LogError(message);
        }

        public async Task ShowHint(string hintTemplateName, double confidence, string hintType = "rectangle")
        {
            await _hintRenderer.ShowHint(hintTemplateName, confidence, hintType);
            BringToTopmost();
        }

        public async Task ClearHintCanvas()
        {
            await _hintRenderer.ClearHint();
        }

        private void BringToTopmost()
        {
            if (!IsLoaded) return;
            var hwnd = new WindowInteropHelper(this).Handle;
            SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
        }
    }
}