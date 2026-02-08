using System;
using System.Threading.Tasks;
using System.Windows.Threading;
using DigitalEducation.ComputerVision.Services;

namespace DigitalEducation
{
    public class OverlayVisionController : IDisposable
    {
        private readonly OverlayLessonController _lessonController;
        private readonly OverlayWindow _window;
        private VisionService _visionService;
        private DispatcherTimer _visionCheckTimer;
        private DispatcherTimer _hintTimer;
        private bool _isVisionChecking = false;

        public OverlayVisionController(OverlayLessonController lessonController, OverlayWindow window)
        {
            _lessonController = lessonController;
            _window = window;
            _lessonController.StepChanged += OnStepChanged;
        }

        public void StartVisionChecks()
        {
            var currentStep = _lessonController.GetCurrentStep();
            if (currentStep == null) return;

            if (currentStep.RequiresVisionValidation &&
                (!string.IsNullOrEmpty(currentStep.VisionTarget) ||
                 !string.IsNullOrEmpty(currentStep.VisionTargetFolder)))
            {
                InitializeVisionService();
                StartVisionCheckTimer();
            }

            if (currentStep.ShowHint && !string.IsNullOrEmpty(currentStep.VisionHint))
            {
                ShowHintAsync(currentStep).ConfigureAwait(false);
            }
        }

        public void StopVisionCheck()
        {
            _visionCheckTimer?.Stop();
        }

        public async Task ClearHint()
        {
            _hintTimer?.Stop();
            await _window.Dispatcher.InvokeAsync(() =>
            {
                var canvas = _window.GetHintCanvas();
                if (canvas != null)
                {
                    canvas.Children.Clear();
                }
            });
        }

        private void OnStepChanged(object sender, EventArgs e)
        {
            StopVisionCheck();
            ClearHint().ConfigureAwait(false);
        }

        private void InitializeVisionService()
        {
            if (_visionService != null) return;

            try
            {
                string templatesPath = GetTemplatesPath();
                if (!System.IO.Directory.Exists(templatesPath))
                {
                    System.IO.Directory.CreateDirectory(templatesPath);
                }

                _visionService = new VisionService(templatesPath);

                _visionCheckTimer = new DispatcherTimer();
                _visionCheckTimer.Interval = TimeSpan.FromMilliseconds(500);
                _visionCheckTimer.Tick += async (s, e) => await CheckVisionStepAsync();
            }
            catch
            {
            }
        }

        private string GetTemplatesPath()
        {
            string projectRoot = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..");
            return System.IO.Path.GetFullPath(
                System.IO.Path.Combine(projectRoot, "ComputerVision", "Templates"));
        }

        private async Task ShowHintAsync(LessonStep step)
        {
            if (_visionService == null || string.IsNullOrEmpty(step.VisionHint)) return;

            try
            {
                var hintResult = await _visionService.FindElementAsync(
                    step.VisionHint,
                    step.HintConfidence
                );

                if (hintResult.IsDetected)
                {
                    await _window.Dispatcher.InvokeAsync(() =>
                    {
                        var screenBounds = new System.Windows.Rect(
                            hintResult.Location.X,
                            hintResult.Location.Y,
                            hintResult.Size.Width,
                            hintResult.Size.Height
                        );

                        var hintRect = new System.Windows.Shapes.Rectangle
                        {
                            Width = screenBounds.Width,
                            Height = screenBounds.Height,
                            Stroke = System.Windows.Media.Brushes.DodgerBlue,
                            StrokeThickness = 3,
                            Fill = System.Windows.Media.Brushes.Transparent,
                            StrokeDashArray = new System.Windows.Media.DoubleCollection(new double[] { 4, 4 })
                        };

                        var canvas = _window.GetHintCanvas();
                        if (canvas != null)
                        {
                            canvas.Children.Clear();
                            System.Windows.Controls.Canvas.SetLeft(hintRect, screenBounds.X);
                            System.Windows.Controls.Canvas.SetTop(hintRect, screenBounds.Y);
                            canvas.Children.Add(hintRect);
                        }
                    });
                }
            }
            catch
            {
            }
        }

        private void StartVisionCheckTimer()
        {
            _visionCheckTimer?.Start();
        }

        private async Task CheckVisionStepAsync()
        {
            if (_isVisionChecking || _visionService == null) return;

            var currentStep = _lessonController.GetCurrentStep();
            if (currentStep == null) return;

            _isVisionChecking = true;

            try
            {
                bool isDetected = false;

                if (!string.IsNullOrEmpty(currentStep.VisionTargetFolder))
                {
                    isDetected = await _visionService.ValidateFolderElementsAsync(
                        currentStep.VisionTargetFolder,
                        currentStep.RequiredMatches,
                        currentStep.VisionConfidence
                    );
                }
                else if (!string.IsNullOrEmpty(currentStep.VisionTarget))
                {
                    var result = await _visionService.FindElementAsync(
                        currentStep.VisionTarget,
                        currentStep.VisionConfidence
                    );
                    isDetected = result.IsDetected;
                }

                if (isDetected)
                {
                    await _window.Dispatcher.InvokeAsync(() =>
                    {
                        _visionCheckTimer?.Stop();
                        if (_lessonController.IsLastStep())
                        {
                            _lessonController.CompleteLesson();
                        }
                        else
                        {
                            _lessonController.GoToNextStep();
                            StartVisionChecks();
                        }
                    });
                }
            }
            catch
            {
            }
            finally
            {
                _isVisionChecking = false;
            }
        }

        public void Dispose()
        {
            _visionCheckTimer?.Stop();
            _hintTimer?.Stop();
            _visionService?.Dispose();
            _visionService = null;

            if (_lessonController != null)
            {
                _lessonController.StepChanged -= OnStepChanged;
            }
        }
    }
}