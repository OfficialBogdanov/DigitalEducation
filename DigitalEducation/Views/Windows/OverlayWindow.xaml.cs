using DigitalEducation.ComputerVision.Services;
using System;
using System.Threading.Tasks;
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
        private IWindowManager _windowManager;
        private bool _isCompleted = false;
        private VisionService _visionService;

        private readonly ILessonLoader _lessonLoader;
        private readonly ICourseIdResolver _courseIdResolver;
        private readonly IProgressSaver _progressSaver;
        private readonly IErrorPresenter _errorPresenter;
        private readonly IHintRenderer _hintRenderer;
        private readonly IVisionServiceFactory _visionServiceFactory;

        public OverlayWindow(string lessonId) : this(
            lessonId,
            new LessonLoader(),
            new LessonCourseResolver(),
            new ProgressSaver(),
            new ErrorPresenter(Application.Current?.MainWindow),
            new VisionFactory(),
            new WindowsApiWindowManager())
        {
        }

        public OverlayWindow(
            string lessonId,
            ILessonLoader lessonLoader,
            ICourseIdResolver courseIdResolver,
            IProgressSaver progressSaver,
            IErrorPresenter errorPresenter,
            IVisionServiceFactory visionServiceFactory,
            IWindowManager windowManager)
        {
            InitializeComponent();

            _lessonLoader = lessonLoader;
            _courseIdResolver = courseIdResolver;
            _progressSaver = progressSaver;
            _errorPresenter = errorPresenter;
            _visionServiceFactory = visionServiceFactory;
            _windowManager = windowManager;

            _visionService = _visionServiceFactory.Create();
            _hintRenderer = new HintRenderer(_visionService, HintCanvas, Dispatcher);

            _lessonController = new OverlayLessonController(
                lessonId,
                this,
                id => _lessonLoader.LoadLesson(id),
                (lid, cid, minutes) => _progressSaver.Save(lid, cid, minutes),
                id => _courseIdResolver.GetCourseId(id));

            _visionController = new OverlayVisionController(_lessonController, this, _visionService);

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
                if (BtnClose != null)
                {
                    var image = FindVisualChild<Image>(BtnClose);
                    if (image != null)
                    {
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

        public void ShowError(string message)
        {
            _errorPresenter.ShowError(message);
        }

        public async Task ShowHint(string hintTemplateName, double confidence)
        {
            await _hintRenderer.ShowHint(hintTemplateName, confidence);
        }

        public async Task ClearHintCanvas()
        {
            await _hintRenderer.ClearHint();
        }
    }
}