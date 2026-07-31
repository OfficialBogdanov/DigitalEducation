using System.Collections.Generic;

namespace DigitalEducation.Core.Models
{
    public class AppSettingsModel
    {
        public string Theme { get; set; } = "Dark";
        public string Palette { get; set; } = "Default";
        public double FontSize { get; set; } = 16;
        public string LessonPosition { get; set; } = "top-right";
        public Dictionary<string, bool> Toggles { get; set; } = new Dictionary<string, bool>();
        public Dictionary<string, object> CustomSettings { get; set; } = new Dictionary<string, object>();
    }
}