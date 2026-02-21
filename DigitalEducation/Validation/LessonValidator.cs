using System.Collections.Generic;
using System.Linq;

namespace DigitalEducation.Pages.CreateCustomLesson
{
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string TitleError { get; set; }
        public string CompletionError { get; set; }
        public string GeneralError { get; set; }
    }

    public static class LessonValidator
    {
        public static ValidationResult Validate(string title, string completionMessage, List<LessonStep> steps)
        {
            var result = new ValidationResult { IsValid = true };

            if (string.IsNullOrWhiteSpace(title))
            {
                result.TitleError = "Пожалуйста, введите название урока";
                result.IsValid = false;
            }

            if (string.IsNullOrWhiteSpace(completionMessage))
            {
                result.CompletionError = "Пожалуйста, введите сообщение о завершении";
                result.IsValid = false;
            }

            if (steps == null || steps.Count == 0)
            {
                result.GeneralError = "Пожалуйста, добавьте хотя бы один шаг";
                result.IsValid = false;
            }
            else
            {
                foreach (var step in steps)
                {
                    if (string.IsNullOrWhiteSpace(step.Description))
                    {
                        result.GeneralError = "Пожалуйста, заполните описание во всех шагах";
                        result.IsValid = false;
                        break;
                    }
                }
            }

            return result;
        }
    }
}