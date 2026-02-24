using System;
using System.Threading.Tasks;
using System.Windows.Threading;
using DigitalEducation.ComputerVision.Services;

namespace DigitalEducation
{
    public class LessonVisionController : IDisposable
    {
        private readonly LessonOverlayController _lessonController;
        private readonly OverlayWindow _window;
        private readonly ComputerVisionService _visionService;
        private readonly ILessonLogger _logger;
        private DispatcherTimer _visionCheckTimer;
        private bool _isVisionChecking = false;

        public LessonVisionController(
            LessonOverlayController lessonController,
            OverlayWindow window,
            ComputerVisionService visionService,
            ILessonLogger logger)
        {
            _lessonController = lessonController;
            _window = window;
            _visionService = visionService;
            _logger = logger;
            _lessonController.StepChanged += OnStepChanged;
        }

        public void StartVisionChecks()
        {
            if (_visionService == null) return;

            var currentStep = _lessonController.GetCurrentStep();
            if (currentStep == null) return;

            if (currentStep.RequiresVisionValidation &&
                (!string.IsNullOrEmpty(currentStep.VisionTarget) ||
                 !string.IsNullOrEmpty(currentStep.VisionTargetFolder)))
            {
                if (_visionCheckTimer == null)
                {
                    _visionCheckTimer = new DispatcherTimer();
                    _visionCheckTimer.Interval = TimeSpan.FromMilliseconds(500);
                    _visionCheckTimer.Tick += async (s, e) => await CheckVisionStepAsync();
                }
                _visionCheckTimer.Start();
            }

            if (currentStep.ShowHint && !string.IsNullOrEmpty(currentStep.VisionHint))
            {
                _window.ShowHint(currentStep.VisionHint, currentStep.HintConfidence, currentStep.HintType);
            }
            else if (currentStep.ShowHint && currentStep.HintType == "dim")
            {
                _window.ShowHint(null, 0, "dim");
            }
        }

        public void StopVisionCheck()
        {
            _visionCheckTimer?.Stop();
        }

        public async Task ClearHint()
        {
            await _window.ClearHintCanvas();
        }

        private void OnStepChanged(object sender, EventArgs e)
        {
            StopVisionCheck();
            ClearHint().ConfigureAwait(false);
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
                string target = null;

                if (!string.IsNullOrEmpty(currentStep.VisionTargetFolder))
                {
                    target = currentStep.VisionTargetFolder;
                    _logger.LogVisionCheckStarted(target);
                    isDetected = await _visionService.ValidateFolderElementsAsync(
                        currentStep.VisionTargetFolder,
                        currentStep.RequiredMatches,
                        currentStep.VisionConfidence
                    );
                    _logger.LogVisionFolderCheck(target, currentStep.RequiredMatches, isDetected ? 1 : 0, isDetected);
                }
                else if (!string.IsNullOrEmpty(currentStep.VisionTarget))
                {
                    target = currentStep.VisionTarget;
                    _logger.LogVisionCheckStarted(target);
                    var result = await _visionService.FindElementAsync(
                        currentStep.VisionTarget,
                        currentStep.VisionConfidence
                    );
                    isDetected = result.IsDetected;
                    if (isDetected)
                        _logger.LogVisionCheckSucceeded(target, result.Confidence);
                    else
                        _logger.LogVisionCheckFailed(target, currentStep.VisionConfidence);
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
            catch (Exception ex)
            {
                _logger.LogError("Ошибка при проверке компьютерного зрения", ex);
            }
            finally
            {
                _isVisionChecking = false;
            }
        }

        public void Dispose()
        {
            _visionCheckTimer?.Stop();
            _visionService?.Dispose();
            if (_lessonController != null)
            {
                _lessonController.StepChanged -= OnStepChanged;
            }
        }
    }
}