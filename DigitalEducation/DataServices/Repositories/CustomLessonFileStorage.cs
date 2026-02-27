using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Encodings.Web;

namespace DigitalEducation.Pages.CreateCustomLesson
{
    public class CustomLessonFileStorage : ILessonStorage
    {
        private readonly string _customLessonsPath;
        private readonly string _templatesPath;
        private readonly JsonSerializerOptions _jsonOptions;

        public CustomLessonFileStorage()
        {
            string projectRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..");
            _customLessonsPath = Path.GetFullPath(Path.Combine(projectRoot, "DataServices", "Lessons", "CustomLessons"));
            _templatesPath = Path.GetFullPath(Path.Combine(projectRoot, "Learning", "Engine", "Templates"));

            Directory.CreateDirectory(_customLessonsPath);
            Directory.CreateDirectory(_templatesPath);

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
        }

        public LessonDataModel LoadLesson(string lessonId)
        {
            string filePath = Path.Combine(_customLessonsPath, $"{lessonId}.json");
            if (!File.Exists(filePath))
                return null;

            string json = File.ReadAllText(filePath, Encoding.UTF8);
            var lesson = JsonSerializer.Deserialize<LessonDataModel>(json, _jsonOptions);
            if (lesson?.Steps != null)
            {
                foreach (var step in lesson.Steps)
                {
                    if (!string.IsNullOrEmpty(step.VisionTarget))
                        step.VisionTarget = Path.GetFileNameWithoutExtension(step.VisionTarget);
                    if (!string.IsNullOrEmpty(step.VisionHint))
                        step.VisionHint = Path.GetFileNameWithoutExtension(step.VisionHint);
                }
            }
            return lesson;
        }

        public string GenerateLessonId(string lessonTitle)
        {
            int titleHash = Math.Abs(lessonTitle.GetHashCode());
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            string combined = $"{titleHash}{timestamp}";
            string numericOnly = new string(combined.Where(char.IsDigit).ToArray());
            return numericOnly.Length > 15 ? numericOnly.Substring(0, 15) : numericOnly;
        }

        public string GenerateNewLessonId()
        {
            return $"CustomLesson_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid():N}";
        }

