using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Interop;
using System.Runtime.InteropServices;

namespace DigitalEducation
{
    public partial class MainWindow : Window
    {
        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;
        private const int WS_MAXIMIZEBOX = 0x10000;
        private const int WS_MINIMIZEBOX = 0x20000;

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        public MainWindow()
        {
            string savedTheme = ThemeManager.GetCurrentTheme();
            ThemeManager.ApplyTheme(savedTheme);

            InitializeComponent();

            Loaded += OnMainWindowLoaded;
            SourceInitialized += OnSourceInitialized;
        }

        private void OnSourceInitialized(object sender, EventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            var style = GetWindowLong(hwnd, GWL_STYLE);
            SetWindowLong(hwnd, GWL_STYLE, style & ~WS_SYSMENU);
        }

        private void OnMainWindowLoaded(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Maximized;

            SetupNavigationButtons();
            LoadHomePage();
            MainLayout.SetActiveNavigation("Home");
        }

        private void SetupNavigationButtons()
        {
            if (MainLayout.FindName("btnHome") is Button btnHome)
                btnHome.Click += OnNavigationButtonClick;

            if (MainLayout.FindName("btnCourses") is Button btnCourses)
                btnCourses.Click += OnNavigationButtonClick;

            if (MainLayout.FindName("btnSettings") is Button btnSettings)
                btnSettings.Click += OnNavigationButtonClick;

            if (MainLayout.FindName("btnCloseApp") is Button btnCloseApp)
                btnCloseApp.Click += OnCloseAppButtonClick;
        }

        private void OnNavigationButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button clickedButton)
            {
                string buttonName = clickedButton.Name;

                if (MainLayout is MasterLayout layout)
                {
                    layout.HandleNavigationClick(clickedButton);
                }

                switch (buttonName)
                {
                    case "btnHome":
                        LoadHomePage();
                        break;
                    case "btnCourses":
                        LoadCoursesPage();
                        break;
                    case "btnSettings":
                        LoadSettingsPage();
                        break;
                }
            }
        }

        private void OnCloseAppButtonClick(object sender, RoutedEventArgs e)
        {
            var dialog = new ConfirmDialog
            {
                Title = "Выход",
                Message = "Вы действительно хотите выйти из приложения?",
                ConfirmButtonText = "Выход",
                CancelButtonText = "Отмена"
            };

            dialog.DialogResultChanged += (s, result) =>
            {
                if (result)
                {
                    Application.Current.Shutdown();
                }
            };

            ShowDialog(dialog, null);
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);

            e.Cancel = true;

            var dialog = new ConfirmDialog
            {
                Title = "Выход",
                Message = "Вы действительно хотите выйти из приложения?",
                ConfirmButtonText = "Выход",
                CancelButtonText = "Отмена"
            };

            dialog.DialogResultChanged += (s, result) =>
            {
                if (result)
                {
                    Application.Current.Shutdown();
                }
            };

            ShowDialog(dialog, null);
        }

        private void LoadHomePage()
        {
            var homePage = new HomePage();
            homePage.CategoryButtonClicked += OnCategoryButtonClicked;
            MainLayout.Content = homePage;
        }

        private void LoadCoursesPage()
        {
            var coursesPage = new CoursesPage();
            coursesPage.CourseButtonClicked += OnCourseButtonClicked;
            MainLayout.Content = coursesPage;
        }

        private void LoadSettingsPage()
        {
            var settingsPage = new SettingsPage();
            settingsPage.SettingsButtonClicked += OnSettingsButtonClicked;
            MainLayout.Content = settingsPage;
        }

        private void LoadFilesLessonsPage()
        {
            var filesLessonsPage = new FilesLessonsPage();
            MainLayout.Content = filesLessonsPage;
        }

        private void OnSettingsButtonClicked(object sender, string action)
        {
            if (action == "ProgressReset")
            {
                var currentPage = MainLayout.Content;
                if (currentPage is CoursesPage coursesPage)
                {
                    coursesPage.RefreshProgress();
                }
            }
            else if (action.StartsWith("ThemeChanged:"))
            {
                ReloadCurrentPage();

                if (MainLayout is MasterLayout layout)
                {
                    if (MainLayout.Content is HomePage)
                        layout.SetActiveNavigation("Home");
                    else if (MainLayout.Content is CoursesPage)
                        layout.SetActiveNavigation("Courses");
                    else if (MainLayout.Content is SettingsPage)
                        layout.SetActiveNavigation("Settings");
                }
            }
        }

        private void OnCategoryButtonClicked(object sender, string categoryName)
        {
            if (categoryName == "Files")
            {
                LoadFilesLessonsPage();
            }
        }

        private void OnCourseButtonClicked(object sender, string courseTag)
        {
            if (courseTag == "OpenFilesLessons")
            {
                LoadFilesLessonsPage();
            }
        }

        public void ReloadCurrentPage()
        {
            if (MainLayout.Content is HomePage)
            {
                LoadHomePage();
            }
            else if (MainLayout.Content is CoursesPage)
            {
                LoadCoursesPage();
            }
            else if (MainLayout.Content is SettingsPage)
            {
                LoadSettingsPage();
            }
            else if (MainLayout.Content is FilesLessonsPage)
            {
                LoadFilesLessonsPage();
            }
        }

        private string GetCourseName(string tag)
        {
            switch (tag)
            {
                case "Files":
                    return "Файлы и папки";
                case "System":
                    return "Операционная система";
                case "Office":
                    return "Офисные программы";
                case "Internet":
                    return "Интернет";
                default:
                    return "Неизвестный курс";
            }
        }

        public void ShowDialog(UIElement dialogContent, EventHandler<bool> resultHandler)
        {
            var dialogWindow = new Window
            {
                Owner = this,
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                Background = Brushes.Transparent,
                ShowInTaskbar = false,
                SizeToContent = SizeToContent.WidthAndHeight,
                ResizeMode = ResizeMode.NoResize,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            var border = new Border
            {
                Child = dialogContent,
                Background = Brushes.White,
                CornerRadius = new CornerRadius(12),
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(1),
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Colors.Black,
                    BlurRadius = 20,
                    ShadowDepth = 0,
                    Opacity = 0.3
                }
            };

            dialogWindow.Content = border;

            if (dialogContent is ConfirmDialog confirmDialog)
            {
                confirmDialog.DialogResultChanged += (s, result) =>
                {
                    dialogWindow.Close();
                    resultHandler?.Invoke(s, result);
                };
            }

            dialogWindow.ShowDialog();
        }

        public void HideDialog()
        {
            DialogPopup.IsOpen = false;
        }

        public void UpdateLessonCompletion(string lessonId, bool isCompleted)
        {
            if (MainLayout.Content is FilesLessonsPage filesPage)
            {
                filesPage.UpdateLessonStatus(lessonId, isCompleted);
            }

            Console.WriteLine($"Урок {lessonId} завершен: {isCompleted}");
        }
    }
}