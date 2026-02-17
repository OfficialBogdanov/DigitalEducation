using System;
using System.IO;
using DigitalEducation.ComputerVision.Services;

namespace DigitalEducation
{
    public interface IVisionServiceFactory
    {
        VisionService Create();
    }

    public class VisionServiceFactory : IVisionServiceFactory
    {
        public VisionService Create()
        {
            try
            {
                string templatesPath = GetTemplatesPath();
                if (!Directory.Exists(templatesPath))
                {
                    Directory.CreateDirectory(templatesPath);
                }
                return new VisionService(templatesPath);
            }
            catch
            {
                return null;
            }
        }

        private string GetTemplatesPath()
        {
            string projectRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..");
            return Path.GetFullPath(
                Path.Combine(projectRoot, "ComputerVision", "Templates"));
        }
    }
}