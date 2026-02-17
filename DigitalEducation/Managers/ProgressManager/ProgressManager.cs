using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DigitalEducation
{
    public static class ProgressManager
    {
        private static UserProgress _currentProgress;
        private static IProgressRepository _repository;
        private static IStatisticsCalculator _statisticsCalculator;
        private static readonly object _lock = new object();

        public static event EventHandler ProgressChanged;

        public static IProgressRepository Repository
        {
            get
            {
                if (_repository == null)
                    _repository = new FileSystemProgressRepository();
                return _repository;
            }
            set => _repository = value ?? throw new ArgumentNullException(nameof(value));
        }

        public static IStatisticsCalculator StatisticsCalculator
        {
            get
            {
                if (_statisticsCalculator == null)
                    _statisticsCalculator = new DefaultStatisticsCalculator();
                return _statisticsCalculator;
            }
            set => _statisticsCalculator = value ?? throw new ArgumentNullException(nameof(value));
        }

        static ProgressManager()
        {
            try
            {
                LoadProgress();
                EnsureAllCourseProgressExists();
                UpdateAllCourseTotalLessons();
            }
            catch (Exception ex)
            {
                LogError($"Критическая ошибка инициализации ProgressManager: {ex}");
                _currentProgress = CreateDefaultProgress();
            }
        }

        private static void LogError(string message)
        {
            Debug.WriteLine($"[ProgressManager] {message}");
        }

        private static UserProgress CreateDefaultProgress()
        {
            return new UserProgress
            {
                LastUpdated = DateTime.UtcNow,
                CompletedLessons = new List<CompletedLesson>(),
                Statistics = new UserStatistics(),
                CourseProgress = new Dictionary<string, CourseProgress>()
            };
        }

        public static void LoadProgress()
        {
            lock (_lock)
            {
                try
                {
                    _currentProgress = Repository.Load();
                }
                catch (Exception ex)
                {
                    LogError($"Ошибка загрузки прогресса: {ex}");
                    _currentProgress = CreateDefaultProgress();
                }
            }
        }

        public static void SaveProgress()
        {
            lock (_lock)
            {
                try
                {
                    Repository.Save(_currentProgress);
                    OnProgressChanged();
                }
                catch (Exception ex)
                {
                    LogError($"Ошибка сохранения прогресса: {ex}");
                }
            }
        }

        public static void SaveLessonCompletion(string lessonId, string courseId, double timeSpentMinutes)
        {
            lock (_lock)
            {
                try
                {
                    var completedLesson = new CompletedLesson
                    {
                        LessonId = lessonId,
                        CourseId = courseId,
                        CompletionDate = DateTime.UtcNow,
                        TimeSpentMinutes = timeSpentMinutes
                    };

                    bool isNewLesson = true;
                    for (int i = 0; i < _currentProgress.CompletedLessons.Count; i++)
                    {
                        if (_currentProgress.CompletedLessons[i].LessonId == lessonId)
                        {
                            _currentProgress.CompletedLessons[i] = completedLesson;
                            isNewLesson = false;
                            break;
                        }
                    }

                    if (isNewLesson)
                    {
                        _currentProgress.CompletedLessons.Add(completedLesson);
                    }

                    int totalLessonsInCourse = LessonManager.GetTotalLessonsInCourse(courseId);
                    StatisticsCalculator.EnsureCourseProgressExists(_currentProgress, courseId, totalLessonsInCourse);
                    StatisticsCalculator.UpdateCourseProgress(_currentProgress, courseId, totalLessonsInCourse);
                    StatisticsCalculator.UpdateStatistics(_currentProgress);
                    StatisticsCalculator.UpdateDaysInARow(_currentProgress);

                    SaveProgress();
                }
                catch (Exception ex)
                {
                    LogError($"Ошибка при сохранении завершения урока {lessonId}: {ex}");
                }
            }
        }

        public static bool IsLessonCompleted(string lessonId)
        {
            lock (_lock)
            {
                foreach (var lesson in _currentProgress.CompletedLessons)
                {
                    if (lesson.LessonId == lessonId)
                        return true;
                }
                return false;
            }
        }

        public static CompletedLesson GetLessonCompletion(string lessonId)
        {
            lock (_lock)
            {
                foreach (var lesson in _currentProgress.CompletedLessons)
                {
                    if (lesson.LessonId == lessonId)
                        return lesson;
                }
                return null;
            }
        }

        public static UserStatistics GetStatistics()
        {
            lock (_lock)
            {
                return _currentProgress.Statistics;
            }
        }

        public static CourseProgress GetCourseProgress(string courseId)
        {
            lock (_lock)
            {
                if (!_currentProgress.CourseProgress.TryGetValue(courseId, out var progress))
                {
                    int totalLessons = LessonManager.GetTotalLessonsInCourse(courseId);
                    progress = new CourseProgress { TotalLessons = totalLessons };
                    _currentProgress.CourseProgress[courseId] = progress;
                }
                else
                {
                    int totalLessons = LessonManager.GetTotalLessonsInCourse(courseId);
                    if (progress.TotalLessons != totalLessons)
                    {
                        progress.TotalLessons = totalLessons;
                    }
                }
                return progress;
            }
        }

        public static List<string> GetCompletedLessonsForCourse(string courseId)
        {
            lock (_lock)
            {
                var result = new List<string>();
                foreach (var lesson in _currentProgress.CompletedLessons)
                {
                    if (lesson.CourseId == courseId)
                        result.Add(lesson.LessonId);
                }
                return result;
            }
        }

        public static void ResetProgress()
        {
            lock (_lock)
            {
                _currentProgress = CreateDefaultProgress();
                EnsureAllCourseProgressExists();
                UpdateAllCourseTotalLessons();
                SaveProgress();
            }
        }

        public static string GetProgressFilePath()
        {
            return Repository.GetFilePath();
        }

        private static void OnProgressChanged()
        {
            ProgressChanged?.Invoke(null, EventArgs.Empty);
        }

        private static void EnsureAllCourseProgressExists()
        {
            var courseCounts = LessonManager.GetCourseLessonCounts();
            foreach (var kv in courseCounts)
            {
                StatisticsCalculator.EnsureCourseProgressExists(_currentProgress, kv.Key, kv.Value);
            }
        }

        private static void UpdateAllCourseTotalLessons()
        {
            var courseCounts = LessonManager.GetCourseLessonCounts();
            foreach (var kv in courseCounts)
            {
                if (_currentProgress.CourseProgress.TryGetValue(kv.Key, out var progress))
                {
                    progress.TotalLessons = kv.Value;
                }
                else
                {
                    _currentProgress.CourseProgress[kv.Key] = new CourseProgress
                    {
                        TotalLessons = kv.Value
                    };
                }
            }
        }
    }
}