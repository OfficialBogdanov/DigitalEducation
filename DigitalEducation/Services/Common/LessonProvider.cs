using System;
using System.Collections.Generic;
using System.Linq;

namespace DigitalEducation
{
    public class LessonProvider : ILessonProvider
    {
        public List<LessonData> GetLessons(string courseId)
        {
            return LessonManager.GetLessonsByCourse(courseId) ?? new List<LessonData>();
        }

        public List<LessonData> Filter(List<LessonData> lessons, string searchQuery)
        {
            if (string.IsNullOrWhiteSpace(searchQuery))
                return lessons;

            var query = searchQuery.ToLower();
            return lessons
                .Where(l => !string.IsNullOrEmpty(l.Title) && l.Title.ToLower().Contains(query))
                .ToList();
        }

        public List<LessonData> Sort(List<LessonData> lessons, string sortOption)
        {
            switch (sortOption)
            {
                case "По названию (А-Я)":
                    return lessons.OrderBy(l => l.Title ?? "").ToList();
                case "По названию (Я-А)":
                    return lessons.OrderByDescending(l => l.Title ?? "").ToList();
                default:
                    return lessons.OrderBy(l => l.Title ?? "").ToList();
            }
        }
    }
}