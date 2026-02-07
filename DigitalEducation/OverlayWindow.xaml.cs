using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using DigitalEducation.ComputerVision.Services;
using System.Text.Json;

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

        private VisionService _visionService;
        private DispatcherTimer _visionCheckTimer;
        private DispatcherTimer _hintTimer;
        private bool _isVisionChecking = false;

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

            _currentLesson = LoadLessonFromAnySource(lessonId);

            if (_currentLesson == null)
            {
                ShowErrorMessage($"Урок '{lessonId}' не найден");
                this.DialogResult = false;
                this.Close();
                return;
            }

            InitializeVisionService();
            _hintTimer = new DispatcherTimer();

            if (Application.Current.MainWindow != null)
            {
                Application.Current.MainWindow.WindowState = WindowState.Minimized;
                Application.Current.MainWindow.ShowInTaskbar = false;
            }

            _totalSteps = _currentLesson.Steps?.Count ?? 0;
            this.Title = _currentLesson.Title;
            LessonTitleText.Text = _currentLesson.Title;

            if (_currentLesson.Steps == null || _totalSteps == 0)
            {
                ShowErrorMessage("В уроке нет шагов");
                this.DialogResult = false;
                this.Close();
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

            ThemeManager.ThemeChanged += OnThemeChanged;
            this.Loaded += (s, e) => UpdateIcons();
        }

        private LessonData LoadLessonFromAnySource(string lessonId)
        {
            var lesson = LessonManager.GetLesson(lessonId);
            if (lesson != null)
                return lesson;

            return LoadCustomLesson(lessonId);
        }

        private LessonData LoadCustomLesson(string lessonId)
        {
            try
            {
                string projectRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..");
                string customLessonsPath = Path.GetFullPath(Path.Combine(projectRoot, "Lessons", "CustomLessons"));
                string lessonFilePath = Path.Combine(customLessonsPath, $"{lessonId}.json");

                if (!File.Exists(lessonFilePath))
                {
                    Console.WriteLine($"Файл кастомного урока не найден: {lessonFilePath}");
                    return null;
                }

                string jsonContent = File.ReadAllText(lessonFilePath, Encoding.UTF8);
                var lesson = JsonSerializer.Deserialize<LessonData>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (lesson != null)
                {
                    if (string.IsNullOrEmpty(lesson.CourseId))
                        lesson.CourseId = "Custom";

                    Console.WriteLine($"Загружен кастомный урок: {lesson.Title}");
                }

                return lesson;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки кастомного урока {lessonId}: {ex.Message}");
                return null;
            }
        }

        private void InitializeVisionService()
        {
            try
            {
                string templatesPath = GetTemplatesPath();

                Console.WriteLine($"Путь к шаблонам: {templatesPath}");

                if (!Directory.Exists(templatesPath))
                {
                    Directory.CreateDirectory(templatesPath);
                    Console.WriteLine("Создана папка Templates");
                }

                var files = Directory.GetFiles(templatesPath, "*.png");
                Console.WriteLine($"Найдено {files.Length} PNG файлов в Templates:");
                foreach (var file in files)
                {
                    Console.WriteLine($"  - {Path.GetFileName(file)}");
                }

                _visionService = new VisionService(templatesPath);

                _visionCheckTimer = new DispatcherTimer();
                _visionCheckTimer.Interval = TimeSpan.FromMilliseconds(500);
                _visionCheckTimer.Tick += async (s, e) => await CheckVisionStepAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Не удалось инициализировать VisionService: {ex.Message}");
            }
        }

        private string GetTemplatesPath()
        {
            string projectRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..");
            string templatesPath = Path.GetFullPath(Path.Combine(projectRoot, "ComputerVision", "Templates"));

            Console.WriteLine($"=== ПУТЬ К ШАБЛОНАМ ===");
            Console.WriteLine($"BaseDirectory: {AppDomain.CurrentDomain.BaseDirectory}");
            Console.WriteLine($"ProjectRoot: {projectRoot}");
            Console.WriteLine($"TemplatesPath: {templatesPath}");
            Console.WriteLine($"Директория существует: {Directory.Exists(templatesPath)}");

            if (Directory.Exists(templatesPath))
            {
                var files = Directory.GetFiles(templatesPath, "*.png");
                Console.WriteLine($"Найдено файлов: {files.Length}");
                foreach (var file in files)
                {
                    Console.WriteLine($"  - {Path.GetFileName(file)}");
                }
            }

            return templatesPath;
        }

        private async Task ShowHintAsync(LessonStep step)
        {
            if (_visionService == null || string.IsNullOrEmpty(step.VisionHint))
            {
                Console.WriteLine($"Не могу показать подсказку: VisionService={_visionService}, VisionHint={step.VisionHint}");
                return;
            }

            try
            {
                Console.WriteLine($"=== ПОИСК ПОДСКАЗКИ ===");
                Console.WriteLine($"Элемент: {step.VisionHint}");
                Console.WriteLine($"Confidence: {step.HintConfidence}");

                var hintResult = await _visionService.FindElementAsync(
                    step.VisionHint,
                    step.HintConfidence
                );

                Console.WriteLine($"Найден: {hintResult.IsDetected}");
                Console.WriteLine($"Уверенность: {hintResult.Confidence}");
                Console.WriteLine($"Координаты: X={hintResult.Location.X}, Y={hintResult.Location.Y}");
                Console.WriteLine($"Размер: {hintResult.Size.Width}x{hintResult.Size.Height}");

                if (hintResult.IsDetected)
                {
                    Dispatcher.Invoke(() =>
                    {
                        Console.WriteLine($"Создаем прямоугольник подсветки...");

                        var screenBounds = new System.Windows.Rect(
                            hintResult.Location.X,
                            hintResult.Location.Y,
                            hintResult.Size.Width,
                            hintResult.Size.Height
                        );

                        Console.WriteLine($"Прямоугольник: X={screenBounds.X}, Y={screenBounds.Y}, W={screenBounds.Width}, H={screenBounds.Height}");

                        var hintRect = new System.Windows.Shapes.Rectangle
                        {
                            Width = screenBounds.Width,
                            Height = screenBounds.Height,
                            Stroke = Brushes.DodgerBlue,
                            StrokeThickness = 3,
                            Fill = Brushes.Transparent,
                            StrokeDashArray = new DoubleCollection(new double[] { 4, 4 })
                        };

                        Canvas.SetLeft(hintRect, screenBounds.X);
                        Canvas.SetTop(hintRect, screenBounds.Y);

                        Console.WriteLine($"Добавляем на Canvas...");
                        Console.WriteLine($"Детей в HintCanvas до: {HintCanvas.Children.Count}");

                        HintCanvas.Children.Clear();
                        HintCanvas.Children.Add(hintRect);

                        Console.WriteLine($"Детей в HintCanvas после: {HintCanvas.Children.Count}");
                        Console.WriteLine("=== ПОДСКАЗКА ОТОБРАЖЕНА ===");
                    });
                }
                else
                {
                    Console.WriteLine($"Подсказка не найдена на экране");
                    Console.WriteLine($"Проверьте: 1) Файл {step.VisionHint}.png в папке Templates");
                    Console.WriteLine($"           2) Confidence threshold может быть слишком высоким");
                    Console.WriteLine($"           3) Элемент виден на экране прямо сейчас");
                    Console.WriteLine("=== ПОДСКАЗКА НЕ НАЙДЕНА ===");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ОШИБКА при показе подсказки: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
            }
        }

        private void ClearHint()
        {
            Console.WriteLine($"Очищаем подсказку. Детей в HintCanvas: {HintCanvas.Children.Count}");
            HintCanvas.Children.Clear();
            _hintTimer?.Stop();
        }

        private async Task CheckVisionStepAsync()
        {
            if (_isVisionChecking || _currentLesson == null || _currentStep < 0 || _currentStep >= _totalSteps)
                return;

            var currentStepData = _currentLesson.Steps[_currentStep];

            bool hasVisionValidation = currentStepData.RequiresVisionValidation &&
                (!string.IsNullOrEmpty(currentStepData.VisionTarget) ||
                 !string.IsNullOrEmpty(currentStepData.VisionTargetFolder));

            if (!hasVisionValidation)
            {
                return;
            }

            _isVisionChecking = true;

            try
            {
                bool isDetected = false;

                if (!string.IsNullOrEmpty(currentStepData.VisionTargetFolder))
                {
                    Console.WriteLine($"=== ПРОВЕРКА ПАПКИ ===");
                    Console.WriteLine($"Папка: {currentStepData.VisionTargetFolder}");
                    Console.WriteLine($"Требуется элементов: {currentStepData.RequiredMatches}");

                    isDetected = await _visionService.ValidateFolderElementsAsync(
                        currentStepData.VisionTargetFolder,
                        currentStepData.RequiredMatches,
                        currentStepData.VisionConfidence
                    );

                    Console.WriteLine($"Результат: {isDetected}");
                    Console.WriteLine($"====================");
                }
                else if (!string.IsNullOrEmpty(currentStepData.VisionTarget))
                {
                    Console.WriteLine($"Ищем элемент: {currentStepData.VisionTarget}");

                    var result = await _visionService.FindElementAsync(
                        currentStepData.VisionTarget,
                        currentStepData.VisionConfidence
                    );

                    Console.WriteLine($"Результат: Найден={result.IsDetected}, Уверенность={result.Confidence}");
                    isDetected = result.IsDetected;
                }

                if (isDetected)
                {
                    await Dispatcher.Invoke(async () =>
                    {
                        _visionCheckTimer?.Stop();
                        await Task.Delay(500);
                        AutoProceedToNextStep();
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка проверки Vision: {ex.Message}");
            }
            finally
            {
                _isVisionChecking = false;
            }
        }

        private void AutoProceedToNextStep()
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

        private void Window_Closed(object sender, EventArgs e)
        {
            ThemeManager.ThemeChanged -= OnThemeChanged;
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

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            ClearHint();

            var currentStepData = _currentLesson?.Steps[_currentStep];
            if (currentStepData?.RequiresVisionValidation == true &&
                !string.IsNullOrEmpty(currentStepData.VisionTarget))
            {
                return;
            }

            _visionCheckTimer?.Stop();

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
            ClearHint();
            _visionCheckTimer?.Stop();

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

        private void ProgressBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void OnThemeChanged(object sender, string themeName)
        {
            UpdateIcons();
        }

        private void UpdateIcons()
        {
            ThemeManager.UpdateAllIconsInContainer(this);
        }

        private void InitializeUI()
        {
            this.Loaded += Window_Loaded;
            this.KeyDown += OverlayWindow_KeyDown;
        }

        private async void ShowCurrentStep()
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

            Console.WriteLine($"\n=== ШАГ {_currentStep + 1} ===");
            Console.WriteLine($"Заголовок: {step.Title}");
            Console.WriteLine($"VisionTarget: {step.VisionTarget}");
            Console.WriteLine($"VisionTargetFolder: {step.VisionTargetFolder}");
            Console.WriteLine($"RequiredMatches: {step.RequiredMatches}");
            Console.WriteLine($"VisionHint: {step.VisionHint}");
            Console.WriteLine($"ShowHint: {step.ShowHint}");

            ClearHint();

            if (step.ShowHint && !string.IsNullOrEmpty(step.VisionHint))
            {
                Console.WriteLine($"Показываем подсказку...");
                await ShowHintAsync(step);
            }

            if (step.RequiresVisionValidation &&
                (!string.IsNullOrEmpty(step.VisionTarget) || !string.IsNullOrEmpty(step.VisionTargetFolder)))
            {
                Console.WriteLine($"Включена валидация Vision");
                BtnNext.Visibility = Visibility.Collapsed;
                _visionCheckTimer?.Start();
            }
            else
            {
                Console.WriteLine($"Валидация не требуется");
                BtnNext.Visibility = Visibility.Visible;
                BtnNext.IsEnabled = true;
                _visionCheckTimer?.Stop();
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

        private void CompleteLesson()
        {
            _lessonTimer.Stop();
            double minutesSpent = _lessonTimer.Elapsed.TotalMinutes;

            if (!_currentLesson.CourseId.Equals("Custom", StringComparison.OrdinalIgnoreCase))
            {
                string courseId = GetCourseIdFromLesson(_lessonId);
                ProgressManager.SaveLessonCompletion(
                    lessonId: _lessonId,
                    courseId: courseId,
                    timeSpentMinutes: minutesSpent
                );

                UpdateLessonStatus();
            }

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
            }
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

        private void CloseLesson(bool completed)
        {
            ClearHint();
            _hintTimer?.Stop();
            _visionCheckTimer?.Stop();
            _visionService?.Dispose();
            _visionService = null;

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

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _visionService?.Dispose();
            _visionCheckTimer?.Stop();
            _hintTimer?.Stop();
            _hintTimer = null;
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

        private void ShowErrorMessage(string message)
        {
            Dispatcher.Invoke(() =>
            {
                DialogService.ShowErrorDialog(
                    message,
                    Application.Current.MainWindow
                );
            });
        }
    }
}