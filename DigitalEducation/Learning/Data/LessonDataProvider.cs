using System;
using System.Collections.Generic;
using System.Linq;

namespace DigitalEducation
{
    public class LessonDataProvider : ILessonProvider
    {
        public List<LessonDataModel> GetLessons(string courseId)
        {
            return LessonRegistry.GetLessonsByCourse(courseId) ?? new List<LessonDataModel>();
        }

        public List<LessonDataModel> Filter(List<LessonDataModel> lessons, string searchQuery)
        {
            if (string.IsNullOrWhiteSpace(searchQuery))
                return lessons;

            var query = searchQuery.ToLower();
            return lessons
                .Where(l => !string.IsNullOrEmpty(l.Title) && l.Title.ToLower().Contains(query))
                .ToList();
        }

        public List<LessonDataModel> Sort(List<LessonDataModel> lessons, string sortOption, Dictionary<string, DateTime> creationDates = null)
        {
            if (sortOption.StartsWith("По дате"))
            {
                if (creationDates == null)
                    return lessons;

                if (sortOption.Contains("сначала новые"))
                    return lessons.OrderByDescending(l => creationDates.ContainsKey(l.Id) ? creationDates[l.Id] : DateTime.MinValue).ToList();
                else
                    return lessons.OrderBy(l => creationDates.ContainsKey(l.Id) ? creationDates[l.Id] : DateTime.MinValue).ToList();
            }
            else
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
}