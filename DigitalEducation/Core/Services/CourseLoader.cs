using DigitalEducation.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace DigitalEducation.Core.Services
{
    public interface ICourseLoader
    {
        Task LoadAllCoursesAsync();
        List<CourseData> GetAllCourses();
        CourseData GetCourse(string courseId);
        List<LessonData> GetLessons(string courseId);
        LessonData GetLesson(string courseId, string lessonId);
        bool IsLoaded { get; }
        event EventHandler LoadingCompleted;
        event EventHandler<string> LoadingProgress;
    }

    public class CourseLoader : ICourseLoader
    {
        private static CourseLoader _instance;
        private static readonly object _lock = new object();

        private readonly string _coursesPath;
        private readonly JsonSerializerOptions _jsonOptions;
        private List<CourseData> _cachedCourses;
        private bool _isLoaded = false;

        public bool IsLoaded => _isLoaded;

        public event EventHandler LoadingCompleted;
        public event EventHandler<string> LoadingProgress;

        private CourseLoader()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            string[] possiblePaths = new string[]
            {
                Path.GetFullPath(Path.Combine(baseDir, "..", "..", "Data", "Courses")),
                Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "Data", "Courses")),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Courses"),
                Path.GetFullPath(Path.Combine(baseDir, "..", "Data", "Courses"))
            };

            string foundPath = null;
            foreach (string path in possiblePaths)
            {
                if (Directory.Exists(path))
                {
                    foundPath = path;
                    System.Diagnostics.Debug.WriteLine($"[CourseLoader] Найден путь: {path}");
                    break;
                }
            }

            if (foundPath == null)
            {
                foundPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Courses");
                System.Diagnostics.Debug.WriteLine($"[CourseLoader] ПАПКА НЕ НАЙДЕНА, используем: {foundPath}");
            }

            _coursesPath = foundPath;
            System.Diagnostics.Debug.WriteLine($"[CourseLoader] ИТОГОВЫЙ ПУТЬ К КУРСАМ: {_coursesPath}");

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            };
        }

        public static CourseLoader Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new CourseLoader();
                        }
                    }
                }
                return _instance;
            }
        }

        public async Task LoadAllCoursesAsync()
        {
            if (_isLoaded)
                return;

            await Task.Run(() =>
            {
                try
                {
                    List<CourseData> courses = new List<CourseData>();

                    System.Diagnostics.Debug.WriteLine($"[CourseLoader] Поиск курсов в: {_coursesPath}");

                    if (!Directory.Exists(_coursesPath))
                    {
                        System.Diagnostics.Debug.WriteLine($"[CourseLoader] ПАПКА НЕ НАЙДЕНА: {_coursesPath}");
                        _cachedCourses = courses;
                        _isLoaded = true;
                        SafeRaiseProgress("Папка с курсами не найдена");
                        SafeRaiseCompleted();
                        return;
                    }

                    string[] courseFolders = Directory.GetDirectories(_coursesPath);
                    System.Diagnostics.Debug.WriteLine($"[CourseLoader] Найдено папок: {courseFolders.Length}");

                    int total = courseFolders.Length;
                    int current = 0;

                    foreach (string folder in courseFolders)
                    {
                        current++;
                        string folderName = Path.GetFileName(folder);
                        SafeRaiseProgress($"Загрузка курса: {folderName} ({current}/{total})");

                        string courseJsonPath = Path.Combine(folder, "course.json");

                        if (File.Exists(courseJsonPath))
                        {
                            try
                            {
                                string json = File.ReadAllText(courseJsonPath);
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
                    }

                    _cachedCourses = courses;
                    _isLoaded = true;
                    SafeRaiseProgress($"Загружено {courses.Count} курсов");
                    SafeRaiseCompleted();

                    System.Diagnostics.Debug.WriteLine($"[CourseLoader] ИТОГО ЗАГРУЖЕНО КУРСОВ: {courses.Count}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[CourseLoader] КРИТИЧЕСКАЯ ОШИБКА: {ex.Message}");
                    _cachedCourses = new List<CourseData>();
                    _isLoaded = true;
                    SafeRaiseCompleted();
                }
            });
        }
        private void SafeRaiseProgress(string message)
        {
            try
            {
                if (Application.Current?.Dispatcher != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        LoadingProgress?.Invoke(this, message);
                    });
                }
                else
                {
                    LoadingProgress?.Invoke(this, message);
                }
            }
            catch
            {
            }
        }

        private void SafeRaiseCompleted()
        {
            try
            {
                if (Application.Current?.Dispatcher != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        LoadingCompleted?.Invoke(this, EventArgs.Empty);
                    });
                }
                else
                {
                    LoadingCompleted?.Invoke(this, EventArgs.Empty);
                }
            }
            catch
            {
            }
        }

        private List<LessonData> LoadLessonsFromFolder(string lessonsPath)
        {
            List<LessonData> lessons = new List<LessonData>();
            string[] lessonFiles = Directory.GetFiles(lessonsPath, "*.json");

            foreach (string file in lessonFiles)
            {
                try
                {
                    string json = File.ReadAllText(file);
                    LessonData lesson = System.Text.Json.JsonSerializer.Deserialize<LessonData>(json, _jsonOptions);

                    if (lesson != null)
                    {
                        lessons.Add(lesson);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[CourseLoader] Ошибка загрузки урока {file}: {ex.Message}");
                }
            }

            return lessons.OrderBy(l => l.Order).ToList();
        }

        public List<CourseData> GetAllCourses()
        {
            if (_cachedCourses == null)
                return new List<CourseData>();
            return _cachedCourses;
        }

        public CourseData GetCourse(string courseId)
        {
            if (_cachedCourses == null)
                return null;
            return _cachedCourses.FirstOrDefault(c => c.Id == courseId);
        }

        public List<LessonData> GetLessons(string courseId)
        {
            CourseData course = GetCourse(courseId);
            return course?.Lessons ?? new List<LessonData>();
        }

        public LessonData GetLesson(string courseId, string lessonId)
        {
            List<LessonData> lessons = GetLessons(courseId);
            return lessons.FirstOrDefault(l => l.Id == lessonId);
        }

        public void Reload()
        {
            _cachedCourses = null;
            _isLoaded = false;
        }
    }
}