        public LessonDataModel SaveNewLesson(LessonDataModel lesson, List<LessonStep> steps)
        {
            string lessonId = GenerateLessonId(lesson.Title);
            lesson.Id = lessonId;

            var stepsForJson = new List<object>();
            int screenshotCounter = 1;

            for (int i = 0; i < steps.Count; i++)
            {
                var step = steps[i];
                var stepDict = new Dictionary<string, object>
                {
                    ["title"] = step.Title,
                    ["description"] = step.Description
                };

                if (!string.IsNullOrWhiteSpace(step.Hint))
                    stepDict["hint"] = step.Hint;

                if (!string.IsNullOrEmpty(step.SelectedFolderPath) && Directory.Exists(step.SelectedFolderPath))
                {
                    string destFolderName = $"{lessonId}_folder_{i + 1}";
                    string destFolderPath = Path.Combine(_templatesPath, destFolderName);

                    bool isInTemplates = Path.GetDirectoryName(step.SelectedFolderPath).Equals(_templatesPath, StringComparison.OrdinalIgnoreCase);
                    bool isSameFolder = isInTemplates && Path.GetFileName(step.SelectedFolderPath).Equals(destFolderName, StringComparison.OrdinalIgnoreCase);

                    if (!isSameFolder)
                    {
                        Directory.CreateDirectory(destFolderPath);
                        var pngFiles = Directory.GetFiles(step.SelectedFolderPath, "*.png");
                        foreach (var file in pngFiles)
                        {
                            string fileName = Path.GetFileName(file);
                            string destFile = Path.Combine(destFolderPath, fileName);
                            File.Copy(file, destFile, true);
                        }
                    }

                    stepDict["visionTargetFolder"] = destFolderName;
                    stepDict["requiredMatches"] = step.RequiredMatches;
                    stepDict["visionConfidence"] = step.VisionConfidence;
                    stepDict["requiresVisionValidation"] = true;
                }
                else if (!string.IsNullOrEmpty(step.SelectedFilePath) && File.Exists(step.SelectedFilePath))
                {
                    string extension = Path.GetExtension(step.SelectedFilePath).ToLower();
                    string numericFileName = $"{lessonId}_{screenshotCounter:000}{extension}";
                    string destPath = Path.Combine(_templatesPath, numericFileName);

                    bool isInTemplates = Path.GetDirectoryName(step.SelectedFilePath).Equals(_templatesPath, StringComparison.OrdinalIgnoreCase);
                    if (!(isInTemplates && Path.GetFileName(step.SelectedFilePath).Equals(numericFileName, StringComparison.OrdinalIgnoreCase)))
                    {
                        File.Copy(step.SelectedFilePath, destPath, true);
                    }

                    stepDict["visionTarget"] = Path.GetFileNameWithoutExtension(numericFileName);
                    stepDict["visionConfidence"] = step.VisionConfidence;
                    stepDict["requiresVisionValidation"] = true;
                    screenshotCounter++;
                }
                else if (!string.IsNullOrEmpty(step.VisionTargetFolder))
                {
                    stepDict["visionTargetFolder"] = step.VisionTargetFolder;
                    stepDict["requiredMatches"] = step.RequiredMatches;
                    stepDict["visionConfidence"] = step.VisionConfidence;
                    stepDict["requiresVisionValidation"] = true;
                }

                if (!string.IsNullOrEmpty(step.SelectedHintFolderPath) && Directory.Exists(step.SelectedHintFolderPath))
                {
                    string destFolderName = $"{lessonId}_hintfolder_{i + 1}";
                    string destFolderPath = Path.Combine(_templatesPath, destFolderName);

                    bool isInTemplates = Path.GetDirectoryName(step.SelectedHintFolderPath).Equals(_templatesPath, StringComparison.OrdinalIgnoreCase);
                    bool isSameFolder = isInTemplates && Path.GetFileName(step.SelectedHintFolderPath).Equals(destFolderName, StringComparison.OrdinalIgnoreCase);

                    if (!isSameFolder)
                    {
                        Directory.CreateDirectory(destFolderPath);
                        var pngFiles = Directory.GetFiles(step.SelectedHintFolderPath, "*.png");
                        foreach (var file in pngFiles)
                        {
                            string fileName = Path.GetFileName(file);
                            string destFile = Path.Combine(destFolderPath, fileName);
                            File.Copy(file, destFile, true);
                        }
                    }

                    stepDict["visionHintFolder"] = destFolderName;
                    stepDict["requiredHintMatches"] = step.RequiredHintMatches;
                    stepDict["hintType"] = step.HintType ?? "rectangle";
                    stepDict["hintConfidence"] = step.HintConfidence;
                }
                else if (!string.IsNullOrEmpty(step.SelectedHintFilePath) && File.Exists(step.SelectedHintFilePath))
                {
                    string ext = Path.GetExtension(step.SelectedHintFilePath);
                    string hintFileName = $"{lessonId}_step{i + 1}_hint{ext}";
                    string destHintPath = Path.Combine(_templatesPath, hintFileName);

                    bool isInTemplates = Path.GetDirectoryName(step.SelectedHintFilePath).Equals(_templatesPath, StringComparison.OrdinalIgnoreCase);
                    if (!(isInTemplates && Path.GetFileName(step.SelectedHintFilePath).Equals(hintFileName, StringComparison.OrdinalIgnoreCase)))
                    {
                        File.Copy(step.SelectedHintFilePath, destHintPath, true);
                    }

                    stepDict["visionHint"] = Path.GetFileNameWithoutExtension(hintFileName);
                    stepDict["hintType"] = step.HintType ?? "rectangle";
                    stepDict["hintConfidence"] = step.HintConfidence;
                }
                else if (!string.IsNullOrEmpty(step.VisionHintFolder))
                {
                    stepDict["visionHintFolder"] = step.VisionHintFolder;
                    stepDict["requiredHintMatches"] = step.RequiredHintMatches;
                    stepDict["hintType"] = step.HintType ?? "rectangle";
                    stepDict["hintConfidence"] = step.HintConfidence;
                }
                else if (!string.IsNullOrEmpty(step.VisionHint))
                {
                    stepDict["visionHint"] = step.VisionHint;
                    stepDict["hintType"] = step.HintType ?? "rectangle";
                    stepDict["hintConfidence"] = step.HintConfidence;
                }

                if (step.ShowHint)
                {
                    stepDict["showHint"] = true;
                }

                stepsForJson.Add(stepDict);
            }

            var jsonObject = new
            {
                id = lesson.Id,
                title = lesson.Title,
                courseId = lesson.CourseId ?? "Custom",
                completionMessage = lesson.CompletionMessage,
                steps = stepsForJson
            };

            string filePath = Path.Combine(_customLessonsPath, $"{lesson.Id}.json");
            string json = JsonSerializer.Serialize(jsonObject, _jsonOptions);
            File.WriteAllText(filePath, json, Encoding.UTF8);

            return lesson;
        }

