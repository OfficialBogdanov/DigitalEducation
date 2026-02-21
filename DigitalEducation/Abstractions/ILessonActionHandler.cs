using System;

namespace DigitalEducation
{
    public interface ILessonActionHandler
    {
        void EditLesson(LessonData lesson);
        void DeleteLesson(LessonData lesson);
        void StartLesson(LessonData lesson);
    }
}