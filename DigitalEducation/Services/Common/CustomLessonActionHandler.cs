using DigitalEducation.Pages;
using DigitalEducation.Pages.CreateCustomLesson;
using System;
using System.IO;
using System.Windows;

namespace DigitalEducation
{
    public class CustomLessonActionHandler : ILessonActionHandler
    {
        private readonly Window _ownerWindow;
        private readonly ILessonStorage _storage;
        private readonly string _customLessonsPath;

        public CustomLessonActionHandler(Window ownerWindow) : this(ownerWindow, new LessonFileStorage())
        {
        }

        public CustomLessonActionHandler(Window ownerWindow, ILessonStorage storage)
        {
            _ownerWindow = ownerWindow;
            _storage = storage;

            string projectRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..");
            _customLessonsPath = Path.GetFullPath(Path.Combine(projectRoot, "Lessons", "CustomLessons"));
        }

        public void EditLesson(LessonData lesson)
        {
            try
            {
                if (_ownerWindow is MainWindow mainWindow)
                {
                    var editLessonPage = new CreateLessonPage(lesson.Id);
                    mainWindow.MainLayout.Content = editLessonPage;
                }
            }
            catch (Exception ex)
            {
                DialogService.ShowErrorDialog($"Ошибка при открытии редактора: {ex.Message}", _ownerWindow);
            }
        }

        public void DeleteLesson(LessonData lesson)
        {
            try
            {
                var result = DialogService.ShowConfirmDialog(
                    "Удаление урока",
                    $"Вы уверены, что хотите удалить урок '{lesson.Title}'?\nЭто действие нельзя отменить.",
                    "Удалить",
                    "Отмена",
                    _ownerWindow
                );

                if (result == true)
                {
                    string lessonFilePath = Path.Combine(_customLessonsPath, $"{lesson.Id}.json");

                    if (File.Exists(lessonFilePath))
                    {
                        File.Delete(lessonFilePath);
                    }

                    DeleteLessonImages(lesson.Id);

                    DialogService.ShowSuccessDialog("Урок успешно удален!", _ownerWindow);

                    LessonManager.ReloadAllLessons();
                }
            }
            catch (Exception ex)
            {
                DialogService.ShowErrorDialog($"Ошибка при удалении урока: {ex.Message}", _ownerWindow);
            }
        }

        public void StartLesson(LessonData lesson)
        {
            try
            {
                OverlayWindow lessonWindow = new OverlayWindow(lesson.Id);

                Window mainWindow = Application.Current.MainWindow;
                if (mainWindow != null && mainWindow.IsLoaded)
                {
                    lessonWindow.Owner = mainWindow;
                    lessonWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                }
                else
                {
                    lessonWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                }

                lessonWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                DialogService.ShowErrorDialog($"Не удалось запустить урок: {ex.Message}", _ownerWindow);
            }
        }

        private void DeleteLessonImages(string lessonId)
        {
            try
            {
                string templatesPath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "..", "..",
                    "Engine", "ComputerVision", "Templates");

                if (Directory.Exists(templatesPath))
                {
                    var pattern = $"{lessonId}_*.*";
                    var imageFiles = Directory.GetFiles(templatesPath, pattern);

                    foreach (var file in imageFiles)
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch { }
                    }
                }
            }
            catch { }
        }
    }
}