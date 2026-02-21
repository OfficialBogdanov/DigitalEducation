using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace DigitalEducation
{
    public interface ILessonLoader
    {
        LessonData LoadLesson(string lessonId);
    }

    public class LessonLoader : ILessonLoader
    {
        public LessonData LoadLesson(string lessonId)
        {
            var lesson = LessonManager.GetLesson(lessonId);
            if (lesson != null) return lesson;
            return LoadCustomLesson(lessonId);
        }

        private LessonData LoadCustomLesson(string lessonId)
        {
            try
            {
                string projectRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..");
                string customLessonsPath = Path.GetFullPath(
                    Path.Combine(projectRoot, "Lessons", "CustomLessons"));
                string lessonFilePath = Path.Combine(customLessonsPath, $"{lessonId}.json");

                if (!File.Exists(lessonFilePath)) return null;

                string jsonContent = File.ReadAllText(lessonFilePath, Encoding.UTF8);
                var lesson = JsonSerializer.Deserialize<LessonData>(jsonContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (lesson != null && string.IsNullOrEmpty(lesson.CourseId))
                    lesson.CourseId = "Custom";

                return lesson;
            }
            catch
            {
                return null;
            }
        }
    }
}