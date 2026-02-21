using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;

namespace DigitalEducation
{
    public class CustomLessonRepository : ICustomLessonRepository
    {
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        public List<LessonData> LoadAllLessons()
        {
            var lessons = new List<LessonData>();
            string lessonsRoot = GetLessonsRootPath();
            if (!Directory.Exists(lessonsRoot))
            {
                Directory.CreateDirectory(lessonsRoot);
                return lessons;
            }

            var categoryFolders = Directory.GetDirectories(lessonsRoot);
            foreach (var folder in categoryFolders)
            {
                lessons.AddRange(LoadLessonsFromFolder(folder));
            }
            return lessons;
        }

        private List<LessonData> LoadLessonsFromFolder(string folderPath)
        {
            var lessons = new List<LessonData>();
            var jsonFiles = Directory.GetFiles(folderPath, "*.json");
            foreach (var filePath in jsonFiles)
            {
                try
                {
                    string jsonContent = File.ReadAllText(filePath, Encoding.UTF8);
                    var lesson = JsonSerializer.Deserialize<LessonData>(jsonContent, _jsonOptions);
                    if (lesson != null && !string.IsNullOrEmpty(lesson.Id))
                    {
                        if (lesson.Steps == null)
                            lesson.Steps = new List<LessonStep>();
                        if (string.IsNullOrEmpty(lesson.CourseId))
                        {
                            if (folderPath.Contains("CustomLessons"))
                                lesson.CourseId = "Custom";
                            else
                                lesson.CourseId = GetCourseIdFromLessonId(lesson.Id);
                        }
                        lessons.Add(lesson);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[FileSystemLessonRepository] Ошибка загрузки {filePath}: {ex.Message}");
                }
            }
            return lessons;
        }

        private string GetLessonsRootPath()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string lessonsPath = Path.Combine(baseDir, "Lessons");
            if (!Directory.Exists(lessonsPath))
            {
                lessonsPath = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "Lessons"));
            }
            return lessonsPath;
        }

        private string GetCourseIdFromLessonId(string lessonId)
        {
            if (lessonId.StartsWith("FilesLesson")) return "Files";
            if (lessonId.StartsWith("OsLesson")) return "System";
            if (lessonId.StartsWith("OfficeLesson")) return "Office";
            if (lessonId.StartsWith("InternetLesson")) return "Internet";
            return "Other";
        }
    }
}