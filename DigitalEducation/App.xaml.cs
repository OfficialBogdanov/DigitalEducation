using DigitalEducation.Core.Managers;
using DigitalEducation.Core.Services;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace DigitalEducation
{
    public partial class App : Application
    {
        private AppSettings _settingsService;
        private FontSize _fontSize;
        private CourseLoader _courseLoader;

        public AppSettings SettingsService => _settingsService;
        public FontSize FontSizeService => _fontSize;
        public CourseLoader CourseLoader => _courseLoader;

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _settingsService = new AppSettings();
            _courseLoader = CourseLoader.Instance;

            _courseLoader.LoadingProgress += OnLoadingProgress;
            _courseLoader.LoadingCompleted += OnLoadingCompleted;

            Theme.Initialize(_settingsService);
            Palette.Initialize(_settingsService);
            _fontSize = new FontSize(_settingsService);

            MainWindow splashWindow = new MainWindow();
            splashWindow.Show();

            await Task.Delay(150);

            DateTime startTime = DateTime.Now;

            splashWindow.UpdateLoadingMessage("Загрузка ресурсов");
            await AnimateProgress(splashWindow, 0, 30, 900);

            splashWindow.UpdateLoadingMessage("Инициализация модулей");
            await _courseLoader.LoadAllCoursesAsync();
            await AnimateProgress(splashWindow, 30, 60, 700);

            splashWindow.UpdateLoadingMessage("Подготовка данных");
            await AnimateProgress(splashWindow, 60, 85, 600);

            splashWindow.UpdateLoadingMessage("Загрузка приложения");
            await AnimateProgress(splashWindow, 85, 100, 500);

            double elapsedMs = (DateTime.Now - startTime).TotalMilliseconds;
            int minimumSplashMs = 2500;

            if (elapsedMs < minimumSplashMs)
            {
                int remainingMs = (int)(minimumSplashMs - elapsedMs);
                await Task.Delay(remainingMs);
            }

            await Task.Delay(300);

            UI.Pages.Main mainWindow = new UI.Pages.Main();
            mainWindow.Show();

            splashWindow.Close();
        }

        private async Task AnimateProgress(MainWindow window, int from, int to, int durationMs)
        {
            int steps = 30;
            int stepDelay = durationMs / steps;
            double stepValue = (double)(to - from) / steps;

            for (int i = 0; i < steps; i++)
            {
                int currentValue = (int)(from + (i * stepValue));
                window.UpdateLoadingProgress(currentValue);
                await Task.Delay(stepDelay);
            }

            window.UpdateLoadingProgress(to);
        }

        private void OnLoadingProgress(object sender, string message)
        {
            if (Application.Current?.MainWindow is MainWindow splash)
            {
                splash.UpdateLoadingMessage(message);
            }
        }

        private void OnLoadingCompleted(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[App] Загрузка курсов завершена");
        }

        public AppSettings GetSettingsService() => _settingsService;
        public FontSize GetFontSizeService() => _fontSize;
        public CourseLoader GetCourseLoader() => _courseLoader;

        protected override void OnExit(ExitEventArgs e)
        {
            _settingsService?.Save();
            base.OnExit(e);
        }
    }
}