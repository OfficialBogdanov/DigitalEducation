using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace DigitalEducation
{
    public partial class MainWindow : Window
    {
        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private NavigationService _navigationService;

        public MainWindow()
        {
            string savedTheme = ThemeManager.GetCurrentTheme();
            ThemeManager.ApplyTheme(savedTheme);

            InitializeComponent();

            _navigationService = new NavigationService(MainLayout);
            _navigationService.CategoryAction += OnCategoryAction;
            _navigationService.CourseAction += OnCourseAction;
            _navigationService.SettingsAction += OnSettingsAction;

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
            _navigationService.NavigateToHome();
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
                switch (clickedButton.Name)
                {
                    case "btnHome":
                        _navigationService.NavigateToHome();
                        break;
                    case "btnCourses":
                        _navigationService.NavigateToCourses();
                        break;
                    case "btnSettings":
                        _navigationService.NavigateToSettings();
                        break;
                }
            }
        }

        private void OnCloseAppButtonClick(object sender, RoutedEventArgs e)
        {
            if (ConfirmExit())
                Application.Current.Shutdown();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            if (ConfirmExit())
                Application.Current.Shutdown();
        }

        private bool ConfirmExit()
        {
            return DialogService.ShowConfirmDialog(
                "Выход",
                "Вы действительно хотите выйти из приложения?",
                "Выход",
                "Отмена",
                this) == true;
        }

        private void OnCategoryAction(object sender, string categoryName)
        {
        }

        private void OnCourseAction(object sender, string courseTag)
        {
        }

        private void OnSettingsAction(object sender, SettingsActionEventArgs e)
        {
            if (e.Action == "ProgressReset")
            {
                if (_navigationService.CurrentPage is CoursesPage coursesPage)
                {
                    coursesPage.RefreshProgress();
                }
            }
            else if (e.Action.StartsWith("ThemeChanged:"))
            {
                _navigationService.ReloadCurrentPage();
            }
        }

        public void UpdateLessonCompletion(string lessonId, bool isCompleted)
        {
            if (_navigationService.CurrentPage is FilesLessonsPage filesPage)
            {
                filesPage.UpdateLessonStatus(lessonId, isCompleted);
            }
        }

        public void LoadHomePage()
        {
            _navigationService?.NavigateToHome();
        }

        public void LoadCoursesPage()
        {
            _navigationService?.NavigateToCourses();
        }

        public void LoadSettingsPage()
        {
            _navigationService?.NavigateToSettings();
        }

        public void LoadFilesLessonsPage()
        {
            _navigationService?.NavigateToFilesLessons();
        }

        public void LoadCustomLessonsPage()
        {
            _navigationService?.NavigateToCustomLessons();
        }
    }
}