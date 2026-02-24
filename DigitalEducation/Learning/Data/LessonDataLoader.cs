using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace DigitalEducation
{
    public interface ILessonLoader
    {
        LessonDataModel LoadLesson(string lessonId);
    }

    public class LessonDataLoader : ILessonLoader
    {
        public LessonDataModel LoadLesson(string lessonId)
        {
            var lesson = LessonRegistry.GetLesson(lessonId);
            if (lesson != null) return lesson;
            return LoadCustomLesson(lessonId);
        }

        private LessonDataModel LoadCustomLesson(string lessonId)
        {
            try
            {
                string projectRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..");
                string customLessonsPath = Path.GetFullPath(
                    Path.Combine(projectRoot, "DataServices", "Lessons", "CustomLessons"));
                string lessonFilePath = Path.Combine(customLessonsPath, $"{lessonId}.json");

                if (!File.Exists(lessonFilePath)) return null;

                string jsonContent = File.ReadAllText(lessonFilePath, Encoding.UTF8);
                var lesson = JsonSerializer.Deserialize<LessonDataModel>(jsonContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (lesson != null && string.IsNullOrEmpty(lesson.CourseId))
                    lesson.CourseId = "Custom";

                if (lesson?.Steps != null)
                {
                    foreach (var step in lesson.Steps)
                    {
                        if (!string.IsNullOrEmpty(step.VisionTarget))
                            step.VisionTarget = Path.GetFileNameWithoutExtension(step.VisionTarget);
                        if (!string.IsNullOrEmpty(step.VisionHint))
                            step.VisionHint = Path.GetFileNameWithoutExtension(step.VisionHint);
                    }
                }

                return lesson;
            }
            catch
            {
                return null;
            }
        }
    }
}