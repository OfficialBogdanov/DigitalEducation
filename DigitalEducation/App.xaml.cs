using System;
using System.IO;
using System.Windows;

namespace DigitalEducation
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Существующий код инициализации тем
            ThemeManager.Initialize();

            string savedTheme = ThemeManager.GetCurrentTheme();
            if (!string.IsNullOrEmpty(savedTheme))
            {
                ApplyThemeOnStartup(savedTheme);
            }

            // НОВЫЙ КОД: Проверка папки шаблонов компьютерного зрения
            CheckVisionTemplatesFolder();

            base.OnStartup(e);
        }

        // НОВЫЙ МЕТОД: Проверка и создание папки для шаблонов
        private void CheckVisionTemplatesFolder()
        {
            string templatesPath = GetTemplatesPath();

            if (!Directory.Exists(templatesPath))
            {
                try
                {
                    // Создаем базовую структуру папок
                    Directory.CreateDirectory(templatesPath);
                    Directory.CreateDirectory(Path.Combine(templatesPath, "Desktop"));

                    // Можно показать сообщение (опционально)
                    /*
                    MessageBox.Show(
                        $"Создана папка для шаблонов: {templatesPath}\n" +
                        "Добавьте PNG файлы элементов интерфейса для работы системы компьютерного зрения.",
                        "Информация",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                    */
                }
                catch (Exception ex)
                {
                    // Можно залогировать ошибку, но не прерывать работу
                    Console.WriteLine($"Не удалось создать папку шаблонов: {ex.Message}");
                }
            }
        }

        // НОВЫЙ МЕТОД: Получение пути к папке шаблонов
        public static string GetTemplatesPath()
        {
            return Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "ComputerVision",
                "Templates"
            );
        }

        // Существующий метод без изменений
        private void ApplyThemeOnStartup(string themeName)
        {
            try
            {
                Resources.MergedDictionaries.Clear();

                ResourceDictionary newTheme = new ResourceDictionary();

                if (themeName == "Dark")
                {
                    newTheme.Source = new Uri("Themes/DarkTheme.xaml", UriKind.Relative);
                }
                else
                {
                    newTheme.Source = new Uri("Themes/LightTheme.xaml", UriKind.Relative);
                }

                Resources.MergedDictionaries.Add(newTheme);
            }
            catch (Exception ex)
            {
                // Обработка ошибок темы
            }
        }
    }
}