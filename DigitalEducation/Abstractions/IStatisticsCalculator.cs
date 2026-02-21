namespace DigitalEducation
{
    public interface IStatisticsCalculator
    {
        void UpdateStatistics(UserProgress progress);
        void UpdateDaysInARow(UserProgress progress);
        void UpdateCourseProgress(UserProgress progress, string courseId, int totalLessonsInCourse);
        void EnsureCourseProgressExists(UserProgress progress, string courseId, int totalLessons);
    }
}