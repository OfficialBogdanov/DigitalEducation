using System.Collections.Generic;

namespace DigitalEducation.Core.Models
{
    public class CourseData
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string Level { get; set; }
        public string EstimatedTime { get; set; }
        public string Category { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public List<LessonData> Lessons { get; set; } = new List<LessonData>();
    }

    public class LessonData
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Order { get; set; }
        public int EstimatedMinutes { get; set; }
        public string Difficulty { get; set; }
        public List<StepData> Steps { get; set; } = new List<StepData>();
    }

    public class StepData
    {
        public int Order { get; set; }
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
        public string HintType { get; set; } = "rectangle";
        public string VisionHintFolder { get; set; }
        public int RequiredHintMatches { get; set; } = 1;
    }
}