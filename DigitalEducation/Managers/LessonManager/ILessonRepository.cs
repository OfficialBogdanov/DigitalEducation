using System.Collections.Generic;

namespace DigitalEducation
{
    public interface ILessonRepository
    {
        List<LessonData> LoadAllLessons();
    }
}