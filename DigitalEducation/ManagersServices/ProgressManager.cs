using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Diagnostics;

namespace DigitalEducation
{
    public static class ProgressManager
    {
        public static event EventHandler ProgressChanged;
        private static UserProgress _currentProgress;
        private static readonly string _progressFilePath;
        private static readonly JsonSerializerOptions _jsonOptions;
        private static readonly object _lock = new object();

        private static void OnProgressChanged()
        {
            ProgressChanged?.Invoke(null, EventArgs.Empty);
        }

        static ProgressManager()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appFolder = Path.Combine(appDataPath, "DigitalEducation");

            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }

            _progressFilePath = Path.Combine(appFolder, "user_progress.json");

            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };

            LoadProgress();
        }

        public static void LoadProgress()
        {
            lock (_lock)
            {
                try
                {
                    if (File.Exists(_progressFilePath))
                    {
                        string json = File.ReadAllText(_progressFilePath);
                        _currentProgress = JsonSerializer.Deserialize<UserProgress>(json, _jsonOptions);
                    }
                    else
                    {
                        _currentProgress = CreateDefaultProgress();
                        SaveProgress();
                    }
                }
                catch
                {
                    _currentProgress = CreateDefaultProgress();
                }
            }
        }

        private static UserProgress CreateDefaultProgress()
        {
            return new UserProgress
            {
                LastUpdated = DateTime.UtcNow,
                CompletedLessons = new List<CompletedLesson>(),
                Statistics = new UserStatistics(),
                CourseProgress = new Dictionary<string, CourseProgress>
                {
                    { "Files", new CourseProgress { TotalLessons = 5 } },
                    { "System", new CourseProgress { TotalLessons = 10 } },
                    { "Office", new CourseProgress { TotalLessons = 8 } },
                    { "Internet", new CourseProgress { TotalLessons = 6 } }
                }
            };
        }

        public static void SaveProgress()
        {
            lock (_lock)
            {
                try
                {
                    _currentProgress.LastUpdated = DateTime.UtcNow;
                    string json = JsonSerializer.Serialize(_currentProgress, _jsonOptions);
                    File.WriteAllText(_progressFilePath, json);
                }
                catch
                {
                }
            }
        }

        public static void SaveLessonCompletion(string lessonId, string courseId, double timeSpentMinutes)
        {
            lock (_lock)
            {
                var completedLesson = new CompletedLesson
                {
                    LessonId = lessonId,
                    CourseId = courseId,
                    CompletionDate = DateTime.UtcNow,
                    TimeSpentMinutes = timeSpentMinutes,
                    Status = "completed"
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

                UpdateStatistics();
                UpdateCourseProgress(courseId);
                UpdateDaysInARow();

                SaveProgress();
                OnProgressChanged();
            }
        }

        private static void UpdateStatistics()
        {
            _currentProgress.Statistics.TotalLessonsCompleted = _currentProgress.CompletedLessons.Count;

            double totalTime = 0;
            foreach (var lesson in _currentProgress.CompletedLessons)
            {
                totalTime += lesson.TimeSpentMinutes;
            }
            _currentProgress.Statistics.TotalTimeSpentMinutes = totalTime;

            _currentProgress.Statistics.TotalCoursesCompleted = 0;
            foreach (var course in _currentProgress.CourseProgress)
            {
                if (course.Value.CompletionPercentage >= 100)
                {
                    _currentProgress.Statistics.TotalCoursesCompleted++;
                }
            }
        }

        private static void UpdateCourseProgress(string courseId)
        {
            if (!_currentProgress.CourseProgress.ContainsKey(courseId))
                return;

            int completedCount = 0;
            double totalTime = 0;

            foreach (var lesson in _currentProgress.CompletedLessons)
            {
                if (lesson.CourseId == courseId)
                {
                    completedCount++;
                    totalTime += lesson.TimeSpentMinutes;
                }
            }

            var course = _currentProgress.CourseProgress[courseId];
            course.CompletedLessons = completedCount;
            course.TotalTimeMinutes = totalTime;

            if (course.TotalLessons > 0)
            {
                course.CompletionPercentage = (int)((double)completedCount / course.TotalLessons * 100);
            }
        }

        private static void UpdateDaysInARow()
        {
            var today = DateTime.UtcNow.Date;
            var lastDate = _currentProgress.Statistics.LastLearningDate?.Date;

            if (lastDate == null)
            {
                _currentProgress.Statistics.DaysInARow = 1;
            }
            else if (lastDate == today.AddDays(-1))
            {
                _currentProgress.Statistics.DaysInARow++;
            }
            else if (lastDate < today.AddDays(-1))
            {
                _currentProgress.Statistics.DaysInARow = 1;
            }

            _currentProgress.Statistics.LastLearningDate = today;
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
                if (_currentProgress.CourseProgress.ContainsKey(courseId))
                    return _currentProgress.CourseProgress[courseId];

                return new CourseProgress();
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
                SaveProgress();
                OnProgressChanged();
            }
        }

        public static string GetProgressFilePath()
        {
            return _progressFilePath;
        }
    }
}