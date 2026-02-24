using System;
using System.Diagnostics;

namespace DigitalEducation
{
    public class DebugLessonLogger : ILessonLogger
    {
        private string GetTimestamp() => DateTime.Now.ToString("HH:mm:ss.fff");

        public void LogLessonStarted(string lessonId, string lessonTitle, int totalSteps)
        {
            Debug.WriteLine($"[{GetTimestamp()}] Урок начат: '{lessonTitle}' (ID: {lessonId}), шагов: {totalSteps}");
        }

        public void LogStepChanged(int stepIndex, string stepTitle, bool requiresVision)
        {
            Debug.WriteLine($"[{GetTimestamp()}] Переход на шаг {stepIndex + 1}: '{stepTitle}' (требуется зрение: {requiresVision})");
        }

        public void LogLessonCompleted(string lessonId, double timeSpentMinutes)
        {
            Debug.WriteLine($"[{GetTimestamp()}] Урок {lessonId} завершён. Затрачено минут: {timeSpentMinutes:F2}");
        }

        public void LogVisionCheckStarted(string target)
        {
            Debug.WriteLine($"[{GetTimestamp()}] Поиск элемента: '{target}'...");
        }

        public void LogVisionCheckSucceeded(string target, double confidence)
        {
            Debug.WriteLine($"[{GetTimestamp()}] Элемент '{target}' НАЙДЕН! Уверенность: {confidence:P}");
        }

        public void LogVisionCheckFailed(string target, double confidence)
        {
            Debug.WriteLine($"[{GetTimestamp()}] Элемент '{target}' НЕ НАЙДЕН (порог: {confidence:P})");
        }

        public void LogVisionFolderCheck(string folderName, int requiredMatches, int foundCount, bool success)
        {
            Debug.WriteLine($"[{GetTimestamp()}] Проверка папки '{folderName}': требуется {requiredMatches}, найдено {foundCount} — {(success ? "УСПЕХ" : "НЕУДАЧА")}");
        }

        public void LogInfo(string message)
        {
            Debug.WriteLine($"[{GetTimestamp()}] INFO: {message}");
        }

        public void LogError(string message, Exception ex = null)
        {
            if (ex == null)
                Debug.WriteLine($"[{GetTimestamp()}] ОШИБКА: {message}");
            else
                Debug.WriteLine($"[{GetTimestamp()}] ОШИБКА: {message} | Исключение: {ex.GetType().Name} - {ex.Message}");
        }
    }
}