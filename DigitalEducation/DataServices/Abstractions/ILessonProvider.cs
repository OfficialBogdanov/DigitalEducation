using System.Collections.Generic;

namespace DigitalEducation
{
    public interface ILessonProvider
    {
        List<LessonDataModel> GetLessons(string courseId);
        List<LessonDataModel> Filter(List<LessonDataModel> lessons, string searchQuery);
        List<LessonDataModel> Sort(List<LessonDataModel> lessons, string sortOption, Dictionary<string, System.DateTime> creationDates = null);
    }
}