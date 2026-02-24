namespace DigitalEducation
{
    public interface IProgressSaver
    {
        void Save(string lessonId, string courseId, double timeSpentMinutes);
    }

    public class LessonProgressSaver : IProgressSaver
    {
        public void Save(string lessonId, string courseId, double timeSpentMinutes)
        {
            UserProgressManager.SaveLessonCompletion(lessonId, courseId, timeSpentMinutes);
        }
    }
}