        public void UpdateLesson(string lessonId, LessonDataModel lesson, List<LessonStep> steps)
        {
            lesson.Id = lessonId;

            var stepsForJson = new List<object>();
            int screenshotCounter = 1;

            for (int i = 0; i < steps.Count; i++)
            {
                var step = steps[i];
                var stepDict = new Dictionary<string, object>
                {
                    ["title"] = step.Title,
                    ["description"] = step.Description
                };

                if (!string.IsNullOrWhiteSpace(step.Hint))
                    stepDict["hint"] = step.Hint;

                if (!string.IsNullOrEmpty(step.SelectedFolderPath) && Directory.Exists(step.SelectedFolderPath))
                {
                    string destFolderName = $"{lessonId}_folder_{i + 1}";
                    string destFolderPath = Path.Combine(_templatesPath, destFolderName);

                    bool isInTemplates = Path.GetDirectoryName(step.SelectedFolderPath).Equals(_templatesPath, StringComparison.OrdinalIgnoreCase);
                    bool isSameFolder = isInTemplates && Path.GetFileName(step.SelectedFolderPath).Equals(destFolderName, StringComparison.OrdinalIgnoreCase);

                    if (!isSameFolder)
                    {
                        Directory.CreateDirectory(destFolderPath);
                        var pngFiles = Directory.GetFiles(step.SelectedFolderPath, "*.png");
                        foreach (var file in pngFiles)
                        {
                            string fileName = Path.GetFileName(file);
                            string destFile = Path.Combine(destFolderPath, fileName);
                            File.Copy(file, destFile, true);
                        }
                    }

                    stepDict["visionTargetFolder"] = destFolderName;
                    stepDict["requiredMatches"] = step.RequiredMatches;
                    stepDict["visionConfidence"] = step.VisionConfidence;
                    stepDict["requiresVisionValidation"] = true;
                }
                else if (!string.IsNullOrEmpty(step.SelectedFilePath) && File.Exists(step.SelectedFilePath))
                {
                    string extension = Path.GetExtension(step.SelectedFilePath).ToLower();
                    string numericFileName = $"{lessonId}_{screenshotCounter:000}{extension}";
                    string destPath = Path.Combine(_templatesPath, numericFileName);

                    bool isInTemplates = Path.GetDirectoryName(step.SelectedFilePath).Equals(_templatesPath, StringComparison.OrdinalIgnoreCase);
                    if (!(isInTemplates && Path.GetFileName(step.SelectedFilePath).Equals(numericFileName, StringComparison.OrdinalIgnoreCase)))
                    {
                        File.Copy(step.SelectedFilePath, destPath, true);
                    }

                    stepDict["visionTarget"] = Path.GetFileNameWithoutExtension(numericFileName);
                    stepDict["visionConfidence"] = step.VisionConfidence;
                    stepDict["requiresVisionValidation"] = true;
                    screenshotCounter++;
                }
                else if (!string.IsNullOrEmpty(step.VisionTargetFolder))
                {
                    stepDict["visionTargetFolder"] = step.VisionTargetFolder;
                    stepDict["requiredMatches"] = step.RequiredMatches;
                    stepDict["visionConfidence"] = step.VisionConfidence;
                    stepDict["requiresVisionValidation"] = true;
                }

                if (!string.IsNullOrEmpty(step.SelectedHintFolderPath) && Directory.Exists(step.SelectedHintFolderPath))
                {
                    string destFolderName = $"{lessonId}_hintfolder_{i + 1}";
                    string destFolderPath = Path.Combine(_templatesPath, destFolderName);

                    bool isInTemplates = Path.GetDirectoryName(step.SelectedHintFolderPath).Equals(_templatesPath, StringComparison.OrdinalIgnoreCase);
                    bool isSameFolder = isInTemplates && Path.GetFileName(step.SelectedHintFolderPath).Equals(destFolderName, StringComparison.OrdinalIgnoreCase);

                    if (!isSameFolder)
                    {
                        Directory.CreateDirectory(destFolderPath);
                        var pngFiles = Directory.GetFiles(step.SelectedHintFolderPath, "*.png");
                        foreach (var file in pngFiles)
                        {
                            string fileName = Path.GetFileName(file);
                            string destFile = Path.Combine(destFolderPath, fileName);
                            File.Copy(file, destFile, true);
                        }
                    }

                    stepDict["visionHintFolder"] = destFolderName;
                    stepDict["requiredHintMatches"] = step.RequiredHintMatches;
                    stepDict["hintType"] = step.HintType ?? "rectangle";
                    stepDict["hintConfidence"] = step.HintConfidence;
                }
                else if (!string.IsNullOrEmpty(step.SelectedHintFilePath) && File.Exists(step.SelectedHintFilePath))
                {
                    string ext = Path.GetExtension(step.SelectedHintFilePath);
                    string hintFileName = $"{lessonId}_step{i + 1}_hint{ext}";
                    string destHintPath = Path.Combine(_templatesPath, hintFileName);

                    bool isInTemplates = Path.GetDirectoryName(step.SelectedHintFilePath).Equals(_templatesPath, StringComparison.OrdinalIgnoreCase);
                    if (!(isInTemplates && Path.GetFileName(step.SelectedHintFilePath).Equals(hintFileName, StringComparison.OrdinalIgnoreCase)))
                    {
                        File.Copy(step.SelectedHintFilePath, destHintPath, true);
                    }

                    stepDict["visionHint"] = Path.GetFileNameWithoutExtension(hintFileName);
                    stepDict["hintType"] = step.HintType ?? "rectangle";
                    stepDict["hintConfidence"] = step.HintConfidence;
                }
                else if (!string.IsNullOrEmpty(step.VisionHintFolder))
                {
                    stepDict["visionHintFolder"] = step.VisionHintFolder;
                    stepDict["requiredHintMatches"] = step.RequiredHintMatches;
                    stepDict["hintType"] = step.HintType ?? "rectangle";
                    stepDict["hintConfidence"] = step.HintConfidence;
                }
                else if (!string.IsNullOrEmpty(step.VisionHint))
                {
                    stepDict["visionHint"] = step.VisionHint;
                    stepDict["hintType"] = step.HintType ?? "rectangle";
                    stepDict["hintConfidence"] = step.HintConfidence;
                }

                if (step.ShowHint)
                {
                    stepDict["showHint"] = true;
                }

                stepsForJson.Add(stepDict);
            }

            var jsonObject = new
            {
                id = lesson.Id,
                title = lesson.Title,
                courseId = lesson.CourseId ?? "Custom",
                completionMessage = lesson.CompletionMessage,
                steps = stepsForJson
            };

            string filePath = Path.Combine(_customLessonsPath, $"{lesson.Id}.json");
            string json = JsonSerializer.Serialize(jsonObject, _jsonOptions);
            File.WriteAllText(filePath, json, Encoding.UTF8);
        }
    }
}