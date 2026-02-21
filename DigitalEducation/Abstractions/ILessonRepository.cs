using System.Collections.Generic;

namespace DigitalEducation
{
    public interface ICustomLessonRepository
    {
        List<LessonData> LoadAllLessons();
    }
}