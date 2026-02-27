using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DigitalEducation
{
    public class LessonDataModel
    {
        [Required]
        public string Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string CourseId { get; set; }

        public List<LessonStep> Steps { get; set; } = new List<LessonStep>();

        public string CompletionMessage { get; set; }
    }

    public class LessonStep
    {
        [Required]
        public string Title { get; set; }

        public string Description { get; set; }
        public string Hint { get; set; }

        public string VisionTarget { get; set; }
        public double VisionConfidence { get; set; } = 0.8;
        public bool RequiresVisionValidation { get; set; }


        public string SelectedFilePath { get; set; }
        public string SelectedFolderPath { get; set; }
        public string VisionTargetFolder { get; set; }
        public int RequiredMatches { get; set; } = 1;

        public string VisionHint { get; set; }
        public double HintConfidence { get; set; } = 0.8;
        public bool ShowHint { get; set; }

        public string HintType { get; set; } = "rectangle";
        public string HintImagePath { get; set; }

        public string VisionHintFolder { get; set; }
        public string SelectedHintFilePath { get; set; }
        public string SelectedHintFolderPath { get; set; }
        public int RequiredHintMatches { get; set; } = 1;
    }
}