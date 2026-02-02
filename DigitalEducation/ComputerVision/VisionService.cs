using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace DigitalEducation.ComputerVision.Services
{
    public class VisionService : IDisposable
    {
        private readonly ScreenCapturer _screenCapturer;
        private readonly Dictionary<string, TemplateInfo> _templates;
        private bool _isDisposed;
        private readonly string _templatesBasePath;
        private readonly object _captureLock = new object();

        public VisionService(string templatesBasePath = null)
        {
            _screenCapturer = new ScreenCapturer();
            _templates = new Dictionary<string, TemplateInfo>();

            _templatesBasePath = templatesBasePath ?? Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "ComputerVision",
                "Templates"
            );

            LoadTemplates();
        }

        public async Task<RecognitionResult> FindElementAsync(string elementName, double confidenceThreshold = 0.8)
        {
            if (!_templates.ContainsKey(elementName))
                return RecognitionResult.NotFound();

            var templateInfo = _templates[elementName];
            string templatePath = GetTemplatePath(templateInfo);

            if (!File.Exists(templatePath))
                return RecognitionResult.NotFound();

            return await Task.Run(() =>
            {
                lock (_captureLock)
                {
                    using (var screenBitmap = _screenCapturer.CaptureScreen())
                    using (var matcher = new TemplateMatcher(screenBitmap))
                    {
                        var result = matcher.FindTemplate(templatePath, confidenceThreshold);

                        if (result.IsDetected && templateInfo.SearchArea != Rectangle.Empty)
                        {
                            if (!templateInfo.SearchArea.Contains(result.Bounds))
                            {
                                return RecognitionResult.NotFound();
                            }
                        }

                        return result;
                    }
                }
            });
        }

        public async Task<RecognitionResult> FindElementInRegionAsync(string elementName, System.Drawing.Rectangle searchRegion, double confidenceThreshold = 0.8)
        {
            if (!_templates.ContainsKey(elementName))
                return RecognitionResult.NotFound();

            var templateInfo = _templates[elementName];
            string templatePath = GetTemplatePath(templateInfo);

            if (!File.Exists(templatePath))
                return RecognitionResult.NotFound();

            return await Task.Run(() =>
            {
                lock (_captureLock)
                {
                    using (var screenBitmap = _screenCapturer.CaptureRegion(searchRegion))
                    using (var matcher = new TemplateMatcher(screenBitmap))
                    {
                        var result = matcher.FindTemplate(templatePath, confidenceThreshold);

                        if (result.IsDetected)
                        {
                            var adjustedLocation = new System.Drawing.Point(
                                result.Location.X + searchRegion.X,
                                result.Location.Y + searchRegion.Y
                            );
                            return RecognitionResult.Found(adjustedLocation, result.Size, result.Confidence);
                        }

                        return result;
                    }
                }
            });
        }

        public async Task<bool> ValidateActionAsync(string elementName, string expectedAction, double confidenceThreshold = 0.8)
        {
            var result = await FindElementAsync(elementName, confidenceThreshold);

            if (!result.IsDetected)
                return false;

            switch (expectedAction.ToLower())
            {
                case "visible":
                    return result.IsDetected;

                case "clicked":
                case "selected":
                    return await CheckElementStateAsync(elementName, result, expectedAction);

                default:
                    return result.IsDetected;
            }
        }

        public async Task<System.Drawing.Point?> GetElementPositionAsync(string elementName, double confidenceThreshold = 0.8)
        {
            var result = await FindElementAsync(elementName, confidenceThreshold);

            if (result.IsDetected)
                return result.Location;

            return null;
        }

        public async Task<bool> WaitForElementAsync(string elementName, int timeoutMs = 5000, double confidenceThreshold = 0.8, int checkIntervalMs = 500)
        {
            var startTime = DateTime.Now;

            while ((DateTime.Now - startTime).TotalMilliseconds < timeoutMs)
            {
                var result = await FindElementAsync(elementName, confidenceThreshold);

                if (result.IsDetected)
                    return true;

                await Task.Delay(checkIntervalMs);
            }

            return false;
        }

        public void AddTemplate(string name, string category, string fileName, System.Drawing.Rectangle searchArea = default, double confidenceThreshold = 0.8)
        {
            var templateInfo = new TemplateInfo
            {
                Name = name,
                Category = category,
                FileName = fileName,
                SearchArea = searchArea,
                ConfidenceThreshold = confidenceThreshold
            };

            _templates[name] = templateInfo;
        }

        public void RemoveTemplate(string name)
        {
            if (_templates.ContainsKey(name))
                _templates.Remove(name);
        }

        public List<string> GetAvailableElements()
        {
            return new List<string>(_templates.Keys);
        }

        public async Task SaveDebugScreenshotAsync(string elementName = null)
        {
            await Task.Run(() =>
            {
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string fileName = elementName != null
                    ? $"debug_{elementName}_{timestamp}.png"
                    : $"debug_{timestamp}.png";

                string debugPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DebugScreenshots");

                if (!Directory.Exists(debugPath))
                    Directory.CreateDirectory(debugPath);

                string filePath = Path.Combine(debugPath, fileName);

                using (var screenBitmap = _screenCapturer.CaptureScreen())
                {
                    _screenCapturer.SaveScreenshot(screenBitmap, filePath);
                }
            });
        }

        private void LoadTemplates()
        {
            if (!Directory.Exists(_templatesBasePath))
                return;

            var categories = Directory.GetDirectories(_templatesBasePath);

            foreach (var categoryPath in categories)
            {
                var categoryName = Path.GetFileName(categoryPath);
                var templateFiles = Directory.GetFiles(categoryPath, "*.png");

                foreach (var filePath in templateFiles)
                {
                    var fileName = Path.GetFileNameWithoutExtension(filePath);
                    AddTemplate(fileName, categoryName, Path.GetFileName(filePath));
                }
            }

            var rootTemplateFiles = Directory.GetFiles(_templatesBasePath, "*.png");
            foreach (var filePath in rootTemplateFiles)
            {
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                AddTemplate(fileName, "General", Path.GetFileName(filePath));
            }
        }

        private string GetTemplatePath(TemplateInfo templateInfo)
        {
            string categoryPath = Path.Combine(_templatesBasePath, templateInfo.Category);

            if (!Directory.Exists(categoryPath))
                categoryPath = _templatesBasePath;

            return Path.Combine(categoryPath, templateInfo.FileName);
        }

        private async Task<bool> CheckElementStateAsync(string elementName, RecognitionResult element, string expectedState)
        {
            await Task.Delay(100);

            var currentResult = await FindElementAsync(elementName, element.Confidence - 0.1);

            if (!currentResult.IsDetected)
                return false;

            switch (expectedState.ToLower())
            {
                case "clicked":
                    return true;

                case "selected":
                    return currentResult.Confidence > element.Confidence;

                default:
                    return currentResult.IsDetected;
            }
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _templates.Clear();
                _isDisposed = true;
            }
        }

        public async Task<bool> ValidateFolderElementsAsync(string folderName, int requiredMatches, double confidenceThreshold = 0.8)
        {
            if (string.IsNullOrEmpty(folderName))
                return false;

            string folderPath = Path.Combine(_templatesBasePath, folderName);

            if (!Directory.Exists(folderPath))
            {
                Console.WriteLine($"Папка не существует: {folderPath}");
                return false;
            }

            var pngFiles = Directory.GetFiles(folderPath, "*.png");
            if (pngFiles.Length == 0)
            {
                Console.WriteLine($"В папке нет PNG файлов: {folderPath}");
                return false;
            }

            Console.WriteLine($"Проверяем папку: {folderName}");
            Console.WriteLine($"Файлов в папке: {pngFiles.Length}");
            Console.WriteLine($"Требуется найти: {requiredMatches}");

            int foundCount = 0;

            using (var screenBitmap = _screenCapturer.CaptureScreen())
            using (var matcher = new TemplateMatcher(screenBitmap))
            {
                foreach (var filePath in pngFiles)
                {
                    var fileName = Path.GetFileNameWithoutExtension(filePath);
                    Console.WriteLine($"  Проверяем: {fileName}");

                    var result = matcher.FindTemplate(filePath, confidenceThreshold);

                    if (result.IsDetected)
                    {
                        foundCount++;
                        Console.WriteLine($"    Найден (уверенность: {result.Confidence})");

                        if (foundCount >= requiredMatches)
                        {
                            Console.WriteLine($"Достаточно элементов найдено: {foundCount}/{requiredMatches}");
                            return true;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"    Не найден");
                    }
                }
            }

            Console.WriteLine($"Найдено элементов: {foundCount}/{requiredMatches}");
            return foundCount >= requiredMatches;
        }
    }
}