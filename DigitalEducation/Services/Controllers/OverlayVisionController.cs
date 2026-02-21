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
        private readonly VisionService _visionService;
        private DispatcherTimer _visionCheckTimer;
        private bool _isVisionChecking = false;

        public OverlayVisionController(OverlayLessonController lessonController, OverlayWindow window, VisionService visionService)
        {
            _lessonController = lessonController;
            _window = window;
            _visionService = visionService;
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
                _window.ShowHint(currentStep.VisionHint, currentStep.HintConfidence);
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
            _visionService?.Dispose();
            if (_lessonController != null)
            {
                _lessonController.StepChanged -= OnStepChanged;
            }
        }
    }
}