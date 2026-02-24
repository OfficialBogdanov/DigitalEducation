using System;
using System.IO;
using DigitalEducation.ComputerVision.Services;

namespace DigitalEducation
{
    public interface IVisionServiceFactory
    {
        VisionService Create(ILessonLogger logger = null);
    }

    public class VisionFactory : IVisionServiceFactory
    {
        public VisionService Create(ILessonLogger logger = null)
        {
            try
            {
                string templatesPath = GetTemplatesPath();
                if (!Directory.Exists(templatesPath))
                {
                    Directory.CreateDirectory(templatesPath);
                }
                logger?.LogInfo($"Путь к шаблонам: {templatesPath}");
                return new VisionService(templatesPath, logger);
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
                Path.Combine(projectRoot, "Engine", "ComputerVision", "Templates"));
        }
    }
}