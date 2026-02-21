using System.Collections.Generic;

namespace DigitalEducation.Pages.CreateCustomLesson
{
    public interface ILessonStorage
    {
        LessonData LoadLesson(string lessonId);
        string GenerateLessonId(string lessonTitle);
        LessonData SaveNewLesson(LessonData lesson, List<LessonStep> steps);
        void UpdateLesson(string lessonId, LessonData lesson, List<LessonStep> steps);
    }
}