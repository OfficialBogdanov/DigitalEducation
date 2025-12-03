using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace DigitalEducation
{
    public static class LessonManager
    {
        private static readonly Dictionary<string, LessonData> _lessons = new Dictionary<string, LessonData>();
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        static LessonManager()
        {
            LoadAllLessons();
        }

        private static void LoadAllLessons()
        {
            LoadLessonsFromCategory("FilesLessons");
            LoadLessonsFromCategory("OsLessons");
        }

        private static void LoadLessonsFromCategory(string category)
        {
            string projectRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..");
            string lessonsPath = Path.GetFullPath(Path.Combine(projectRoot, "Lessons", category));

            if (!Directory.Exists(lessonsPath))
            {
                Directory.CreateDirectory(lessonsPath);
                return;
            }

            var jsonFiles = Directory.GetFiles(lessonsPath, "*.json");

            foreach (var filePath in jsonFiles)
            {
                try
                {
                    string jsonContent = File.ReadAllText(filePath, Encoding.UTF8);
                    var lesson = JsonSerializer.Deserialize<LessonData>(jsonContent, _jsonOptions);

                    if (lesson != null && !string.IsNullOrEmpty(lesson.Id))
                    {
                        if (lesson.Steps == null)
                        {
                            lesson.Steps = new List<LessonStep>();
                        }

                        if (string.IsNullOrEmpty(lesson.CourseId))
                        {
                            lesson.CourseId = GetCourseIdFromLessonId(lesson.Id);
                        }

                        _lessons[lesson.Id] = lesson;
                    }
                }
                catch (JsonException)
                {
                }
                catch
                {
                }
            }
        }

        private static string GetCourseIdFromLessonId(string lessonId)
        {
            if (lessonId.StartsWith("FilesLesson")) return "Files";
            if (lessonId.StartsWith("OsLesson")) return "System";
            if (lessonId.StartsWith("OfficeLesson")) return "Office";
            if (lessonId.StartsWith("InternetLesson")) return "Internet";
            return "Other";
        }

        public static LessonData GetLesson(string lessonId)
        {
            if (_lessons.TryGetValue(lessonId, out var lesson))
            {
                return lesson;
            }
            return null;
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
                {
                    result.Add(lesson);
                }
            }

            return result;
        }

        public static List<LessonData> GetLessonsByCourse(string courseId)
        {
            var result = new List<LessonData>();

            foreach (var lesson in _lessons.Values)
            {
                if (lesson.CourseId == courseId)
                {
                    result.Add(lesson);
                }
            }

            return result;
        }

        public static int GetTotalLessonsInCourse(string courseId)
        {
            int count = 0;

            foreach (var lesson in _lessons.Values)
            {
                if (lesson.CourseId == courseId)
                {
                    count++;
                }
            }

            return count;
        }

        public static Dictionary<string, int> GetCourseLessonCounts()
        {
            var counts = new Dictionary<string, int>
            {
                { "Files", 0 },
                { "System", 0 },
                { "Office", 0 },
                { "Internet", 0 }
            };

            foreach (var lesson in _lessons.Values)
            {
                if (counts.ContainsKey(lesson.CourseId))
                {
                    counts[lesson.CourseId]++;
                }
            }

            return counts;
        }

        public static void UpdateCourseProgressInManager()
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
    }
}