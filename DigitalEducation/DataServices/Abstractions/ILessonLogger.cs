using System;

namespace DigitalEducation
{
    public interface ILessonLogger
    {
        void LogLessonStarted(string lessonId, string lessonTitle, int totalSteps);
        void LogStepChanged(int stepIndex, string stepTitle, bool requiresVision);
        void LogLessonCompleted(string lessonId, double timeSpentMinutes);
        void LogVisionCheckStarted(string target);
        void LogVisionCheckSucceeded(string target, double confidence);
        void LogVisionCheckFailed(string target, double confidence);
        void LogVisionFolderCheck(string folderName, int requiredMatches, int foundCount, bool success);
        void LogInfo(string message);
        void LogError(string message, Exception ex = null);
    }
}