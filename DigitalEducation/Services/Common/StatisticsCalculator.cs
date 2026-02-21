using System;

namespace DigitalEducation
{
    public class StatisticsCalculator : IStatisticsCalculator
    {
        public void UpdateStatistics(UserProgress progress)
        {
            progress.Statistics.TotalLessonsCompleted = progress.CompletedLessons.Count;

            double totalTime = 0;
            foreach (var lesson in progress.CompletedLessons)
            {
                totalTime += lesson.TimeSpentMinutes;
            }
            progress.Statistics.TotalTimeSpentMinutes = totalTime;

            progress.Statistics.TotalCoursesCompleted = 0;
            foreach (var course in progress.CourseProgress)
            {
                if (course.Value.CompletionPercentage >= 100)
                {
                    progress.Statistics.TotalCoursesCompleted++;
                }
            }
        }

        public void UpdateDaysInARow(UserProgress progress)
        {
            var today = DateTime.UtcNow.Date;
            var lastDate = progress.Statistics.LastLearningDate?.Date;

            if (lastDate == null)
            {
                progress.Statistics.DaysInARow = 1;
            }
            else if (lastDate == today.AddDays(-1))
            {
                progress.Statistics.DaysInARow++;
            }
            else if (lastDate < today.AddDays(-1))
            {
                progress.Statistics.DaysInARow = 1;
            }

            progress.Statistics.LastLearningDate = today;
        }

        public void UpdateCourseProgress(UserProgress progress, string courseId, int totalLessonsInCourse)
        {
            if (!progress.CourseProgress.TryGetValue(courseId, out var course))
                return;

            int completedCount = 0;
            double totalTime = 0;

            foreach (var lesson in progress.CompletedLessons)
            {
                if (lesson.CourseId == courseId)
                {
                    completedCount++;
                    totalTime += lesson.TimeSpentMinutes;
                }
            }

            course.CompletedLessons = completedCount;
            course.TotalTimeMinutes = totalTime;
            course.TotalLessons = totalLessonsInCourse;

            if (totalLessonsInCourse > 0)
            {
                course.CompletionPercentage = (int)((double)completedCount / totalLessonsInCourse * 100);
            }
            else
            {
                course.CompletionPercentage = 0;
            }
        }

        public void EnsureCourseProgressExists(UserProgress progress, string courseId, int totalLessons)
        {
            if (!progress.CourseProgress.ContainsKey(courseId))
            {
                progress.CourseProgress[courseId] = new CourseProgress
                {
                    TotalLessons = totalLessons
                };
            }
        }
    }
}