using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace DigitalEducation
{
    public class OverlayLessonController
    {
        private readonly string _lessonId;
        private readonly OverlayWindow _window;
        private LessonData _currentLesson;
        private int _currentStep = 0;
        private int _totalSteps = 0;
        private Stopwatch _lessonTimer;
        private DateTime _lessonStartTime;
        private bool _isCompleted = false;

        public event EventHandler StepChanged;
        public event EventHandler LessonCompleted;

        public OverlayLessonController(string lessonId, OverlayWindow window)
        {
            _lessonId = lessonId;
            _window = window;
            _lessonTimer = new Stopwatch();
        }

        public bool Initialize()
        {
            _currentLesson = LoadLessonFromAnySource(_lessonId);
            if (_currentLesson == null)
            {
                ShowErrorMessage($"Урок '{_lessonId}' не найден");
                return false;
            }

            _totalSteps = _currentLesson.Steps?.Count ?? 0;
            if (_totalSteps == 0)
            {
                ShowErrorMessage("В уроке нет шагов");
                return false;
            }

            _window.SetLessonTitle(_currentLesson.Title);
            _lessonStartTime = DateTime.Now;
            return true;
        }

        public void ShowCurrentStep()
        {
            if (_currentLesson == null || _currentStep < 0 || _currentStep >= _totalSteps)
                return;

            var step = _currentLesson.Steps[_currentStep];

            _window.SetStepContent(step.Title, step.Description, step.Hint);

            bool showBack = _currentStep > 0;
            string nextButtonText = _currentStep == _totalSteps - 1 ? "Завершить" : "Далее";
            bool showNext = !step.RequiresVisionValidation ||
                           (string.IsNullOrEmpty(step.VisionTarget) &&
                            string.IsNullOrEmpty(step.VisionTargetFolder));

            _window.SetNavigationButtons(showBack, nextButtonText, showNext);
            UpdateProgressBar();
            StepChanged?.Invoke(this, EventArgs.Empty);
        }

        public void UpdateProgressBar()
        {
            if (_totalSteps <= 0) return;

            double progress = (double)(_currentStep + 1) / _totalSteps;
            int percentage = (int)(progress * 100);
            string progressText = $"Шаг {_currentStep + 1} из {_totalSteps} ({percentage}%)";

            _window.UpdateProgress(progress, progressText);
        }

        public void GoToNextStep()
        {
            if (_currentStep < _totalSteps - 1)
            {
                _currentStep++;
                ShowCurrentStep();
            }
        }

        public void GoToPreviousStep()
        {
            if (_currentStep > 0)
            {
                _currentStep--;
                ShowCurrentStep();
            }
        }

        public bool CanProceedToNextStep()
        {
            if (_currentStep < _totalSteps - 1) return true;
            if (_currentStep == _totalSteps - 1) return true;
            return false;
        }

        public bool CanGoToPreviousStep()
        {
            return _currentStep > 0;
        }

        public bool IsLastStep()
        {
            return _currentStep == _totalSteps - 1;
        }

        public void CompleteLesson()
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
            }

            _isCompleted = true;
            ShowCompletionMessage();
            LessonCompleted?.Invoke(this, EventArgs.Empty);
        }

        public void StartLessonTimer()
        {
            _lessonTimer.Start();
        }

        public LessonStep GetCurrentStep()
        {
            if (_currentLesson == null || _currentStep < 0 || _currentStep >= _totalSteps)
                return null;
            return _currentLesson.Steps[_currentStep];
        }

        public bool RequiresVisionValidation()
        {
            var step = GetCurrentStep();
            return step?.RequiresVisionValidation == true;
        }

        private LessonData LoadLessonFromAnySource(string lessonId)
        {
            var lesson = LessonManager.GetLesson(lessonId);
            if (lesson != null) return lesson;
            return LoadCustomLesson(lessonId);
        }

        private LessonData LoadCustomLesson(string lessonId)
        {
            try
            {
                string projectRoot = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..");
                string customLessonsPath = System.IO.Path.GetFullPath(
                    System.IO.Path.Combine(projectRoot, "Lessons", "CustomLessons"));
                string lessonFilePath = System.IO.Path.Combine(customLessonsPath, $"{lessonId}.json");

                if (!System.IO.File.Exists(lessonFilePath)) return null;

                string jsonContent = System.IO.File.ReadAllText(lessonFilePath, System.Text.Encoding.UTF8);
                var lesson = System.Text.Json.JsonSerializer.Deserialize<LessonData>(jsonContent,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (lesson != null && string.IsNullOrEmpty(lesson.CourseId))
                    lesson.CourseId = "Custom";

                return lesson;
            }
            catch
            {
                return null;
            }
        }

        private string GetCourseIdFromLesson(string lessonId)
        {
            if (lessonId.StartsWith("FilesLesson")) return "Files";
            if (lessonId.StartsWith("OsLesson")) return "System";
            if (lessonId.StartsWith("OfficeLesson")) return "Office";
            if (lessonId.StartsWith("InternetLesson")) return "Internet";
            return "Other";
        }

        private void ShowCompletionMessage()
        {
            string title = "Урок завершен!";
            string message = !string.IsNullOrEmpty(_currentLesson.CompletionMessage)
                ? _currentLesson.CompletionMessage
                : "Поздравляем! Урок успешно завершен!";

            _window.ShowLessonCompletion(title, message);
        }

        private void ShowErrorMessage(string message)
        {
            _window.Dispatcher.Invoke(() =>
            {
                DialogService.ShowErrorDialog(message, Application.Current.MainWindow);
            });
        }
    }
}