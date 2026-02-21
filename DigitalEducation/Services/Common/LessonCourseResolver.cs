using System;

namespace DigitalEducation
{
    public interface ICourseIdResolver
    {
        string GetCourseId(string lessonId);
    }

    public class LessonCourseResolver : ICourseIdResolver
    {
        public string GetCourseId(string lessonId)
        {
            if (lessonId.StartsWith("FilesLesson")) return "Files";
            if (lessonId.StartsWith("OsLesson")) return "System";
            if (lessonId.StartsWith("OfficeLesson")) return "Office";
            if (lessonId.StartsWith("InternetLesson")) return "Internet";
            return "Other";
        }
    }
}