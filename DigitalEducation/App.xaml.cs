using DigitalEducation.Core.Managers;
using DigitalEducation.Core.Services;
using System.Windows;

namespace DigitalEducation
{
    public partial class App : Application
    {
        private AppSettings _settingsService;
        private FontSize _fontSize;
        private ICourseLoader _courseLoader;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _settingsService = new AppSettings();
            _courseLoader = new CourseLoader();

            Theme.Initialize(_settingsService);
            Palette.Initialize(_settingsService);
            _fontSize = new FontSize(_settingsService);
        }

        public AppSettings GetSettingsService()
        {
            return _settingsService;
        }

        public FontSize GetFontSizeService()
        {
            return _fontSize;
        }

        public ICourseLoader GetCourseLoader()
        {
            return _courseLoader;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _settingsService?.Save();
            base.OnExit(e);
        }
    }
}