using DigitalEducation.UI.Pages;
using System;
using System.Windows;

namespace DigitalEducation.Core.Managers
{
    public static class Navigation
    {
        private static void NavigateTo<T>(Window current) where T : Window, new()
        {
            try
            {
                T newWindow = new T();
                newWindow.Show();

                if (current != null && current.IsLoaded)
                {
                    current.Close();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Navigation] Ошибка: {ex.Message}");
            }
        }

        public static void NavigateToMain(Window current)
        {
            NavigateTo<Main>(current);
        }

        public static void NavigateToCourses(Window current)
        {
            NavigateTo<Courses>(current);
        }

        public static void NavigateToCourse(Window current, string courseId)
        {
            try
            {
                Course coursePage = new Course(courseId);
                coursePage.Show();

                if (current != null && current.IsLoaded)
                {
                    current.Close();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Navigation] Ошибка: {ex.Message}");
            }
        }

        public static void NavigateToSettings(Window current)
        {
            NavigateTo<Settings>(current);
        }

        public static void NavigateToAchievements(Window current)
        {
            NavigateTo<Achievements>(current);
        }

        public static void NavigateToConstructor(Window current)
        {
            System.Diagnostics.Debug.WriteLine("[Navigation] Конструктор еще не реализован");
        }

        public static void CloseAllWindows()
        {
            for (int i = Application.Current.Windows.Count - 1; i >= 0; i--)
            {
                Window window = Application.Current.Windows[i];
                if (window != Application.Current.MainWindow)
                {
                    window.Close();
                }
            }
        }
    }
}