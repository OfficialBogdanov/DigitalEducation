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

        public App()
        {
            _courseLoader = CourseLoader.Instance;
            _ = _courseLoader.LoadAllCoursesAsync();
            System.Diagnostics.Debug.WriteLine("[App] Загрузка курсов запущена в конструкторе");
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _settingsService = new AppSettings();

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

            int waitCount = 0;
            while (!_courseLoader.IsLoaded && waitCount < 30)
            {
                await Task.Delay(100);
                waitCount++;
            }

            if (_courseLoader.IsLoaded)
            {
                var count = _courseLoader.GetAllCourses().Count;
                System.Diagnostics.Debug.WriteLine($"[App] Загружено {count} курсов");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[App] Курсы не загружены, но продолжаем");
            }

            await AnimateProgress(splashWindow, 30, 60, 700);

            splashWindow.UpdateLoadingMessage("Подготовка данных");
            await AnimateProgress(splashWindow, 60, 85, 600);

            splashWindow.UpdateLoadingMessage("Загрузка приложения");
            await AnimateProgress(splashWindow, 85, 100, 500);

            double elapsedMs = (DateTime.Now - startTime).TotalMilliseconds;
            if (elapsedMs < 2500)
            {
                await Task.Delay((int)(2500 - elapsedMs));
            }

            await Task.Delay(300);

            UI.Pages.Main mainWindow = new UI.Pages.Main();
            mainWindow.Show();
            splashWindow.Close();
        }

        private async Task AnimateProgress(MainWindow window, int from, int to, int durationMs)
        {
            int steps = 30;

            for (int i = 0; i < steps; i++)
            {
                double t = (double)i / steps;
                double eased = 1 - Math.Pow(1 - t, 2);
                int currentValue = (int)(from + (to - from) * eased);
                window.UpdateLoadingProgress(currentValue);

                double delayMultiplier = 1 + 1.5 * Math.Pow(t, 2);
                int stepDelay = (int)(durationMs / steps * delayMultiplier);

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