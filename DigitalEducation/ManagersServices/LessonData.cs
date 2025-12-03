using System.Collections.Generic;

namespace DigitalEducation
{
    public class LessonStep
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Hint { get; set; }
    }

    public class LessonData
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string CourseId { get; set; }
        public List<LessonStep> Steps { get; set; }
        public string CompletionMessage { get; set; }
    }
}