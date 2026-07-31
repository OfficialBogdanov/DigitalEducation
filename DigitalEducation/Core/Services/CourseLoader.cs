using DigitalEducation.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Linq;

namespace DigitalEducation.Core.Services
{
    public interface ICourseLoader
    {
        List<CourseData> LoadAllCourses();
        CourseData LoadCourse(string courseId);
        List<LessonData> LoadLessons(string courseId);
        LessonData LoadLesson(string courseId, string lessonId);
        void Reload();
    }

    public class CourseLoader : ICourseLoader
    {
        private readonly string _coursesPath;
        private readonly JsonSerializerOptions _jsonOptions;
        private List<CourseData> _cachedCourses;

        public CourseLoader()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            string[] possiblePaths = new string[]
            {
                Path.Combine(baseDir, "Data", "Courses"),
                Path.Combine(baseDir, "..", "..", "..", "Data", "Courses"),
                Path.Combine(baseDir, "..", "..", "Data", "Courses"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Courses")
            };

            string foundPath = null;
            foreach (string path in possiblePaths)
            {
                string fullPath = Path.GetFullPath(path);
                if (Directory.Exists(fullPath))
                {
                    foundPath = fullPath;
                    break;
                }
            }

            if (foundPath == null)
            {
                foundPath = Path.Combine(baseDir, "Data", "Courses");
                Directory.CreateDirectory(foundPath);
            }

            _coursesPath = foundPath;
            System.Diagnostics.Debug.WriteLine($"[CourseLoader] Путь к курсам: {_coursesPath}");

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            };
        }

        public List<CourseData> LoadAllCourses()
        {
            if (_cachedCourses != null)
                return _cachedCourses;

            List<CourseData> courses = new List<CourseData>();

            if (!Directory.Exists(_coursesPath))
            {
                System.Diagnostics.Debug.WriteLine($"[CourseLoader] Папка не найдена: {_coursesPath}");
                return courses;
            }

            string[] courseFolders = Directory.GetDirectories(_coursesPath);
            System.Diagnostics.Debug.WriteLine($"[CourseLoader] Найдено папок курсов: {courseFolders.Length}");

            foreach (string folder in courseFolders)
            {
                string courseJsonPath = Path.Combine(folder, "course.json");
                System.Diagnostics.Debug.WriteLine($"[CourseLoader] Проверка: {courseJsonPath}");

                if (File.Exists(courseJsonPath))
                {
                    try
                    {
                        string json = File.ReadAllText(courseJsonPath);
                        System.Diagnostics.Debug.WriteLine($"[CourseLoader] Загружен JSON: {json.Substring(0, Math.Min(100, json.Length))}...");

                        CourseData course = System.Text.Json.JsonSerializer.Deserialize<CourseData>(json, _jsonOptions);

                        if (course != null)
                        {
                            string lessonsPath = Path.Combine(folder, "Lessons");
                            if (Directory.Exists(lessonsPath))
                            {
                                course.Lessons = LoadLessonsFromFolder(lessonsPath);
                            }
                            courses.Add(course);
                            System.Diagnostics.Debug.WriteLine($"[CourseLoader] Загружен курс: {course.Title}");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[CourseLoader] Ошибка загрузки {folder}: {ex.Message}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[CourseLoader] course.json не найден в {folder}");
                }
            }

            _cachedCourses = courses;
            return courses;
        }

        public CourseData LoadCourse(string courseId)
        {
            List<CourseData> allCourses = LoadAllCourses();
            return allCourses.FirstOrDefault(c => c.Id == courseId);
        }

        public List<LessonData> LoadLessons(string courseId)
        {
            CourseData course = LoadCourse(courseId);
            return course?.Lessons ?? new List<LessonData>();
        }

        public LessonData LoadLesson(string courseId, string lessonId)
        {
            List<LessonData> lessons = LoadLessons(courseId);
            return lessons.FirstOrDefault(l => l.Id == lessonId);
        }

        private List<LessonData> LoadLessonsFromFolder(string lessonsPath)
        {
            List<LessonData> lessons = new List<LessonData>();
            string[] lessonFiles = Directory.GetFiles(lessonsPath, "*.json");

            System.Diagnostics.Debug.WriteLine($"[CourseLoader] Найдено файлов уроков: {lessonFiles.Length} в {lessonsPath}");

            foreach (string file in lessonFiles)
            {
                try
                {
                    string json = File.ReadAllText(file);
                    LessonData lesson = System.Text.Json.JsonSerializer.Deserialize<LessonData>(json, _jsonOptions);

                    if (lesson != null)
                    {
                        lessons.Add(lesson);
                        System.Diagnostics.Debug.WriteLine($"[CourseLoader] Загружен урок: {lesson.Title}");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[CourseLoader] Ошибка загрузки урока {file}: {ex.Message}");
                }
            }

            return lessons.OrderBy(l => l.Order).ToList();
        }

        public void Reload()
        {
            _cachedCourses = null;
        }
    }
}