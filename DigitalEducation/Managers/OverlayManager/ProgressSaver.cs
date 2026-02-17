namespace DigitalEducation
{
    public interface IProgressSaver
    {
        void Save(string lessonId, string courseId, double timeSpentMinutes);
    }

    public class ProgressSaver : IProgressSaver
    {
        public void Save(string lessonId, string courseId, double timeSpentMinutes)
        {
            ProgressManager.SaveLessonCompletion(lessonId, courseId, timeSpentMinutes);
        }
    }
}