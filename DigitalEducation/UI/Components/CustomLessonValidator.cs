// CustomLessonValidator.cs
using System.Collections.Generic;
using System.IO;
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

    public static class CustomLessonValidator
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

                    // Валидация источника для проверки (validation)
                    bool hasValidationFile = !string.IsNullOrEmpty(step.SelectedFilePath);
                    bool hasValidationFolder = !string.IsNullOrEmpty(step.SelectedFolderPath);

                    if (hasValidationFile && hasValidationFolder)
                    {
                        result.GeneralError = "Нельзя одновременно выбрать и файл, и папку для проверки";
                        result.IsValid = false;
                        break;
                    }

                    if (hasValidationFile)
                    {
                        if (step.VisionConfidence < 0.6 || step.VisionConfidence > 0.9)
                        {
                            result.GeneralError = "Точность совпадения для изображения проверки должна быть от 0.6 до 0.9";
                            result.IsValid = false;
                            break;
                        }
                    }

                    if (hasValidationFolder)
                    {
                        if (step.RequiredMatches < 1)
                        {
                            result.GeneralError = "Количество необходимых совпадений для проверки должно быть не меньше 1";
                            result.IsValid = false;
                            break;
                        }

                        if (!string.IsNullOrEmpty(step.SelectedFolderPath) && Directory.Exists(step.SelectedFolderPath))
                        {
                            var pngFiles = Directory.GetFiles(step.SelectedFolderPath, "*.png");
                            if (pngFiles.Length == 0)
                            {
                                result.GeneralError = "Выбранная папка для проверки не содержит PNG-файлов";
                                result.IsValid = false;
                                break;
                            }

                            if (step.RequiredMatches > pngFiles.Length)
                            {
                                result.GeneralError = "Количество необходимых совпадений превышает количество файлов в папке";
                                result.IsValid = false;
                                break;
                            }
                        }

                        if (step.VisionConfidence < 0.6 || step.VisionConfidence > 0.9)
                        {
                            result.GeneralError = "Точность совпадения для проверки папки должна быть от 0.6 до 0.9";
                            result.IsValid = false;
                            break;
                        }
                    }

                    // Валидация источника для подсказки (hint)
                    bool hasHintFile = !string.IsNullOrEmpty(step.SelectedHintFilePath);
                    bool hasHintFolder = !string.IsNullOrEmpty(step.SelectedHintFolderPath);

                    if (hasHintFile && hasHintFolder)
                    {
                        result.GeneralError = "Нельзя одновременно выбрать и файл, и папку для подсказки";
                        result.IsValid = false;
                        break;
                    }

                    if (hasHintFile || hasHintFolder)
                    {
                        

                        if (step.HintConfidence < 0.6 || step.HintConfidence > 0.9)
                        {
                            result.GeneralError = "Точность подсказки должна быть от 0.6 до 0.9";
                            result.IsValid = false;
                            break;
                        }
                    }

                    if (hasHintFolder)
                    {
                        if (step.RequiredHintMatches < 1)
                        {
                            result.GeneralError = "Количество необходимых совпадений для подсказки должно быть не меньше 1";
                            result.IsValid = false;
                            break;
                        }

                        if (!string.IsNullOrEmpty(step.SelectedHintFolderPath) && Directory.Exists(step.SelectedHintFolderPath))
                        {
                            var pngFiles = Directory.GetFiles(step.SelectedHintFolderPath, "*.png");
                            if (pngFiles.Length == 0)
                            {
                                result.GeneralError = "Выбранная папка для подсказки не содержит PNG-файлов";
                                result.IsValid = false;
                                break;
                            }

                            if (step.RequiredHintMatches > pngFiles.Length)
                            {
                                result.GeneralError = "Количество необходимых совпадений для подсказки превышает количество файлов в папке";
                                result.IsValid = false;
                                break;
                            }
                        }
                    }
                }
            }

            return result;
        }
    }
}