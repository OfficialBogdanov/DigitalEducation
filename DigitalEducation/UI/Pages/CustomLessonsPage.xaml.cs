using DigitalEducation.Pages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DigitalEducation
{
    public partial class CustomLessonsPage : UserControl, IPage
    {
        private readonly ILessonProvider _lessonProvider;
        private readonly ILessonCardFactory _cardFactory;
        private readonly ILessonActionHandler _actionHandler;

        private List<LessonDataModel> _allLessons = new List<LessonDataModel>();
        private List<LessonDataModel> _displayedLessons = new List<LessonDataModel>();
        private Dictionary<string, DateTime> _lessonCreationDates = new Dictionary<string, DateTime>();

        private readonly List<string> _sortOptions = new List<string>
        {
            "По названию (А-Я)",
            "По названию (Я-А)",
            "По дате (Новые)",
            "По дате (Старые)"
        };

        private string _currentSearchQuery = "";
        private string _currentSortOption = "По названию (А-Я)";

        public CustomLessonsPage() : this(
            new LessonDataProvider(),
            new CustomLessonCardFactory(Application.Current.MainWindow),
            new CustomLessonActionsHandler(Application.Current.MainWindow))
        {
        }

        public CustomLessonsPage(
            ILessonProvider lessonProvider,
            ILessonCardFactory cardFactory,
            ILessonActionHandler actionHandler)
        {
            InitializeComponent();
            _lessonProvider = lessonProvider;
            _cardFactory = cardFactory;
            _actionHandler = actionHandler;

            this.Loaded += CustomLessonsPage_Loaded;
            this.Unloaded += CustomLessonsPage_Unloaded;
            AppThemeManager.ThemeChanged += OnThemeChanged;
        }

        private void CustomLessonsPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                InitializeSearchAndSort();
                RefreshLessons();
                UpdateIcons();
            }
            catch (Exception ex)
            {
                AppDialogService.ShowErrorDialog($"Ошибка загрузки уроков: {ex.Message}", Window.GetWindow(this));
            }
        }

        private void CustomLessonsPage_Unloaded(object sender, RoutedEventArgs e)
        {
            AppThemeManager.ThemeChanged -= OnThemeChanged;
        }

        private void OnThemeChanged(object sender, string themeName)
        {
            UpdateIcons();
        }

        private void UpdateIcons()
        {
            AppThemeManager.UpdateAllIconsInContainer(this);
        }

        private void InitializeSearchAndSort()
        {
            SortComboBox.ItemsSource = _sortOptions;
            SortComboBox.SelectedItem = _currentSortOption;
        }

        private void RefreshLessons()
        {
            _allLessons = _lessonProvider.GetLessons("Custom");
            _lessonCreationDates.Clear();

            string projectRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..");
            string customLessonsPath = Path.GetFullPath(Path.Combine(projectRoot, "Lessons", "CustomLessons"));

            foreach (var lesson in _allLessons)
            {
                string filePath = Path.Combine(customLessonsPath, $"{lesson.Id}.json");
                if (File.Exists(filePath))
                {
                    _lessonCreationDates[lesson.Id] = File.GetCreationTime(filePath);
                }
                else
                {
                    _lessonCreationDates[lesson.Id] = DateTime.MinValue;
                }
            }

            RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            var filtered = _lessonProvider.Filter(_allLessons, _currentSearchQuery);
            _displayedLessons = _lessonProvider.Sort(filtered, _currentSortOption, _lessonCreationDates);
            UpdateLessonsDisplay();
        }

        private void UpdateLessonsDisplay()
        {
            LessonsContainer.Children.Clear();

            var hasSearchQuery = !string.IsNullOrWhiteSpace(_currentSearchQuery);
            var lessonsToDisplay = _displayedLessons;

            if (hasSearchQuery)
            {
                if (lessonsToDisplay.Count > 0)
                {
                    SearchResultsText.Text = $"Найдено уроков: {lessonsToDisplay.Count}";
                    SearchResultsText.Visibility = Visibility.Visible;
                    NoSearchResultsBorder.Visibility = Visibility.Collapsed;
                    EmptyStateBorder.Visibility = Visibility.Collapsed;
                }
                else
                {
                    SearchResultsText.Visibility = Visibility.Collapsed;
                    NoSearchResultsBorder.Visibility = Visibility.Visible;
                    EmptyStateBorder.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                SearchResultsText.Visibility = Visibility.Collapsed;
                NoSearchResultsBorder.Visibility = Visibility.Collapsed;

                if (lessonsToDisplay.Count == 0)
                {
                    EmptyStateBorder.Visibility = Visibility.Visible;
                    return;
                }
                else
                {
                    EmptyStateBorder.Visibility = Visibility.Collapsed;
                }
            }

            for (int i = 0; i < lessonsToDisplay.Count; i++)
            {
                var lesson = lessonsToDisplay[i];
                int number = i + 1;

                var card = _cardFactory.CreateLessonCard(
                    lesson,
                    number,
                    onEdit: (l) => _actionHandler.EditLesson(l),
                    onDelete: (l) =>
                    {
                        _actionHandler.DeleteLesson(l);
                        RefreshLessons();
                    },
                    onStart: (l) => _actionHandler.StartLesson(l)
                );

                LessonsContainer.Children.Add(card);
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _currentSearchQuery = SearchTextBox.Text.Trim();
            ClearSearchButton.Visibility = string.IsNullOrEmpty(_currentSearchQuery)
                ? Visibility.Collapsed
                : Visibility.Visible;

            Dispatcher.BeginInvoke(new Action(RefreshDisplay), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void ClearSearchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = "";
            _currentSearchQuery = "";
            ClearSearchButton.Visibility = Visibility.Collapsed;
            RefreshDisplay();
            SearchTextBox.Focus();
        }

        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SortComboBox.SelectedItem != null)
            {
                _currentSortOption = SortComboBox.SelectedItem.ToString();
                RefreshDisplay();
            }
        }

        private void CreateLessonButton_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow mainWindow)
            {
                var createLessonPage = new CreateLessonPage();
                mainWindow.MainLayout.Content = createLessonPage;
            }
        }
    }
}