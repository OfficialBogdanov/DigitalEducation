using System;
using System.Windows.Controls;

namespace DigitalEducation
{
    public class NavigationService
    {
        private readonly MasterLayout _mainLayout;
        private readonly IPageFactory _pageFactory;
        private IPage _currentPage;

        public IPage CurrentPage => _currentPage;

        public event EventHandler<string> CategoryAction;
        public event EventHandler<string> CourseAction;
        public event EventHandler<SettingsActionEventArgs> SettingsAction;

        public NavigationService(MasterLayout mainLayout) : this(mainLayout, new DefaultPageFactory()) { }

        public NavigationService(MasterLayout mainLayout, IPageFactory pageFactory)
        {
            _mainLayout = mainLayout;
            _pageFactory = pageFactory;
        }

        public void NavigateToHome()
        {
            NavigateToPage<HomePage>();
        }

        public void NavigateToCourses()
        {
            NavigateToPage<CoursesPage>();
        }

        public void NavigateToSettings()
        {
            NavigateToPage<SettingsPage>();
        }

        public void NavigateToFilesLessons()
        {
            NavigateToPage<FilesLessonsPage>();
        }

        public void NavigateToCustomLessons()
        {
            NavigateToPage<CustomLessonsPage>();
        }

        private void NavigateToPage<T>() where T : IPage, new()
        {
            UnsubscribeFromCurrentPage();
            var page = _pageFactory.CreatePage<T>();
            _currentPage = page;
            _mainLayout.Content = page as UserControl; 
            _mainLayout.SetActiveNavigation(GetNavigationKeyForPage<T>());

            SubscribeToPageEvents(page);
        }

        private string GetNavigationKeyForPage<T>() where T : IPage
        {
            if (typeof(T) == typeof(HomePage)) return "Home";
            if (typeof(T) == typeof(CoursesPage) || typeof(T) == typeof(FilesLessonsPage) || typeof(T) == typeof(CustomLessonsPage)) return "Courses";
            if (typeof(T) == typeof(SettingsPage)) return "Settings";
            return "";
        }

        public void ReloadCurrentPage()
        {
            if (_currentPage is HomePage)
                NavigateToHome();
            else if (_currentPage is CoursesPage)
                NavigateToCourses();
            else if (_currentPage is SettingsPage)
                NavigateToSettings();
            else if (_currentPage is FilesLessonsPage)
                NavigateToFilesLessons();
            else if (_currentPage is CustomLessonsPage)
                NavigateToCustomLessons();
        }

        private void UnsubscribeFromCurrentPage()
        {
            if (_currentPage is HomePage home)
                home.CategoryButtonClicked -= OnCategoryButtonClicked;
            else if (_currentPage is CoursesPage courses)
                courses.CourseButtonClicked -= OnCourseButtonClicked;
            else if (_currentPage is SettingsPage settings)
                settings.SettingsButtonClicked -= OnSettingsButtonClicked;
        }

        private void SubscribeToPageEvents(IPage page)
        {
            if (page is HomePage home)
                home.CategoryButtonClicked += OnCategoryButtonClicked;
            else if (page is CoursesPage courses)
                courses.CourseButtonClicked += OnCourseButtonClicked;
            else if (page is SettingsPage settings)
                settings.SettingsButtonClicked += OnSettingsButtonClicked;
        }

        private void OnCategoryButtonClicked(object sender, string categoryName)
        {
            if (categoryName == "Files")
                NavigateToFilesLessons();
            else if (categoryName == "Custom")
                NavigateToCustomLessons();
            else
                CategoryAction?.Invoke(this, categoryName);
        }

        private void OnCourseButtonClicked(object sender, string courseTag)
        {
            if (courseTag == "OpenFilesLessons")
                NavigateToFilesLessons();
            else if (courseTag == "Custom")
                NavigateToCustomLessons();
            else
                CourseAction?.Invoke(this, courseTag);
        }

        private void OnSettingsButtonClicked(object sender, string action)
        {
            SettingsAction?.Invoke(this, new SettingsActionEventArgs(action));
        }
    }

    public class SettingsActionEventArgs : EventArgs
    {
        public string Action { get; }
        public SettingsActionEventArgs(string action) => Action = action;
    }
}