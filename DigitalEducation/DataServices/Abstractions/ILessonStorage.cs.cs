using System.Collections.Generic;

namespace DigitalEducation.Pages.CreateCustomLesson
{
    public interface ILessonStorage
    {
        LessonDataModel LoadLesson(string lessonId);
        string GenerateLessonId(string lessonTitle);
        LessonDataModel SaveNewLesson(LessonDataModel lesson, List<LessonStep> steps);
        void UpdateLesson(string lessonId, LessonDataModel lesson, List<LessonStep> steps);
        string GenerateNewLessonId();
    }
}