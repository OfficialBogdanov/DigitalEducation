using System;
using System.IO;
using DigitalEducation.ComputerVision.Services;

namespace DigitalEducation
{
    public interface IVisionServiceFactory
    {
        ComputerVisionService Create(ILessonLogger logger = null);
    }

    public class VisionServiceFactory : IVisionServiceFactory
    {
        public ComputerVisionService Create(ILessonLogger logger = null)
        {
            try
            {
                string templatesPath = GetTemplatesPath();
                if (!Directory.Exists(templatesPath))
                {
                    Directory.CreateDirectory(templatesPath);
                }
                logger?.LogInfo($"Путь к шаблонам: {templatesPath}");
                return new ComputerVisionService(templatesPath, logger);
            }
            catch (Exception ex)
            {
                logger?.LogError("Ошибка создания VisionService", ex);
                return null;
            }
        }

        private string GetTemplatesPath()
        {
            string projectRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..");
            return Path.GetFullPath(
                Path.Combine(projectRoot, "Learning", "Engine", "Templates"));
        }
    }
}