namespace DigitalEducation
{
    public interface IStatisticsCalculator
    {
        void UpdateStatistics(UserProgressModel progress);
        void UpdateDaysInARow(UserProgressModel progress);
        void UpdateCourseProgress(UserProgressModel progress, string courseId, int totalLessonsInCourse);
        void EnsureCourseProgressExists(UserProgressModel progress, string courseId, int totalLessons);
    }
}