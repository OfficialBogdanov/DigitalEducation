using DigitalEducation.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DigitalEducation
{
    public partial class CustomLessonsPage : UserControl, IPage
    {
        private readonly ILessonProvider _lessonProvider;
        private readonly ILessonCardFactory _cardFactory;
        private readonly ILessonActionHandler _actionHandler;

        private List<LessonData> _allLessons = new List<LessonData>();
        private List<LessonData> _filteredLessons = new List<LessonData>();

        private readonly List<string> _sortOptions = new List<string>
        {
            "По названию (А-Я)",
            "По названию (Я-А)"
        };

        private string _currentSearchQuery = "";
        private string _currentSortOption = "По названию (А-Я)";

        public CustomLessonsPage() : this(
            new LessonProvider(),
            new LessonCardFactory(Application.Current.MainWindow),
            new CustomLessonActionHandler(Application.Current.MainWindow))
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
            ThemeManager.ThemeChanged += OnThemeChanged;
        }

        private void CustomLessonsPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                InitializeSearchAndSort();
                RefreshLessons();
                UpdateIcons();
                UpdateLessonsDisplay();
            }
            catch (Exception ex)
            {
                DialogService.ShowErrorDialog(
                    $"Ошибка загрузки уроков: {ex.Message}",
                    Window.GetWindow(this)
                );
            }
        }

        private void CustomLessonsPage_Unloaded(object sender, RoutedEventArgs e)
        {
            ThemeManager.ThemeChanged -= OnThemeChanged;
        }

        private void OnThemeChanged(object sender, string themeName)
        {
            UpdateIcons();
        }

        private void UpdateIcons()
        {
            ThemeManager.UpdateAllIconsInContainer(this);
        }

        private void InitializeSearchAndSort()
        {
            SortComboBox.ItemsSource = _sortOptions;
            SortComboBox.SelectedItem = _currentSortOption;
        }

        private void RefreshLessons()
        {
            _allLessons = _lessonProvider.GetLessons("Custom");
            ApplySearch();
        }

        private void ApplySearch()
        {
            _filteredLessons = _lessonProvider.Filter(_allLessons, _currentSearchQuery);
            ApplySorting();
        }

        private void ApplySorting()
        {
            var lessonsToSort = _filteredLessons.Any() ? _filteredLessons : _allLessons;
            var sorted = _lessonProvider.Sort(lessonsToSort, _currentSortOption);

            if (_filteredLessons.Any())
                _filteredLessons = sorted;
            else
                _allLessons = sorted;
        }

        private void UpdateLessonsDisplay()
        {
            LessonsContainer.Children.Clear();

            var lessonsToDisplay = _filteredLessons.Any() ? _filteredLessons : _allLessons;
            var hasSearchQuery = !string.IsNullOrWhiteSpace(_currentSearchQuery);

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
                        UpdateLessonsDisplay();
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

            Dispatcher.BeginInvoke(new Action(() =>
            {
                ApplySearch();
                UpdateLessonsDisplay();
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void ClearSearchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = "";
            _currentSearchQuery = "";
            ClearSearchButton.Visibility = Visibility.Collapsed;
            ApplySearch();
            UpdateLessonsDisplay();
            SearchTextBox.Focus();
        }

        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SortComboBox.SelectedItem != null)
            {
                _currentSortOption = SortComboBox.SelectedItem.ToString();
                ApplySorting();
                UpdateLessonsDisplay();
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