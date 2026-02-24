using System;

namespace DigitalEducation
{
    public interface ILessonActionHandler
    {
        void EditLesson(LessonDataModel lesson);
        void DeleteLesson(LessonDataModel lesson);
        void StartLesson(LessonDataModel lesson);
    }
}