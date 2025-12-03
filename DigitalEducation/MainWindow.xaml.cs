using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DigitalEducation
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += OnMainWindowLoaded;
        }

        private void OnMainWindowLoaded(object sender, RoutedEventArgs e)
        {
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

            if (MainLayout.FindName("btnProgress") is Button btnProgress)
                btnProgress.Click += OnNavigationButtonClick;

            if (MainLayout.FindName("btnSettings") is Button btnSettings)
                btnSettings.Click += OnNavigationButtonClick;
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
                    case "btnProgress":
                        LoadProgressPage();
                        break;
                    case "btnSettings":
                        LoadSettingsPage();
                        break;
                }
            }
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

        private void LoadProgressPage()
        {
            MainLayout.Content = CreateSimplePage("📈 Прогресс", "Здесь будет отображаться ваш прогресс обучения");
        }

        private void LoadSettingsPage()
        {
            var settingsPage = new SettingsPage();
            settingsPage.SettingsButtonClicked += OnSettingsButtonClicked;
            MainLayout.Content = settingsPage;
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
                else if (currentPage is HomePage homePage)
                {
                }
            }
        }

        private void OnCategoryButtonClicked(object sender, string categoryName)
        {
            if (categoryName == "Files")
            {
                LoadFilesLessonsPage();
            }
            else
            {
            }
        }

        private void OnCourseButtonClicked(object sender, string courseTag)
        {
            if (courseTag == "OpenFilesLessons")
            {
                LoadFilesLessonsPage();
            }
            else
            {
                string courseName = GetCourseName(courseTag);
            }
        }

        private void LoadFilesLessonsPage()
        {
            var filesLessonsPage = new FilesLessonsPage();
            MainLayout.Content = filesLessonsPage;
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

        private UIElement CreateSimplePage(string title, string description)
        {
            var stackPanel = new StackPanel
            {
                Margin = new Thickness(40)
            };

            var titleBlock = new TextBlock
            {
                Text = title,
                Style = (Style)Application.Current.FindResource("TitleTextStyle"),
                Foreground = (System.Windows.Media.Brush)Application.Current.FindResource("PrimaryDarkBrush")
            };

            var descBlock = new TextBlock
            {
                Text = description,
                Style = (Style)Application.Current.FindResource("BodyTextStyle"),
                Margin = new Thickness(0, 20, 0, 0)
            };

            stackPanel.Children.Add(titleBlock);
            stackPanel.Children.Add(descBlock);

            return new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = stackPanel
            };
        }

        private void ShowMessage(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void UpdateLessonCompletion(string lessonId, bool isCompleted)
        {
            Console.WriteLine($"Урок {lessonId} завершен: {isCompleted}");
        }
    }
}