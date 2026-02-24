using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Encodings.Web;

namespace DigitalEducation.Pages.CreateCustomLesson
{
    public class LessonFileStorage : ILessonStorage
    {
        private readonly string _customLessonsPath;
        private readonly string _templatesPath;
        private readonly JsonSerializerOptions _jsonOptions;

        public LessonFileStorage()
        {
            string projectRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..");
            _customLessonsPath = Path.GetFullPath(Path.Combine(projectRoot, "Lessons", "CustomLessons"));
            _templatesPath = Path.GetFullPath(Path.Combine(projectRoot, "Engine", "ComputerVision", "Templates"));

            Directory.CreateDirectory(_customLessonsPath);
            Directory.CreateDirectory(_templatesPath);

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
        }

        public LessonData LoadLesson(string lessonId)
        {
            string filePath = Path.Combine(_customLessonsPath, $"{lessonId}.json");
            if (!File.Exists(filePath))
                return null;

            string json = File.ReadAllText(filePath, Encoding.UTF8);
            var lesson = JsonSerializer.Deserialize<LessonData>(json, _jsonOptions);
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

        public LessonData SaveNewLesson(LessonData lesson, List<LessonStep> steps)
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

                if (!string.IsNullOrEmpty(step.VisionTarget) && File.Exists(step.VisionTarget))
                {
                    string extension = Path.GetExtension(step.VisionTarget).ToLower();
                    string numericFileName = $"{lessonId}_{screenshotCounter:000}{extension}";
                    string destPath = Path.Combine(_templatesPath, numericFileName);

                    File.Copy(step.VisionTarget, destPath, true);
                    stepDict["visionTarget"] = numericFileName;
                    stepDict["visionConfidence"] = 0.85;
                    stepDict["requiresVisionValidation"] = true;

                    screenshotCounter++;
                }
                else if (!string.IsNullOrEmpty(step.VisionTargetFolder))
                {
                    stepDict["visionTargetFolder"] = step.VisionTargetFolder;
                    stepDict["requiredMatches"] = 1;
                    stepDict["visionConfidence"] = 0.8;
                    stepDict["requiresVisionValidation"] = true;
                }

                if (!string.IsNullOrEmpty(step.HintImagePath) && File.Exists(step.HintImagePath))
                {
                    string ext = Path.GetExtension(step.HintImagePath);
                    string hintFileName = $"{lessonId}_step{i + 1}_hint{ext}";
                    string destHintPath = Path.Combine(_templatesPath, hintFileName);
                    File.Copy(step.HintImagePath, destHintPath, true);
                    stepDict["visionHint"] = hintFileName;
                    stepDict["hintType"] = step.HintType ?? "rectangle";
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

        public void UpdateLesson(string lessonId, LessonData lesson, List<LessonStep> steps)
        {
            lesson.Id = lessonId;

            var oldImages = Directory.GetFiles(_templatesPath, $"{lessonId}_*.*");
            foreach (var img in oldImages)
            {
                try { File.Delete(img); } catch { }
            }

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

                if (!string.IsNullOrEmpty(step.VisionTarget) && File.Exists(step.VisionTarget))
                {
                    string extension = Path.GetExtension(step.VisionTarget).ToLower();
                    string numericFileName = $"{lessonId}_{screenshotCounter:000}{extension}";
                    string destPath = Path.Combine(_templatesPath, numericFileName);
                    File.Copy(step.VisionTarget, destPath, true);
                    stepDict["visionTarget"] = Path.GetFileNameWithoutExtension(numericFileName); 
                    stepDict["visionConfidence"] = 0.85;
                    stepDict["requiresVisionValidation"] = true;
                    screenshotCounter++;
                }
                else if (!string.IsNullOrEmpty(step.VisionTargetFolder))
                {
                    stepDict["visionTargetFolder"] = step.VisionTargetFolder;
                    stepDict["requiredMatches"] = 1;
                    stepDict["visionConfidence"] = 0.8;
                    stepDict["requiresVisionValidation"] = true;
                }

                if (!string.IsNullOrEmpty(step.HintImagePath) && File.Exists(step.HintImagePath))
                {
                    string ext = Path.GetExtension(step.HintImagePath);
                    string hintFileName = $"{lessonId}_step{i + 1}_hint{ext}";
                    string destHintPath = Path.Combine(_templatesPath, hintFileName);
                    File.Copy(step.HintImagePath, destHintPath, true);
                    stepDict["visionHint"] = Path.GetFileNameWithoutExtension(hintFileName);
                    stepDict["hintType"] = step.HintType ?? "rectangle";
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