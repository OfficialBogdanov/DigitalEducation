using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DigitalEducation
{
    public static class LessonManager
    {
        private static readonly Dictionary<string, LessonData> _lessons = new Dictionary<string, LessonData>();
        private static ICustomLessonRepository _repository;

        public static ICustomLessonRepository Repository
        {
            get
            {
                if (_repository == null)
                    _repository = new CustomLessonRepository();
                return _repository;
            }
            set => _repository = value ?? throw new ArgumentNullException(nameof(value));
        }

        static LessonManager()
        {
            try
            {
                LoadAllLessons();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LessonManager] Ошибка инициализации: {ex.Message}");
            }
        }

        public static LessonData GetLesson(string lessonId)
        {
            _lessons.TryGetValue(lessonId, out var lesson);
            return lesson;
        }

        public static bool LessonExists(string lessonId)
        {
            return _lessons.ContainsKey(lessonId);
        }

        public static List<LessonData> GetAllLessons()
        {
            return new List<LessonData>(_lessons.Values);
        }

        public static List<LessonData> GetLessonsByCategory(string categoryPrefix)
        {
            var result = new List<LessonData>();
            foreach (var lesson in _lessons.Values)
            {
                if (lesson.Id.StartsWith(categoryPrefix))
                    result.Add(lesson);
            }
            return result;
        }

        public static List<LessonData> GetLessonsByCourse(string courseId)
        {
            var result = new List<LessonData>();
            foreach (var lesson in _lessons.Values)
            {
                if (lesson.CourseId == courseId)
                    result.Add(lesson);
            }
            return result;
        }

        public static int GetTotalLessonsInCourse(string courseId)
        {
            int count = 0;
            foreach (var lesson in _lessons.Values)
            {
                if (lesson.CourseId == courseId)
                    count++;
            }
            return count;
        }

        public static Dictionary<string, int> GetCourseLessonCounts()
        {
            var counts = new Dictionary<string, int>();
            foreach (var lesson in _lessons.Values)
            {
                if (counts.ContainsKey(lesson.CourseId))
                    counts[lesson.CourseId]++;
                else
                    counts[lesson.CourseId] = 1;
            }
            return counts;
        }

        public static void UpdateCourseProgressInManager()
        {
            try
            {
                var courseCounts = GetCourseLessonCounts();
                foreach (var course in courseCounts)
                {
                    var progress = ProgressManager.GetCourseProgress(course.Key);
                    if (progress != null)
                    {
                        progress.TotalLessons = course.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LessonManager] Ошибка обновления прогресса: {ex.Message}");
            }
        }

        private static void LoadAllLessons()
        {
            _lessons.Clear();
            var loadedLessons = Repository.LoadAllLessons();
            foreach (var lesson in loadedLessons)
            {
                _lessons[lesson.Id] = lesson;
            }
        }

        public static void ReloadAllLessons()
        {
            LoadAllLessons();
        }
    }
}