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
            return JsonSerializer.Deserialize<LessonData>(json, _jsonOptions);
        }

        public string GenerateLessonId(string lessonTitle)
        {
            int titleHash = Math.Abs(lessonTitle.GetHashCode());
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            string combined = $"{titleHash}{timestamp}";
            string numericOnly = new string(combined.Where(char.IsDigit).ToArray());
            return numericOnly.Length > 15 ? numericOnly.Substring(0, 15) : numericOnly;
        }

        public LessonData SaveNewLesson(LessonData lesson, List<LessonStep> steps)
        {
            string lessonId = GenerateLessonId(lesson.Title);
            lesson.Id = lessonId;

            List<object> stepsForJson = new List<object>();
            int screenshotCounter = 1;

            foreach (var step in steps)
            {
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

            List<object> stepsForJson = new List<object>();
            int screenshotCounter = 1;

            foreach (var step in steps)
            {
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