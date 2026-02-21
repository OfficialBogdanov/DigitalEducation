using System.Collections.Generic;

namespace DigitalEducation
{
    public interface ILessonProvider
    {
        List<LessonData> GetLessons(string courseId);
        List<LessonData> Filter(List<LessonData> lessons, string searchQuery);
        List<LessonData> Sort(List<LessonData> lessons, string sortOption);
    }
}