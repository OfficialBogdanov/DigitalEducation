using System.Collections.Generic;

namespace DigitalEducation
{
    public interface ICustomLessonRepository
    {
        List<LessonDataModel> LoadAllLessons();
    }
}