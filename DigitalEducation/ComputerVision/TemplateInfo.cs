using System.Drawing;

namespace DigitalEducation.ComputerVision.Services
{
    public class TemplateInfo
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public string FileName { get; set; }
        public Rectangle SearchArea { get; set; }
        public double ConfidenceThreshold { get; set; }
    }
}