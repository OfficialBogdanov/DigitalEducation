using System.Collections.Generic;

namespace DigitalEducation
{
    public class LessonData
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string CourseId { get; set; }
        public List<LessonStep> Steps { get; set; }
        public string CompletionMessage { get; set; }
    }

    public class LessonStep
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Hint { get; set; }

        public string VisionTarget { get; set; }
        public double VisionConfidence { get; set; } = 0.85;
        public bool RequiresVisionValidation { get; set; }

        public string VisionTargetFolder { get; set; }
        public int RequiredMatches { get; set; } = 1;

        public string VisionHint { get; set; }
        public double HintConfidence { get; set; } = 0.8;
        public bool ShowHint { get; set; }
    }
}