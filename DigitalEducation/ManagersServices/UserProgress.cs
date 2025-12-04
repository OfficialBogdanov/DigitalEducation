using System;
using System.Collections.Generic;

namespace DigitalEducation
{
    public class UserProgress
    {
        public DateTime LastUpdated { get; set; }
        public UserStatistics Statistics { get; set; } = new UserStatistics();

        public List<CompletedLesson> CompletedLessons { get; set; } = new List<CompletedLesson>();
        public Dictionary<string, CourseProgress> CourseProgress { get; set; } = new Dictionary<string, CourseProgress>();
    }

    public class CompletedLesson
    {
        public string LessonId { get; set; }
        public string CourseId { get; set; }
        public DateTime CompletionDate { get; set; }
        public double TimeSpentMinutes { get; set; }
        public string Status { get; set; } = "completed";
    }

    public class UserStatistics
    {
        public int TotalCoursesCompleted { get; set; }
        public int TotalLessonsCompleted { get; set; }
        public double TotalTimeSpentMinutes { get; set; }
        public int DaysInARow { get; set; }
        public DateTime? LastLearningDate { get; set; }
    }

    public class CourseProgress
    {
        public int TotalLessons { get; set; }
        public int CompletedLessons { get; set; }
        public int CompletionPercentage { get; set; }
        public double TotalTimeMinutes { get; set; }
    }
}