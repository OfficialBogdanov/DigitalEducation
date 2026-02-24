using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace DigitalEducation
{
    public class UserProgressFileRepository : IProgressRepository
    {
        private readonly string _filePath;
        private readonly JsonSerializerOptions _jsonOptions;

        public UserProgressFileRepository()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appFolder = Path.Combine(appDataPath, "DigitalEducation");
            if (!Directory.Exists(appFolder))
                Directory.CreateDirectory(appFolder);
            _filePath = Path.Combine(appFolder, "user_progress.json");
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };
        }

        public UserProgressModel Load()
        {
            if (!File.Exists(_filePath))
                return CreateDefaultProgress();

            string json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<UserProgressModel>(json, _jsonOptions) ?? CreateDefaultProgress();
        }

        public void Save(UserProgressModel progress)
        {
            progress.LastUpdated = DateTime.UtcNow;
            string json = JsonSerializer.Serialize(progress, _jsonOptions);
            File.WriteAllText(_filePath, json);
        }

        public string GetFilePath() => _filePath;

        private UserProgressModel CreateDefaultProgress()
        {
            return new UserProgressModel
            {
                LastUpdated = DateTime.UtcNow,
                CompletedLessons = new List<CompletedLesson>(),
                Statistics = new UserStatistics(),
                CourseProgress = new Dictionary<string, CourseProgress>()
            };
        }
    }
}