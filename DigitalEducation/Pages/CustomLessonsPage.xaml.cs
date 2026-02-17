using DigitalEducation.Pages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DigitalEducation
{
    public partial class CustomLessonsPage : UserControl, IPage
    {
        private readonly string _customLessonsPath;
        private List<LessonData> _lessons = new List<LessonData>();
        private List<LessonData> _filteredLessons = new List<LessonData>();

        private readonly Dictionary<string, string> _lessonFilePaths = new Dictionary<string, string>();

        private readonly List<string> _sortOptions = new List<string>
        {
            "По названию (А-Я)",
            "По названию (Я-А)",
            "По дате (сначала новые)",
            "По дате (сначала старые)"
        };

        private string _currentSearchQuery = "";
        private string _currentSortOption = "По дате (сначала новые)";

        public CustomLessonsPage()
        {
            InitializeComponent();

            string projectRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..");
            _customLessonsPath = Path.GetFullPath(Path.Combine(projectRoot, "Lessons", "CustomLessons"));

            this.Loaded += CustomLessonsPage_Loaded;
            this.Unloaded += CustomLessonsPage_Unloaded;
            ThemeManager.ThemeChanged += OnThemeChanged;
        }

        private void CustomLessonsPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                InitializeSearchAndSort();
                LoadCustomLessons();
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
            UpdateCreateButtonIcon();
            UpdateEmptyStateIcon();

            UpdateSearchAndSortIcons();
        }

        private void UpdateCreateButtonIcon()
        {
            var stackPanel = CreateLessonButton?.Content as StackPanel;
            if (stackPanel != null && stackPanel.Children.Count > 0)
            {
                var icon = stackPanel.Children[0] as Image;
                if (icon != null)
                {
                    ThemeManager.UpdateImageSource(icon, "Document");
                }
            }
        }

        private void UpdateEmptyStateIcon()
        {
            if (EmptyStateIcon != null)
            {
                ThemeManager.UpdateImageSource(EmptyStateIcon, "Info");
            }
        }

        private void UpdateSearchAndSortIcons()
        {
            try
            {
                if (SortComboBox != null)
                {
                    var contentPresenter = FindVisualChild<ContentPresenter>(SortComboBox);
                    if (contentPresenter != null)
                    {
                        var stackPanel = VisualTreeHelper.GetChild(contentPresenter, 0) as StackPanel;
                        if (stackPanel != null && stackPanel.Children.Count > 0)
                        {
                            var icon = stackPanel.Children[0] as Image;
                            if (icon != null && icon.Tag != null)
                            {
                                ThemeManager.UpdateImageSource(icon, icon.Tag.ToString());
                            }
                        }
                    }

                    foreach (var item in SortComboBox.Items)
                    {
                        if (SortComboBox.ItemContainerGenerator.ContainerFromItem(item) is ComboBoxItem container)
                        {
                            var stackPanel = VisualTreeHelper.GetChild(container, 0) as StackPanel;
                            if (stackPanel != null && stackPanel.Children.Count > 0)
                            {
                                var icon = stackPanel.Children[0] as Image;
                                if (icon != null && icon.Tag != null)
                                {
                                    ThemeManager.UpdateImageSource(icon, icon.Tag.ToString());
                                }
                            }
                        }
                    }
                }

                if (SearchTextBox != null && SearchTextBox.Parent != null)
                {
                    var border = SearchTextBox.Parent as Border;
                    if (border != null)
                    {
                        var grid = VisualTreeHelper.GetChild(border, 0) as Grid;
                        if (grid != null && grid.Children.Count > 0)
                        {
                            var searchIcon = grid.Children[0] as Image;
                            if (searchIcon != null)
                            {
                                ThemeManager.UpdateImageSource(searchIcon, "Search");
                            }

                            if (grid.Children.Count > 2)
                            {
                                var clearButton = grid.Children[2] as Button;
                                if (clearButton?.Content is Image clearIcon)
                                {
                                    ThemeManager.UpdateImageSource(clearIcon, "Close");
                                }
                            }
                        }
                    }
                }

                if (NoSearchResultsBorder != null && NoSearchResultsBorder.Child is StackPanel noResultsStackPanel)
                {
                    if (noResultsStackPanel.Children.Count > 0 && noResultsStackPanel.Children[0] is Border iconBorder)
                    {
                        if (iconBorder.Child is Image noResultsIcon)
                        {
                            ThemeManager.UpdateImageSource(noResultsIcon, "Search");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обновлении иконок поиска и сортировки: {ex.Message}");
            }
        }

        private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T foundChild)
                {
                    return foundChild;
                }

                var childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                {
                    return childOfChild;
                }
            }

            return null;
        }

        private void InitializeSearchAndSort()
        {
            try
            {
                SortComboBox.ItemsSource = _sortOptions;
                SortComboBox.SelectedItem = _currentSortOption;

                if (SearchTextBox != null)
                {
                    SearchTextBox.Tag = "Поиск уроков по названию...";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка инициализации поиска и сортировки: {ex.Message}");
            }
        }

        private void LoadCustomLessons()
        {
            _lessons.Clear();
            _lessonFilePaths.Clear();

            if (!Directory.Exists(_customLessonsPath))
            {
                Directory.CreateDirectory(_customLessonsPath);
                return;
            }

            var jsonFiles = Directory.GetFiles(_customLessonsPath, "*.json");

            foreach (var filePath in jsonFiles)
            {
                try
                {
                    string jsonContent = File.ReadAllText(filePath, Encoding.UTF8);
                    var lesson = JsonSerializer.Deserialize<LessonData>(jsonContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (lesson != null)
                    {
                        if (string.IsNullOrEmpty(lesson.CourseId))
                        {
                            lesson.CourseId = "Custom";
                        }

                        _lessonFilePaths[lesson.Id] = filePath;

                        _lessons.Add(lesson);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка загрузки урока {filePath}: {ex.Message}");
                }
            }

            ApplySorting();
        }

        private void ApplySearch()
        {
            if (string.IsNullOrWhiteSpace(_currentSearchQuery))
            {
                _filteredLessons = new List<LessonData>(_lessons);
            }
            else
            {
                var query = _currentSearchQuery.ToLower();
                _filteredLessons = _lessons
                    .Where(lesson =>
                        (!string.IsNullOrEmpty(lesson.Title) &&
                         lesson.Title.ToLower().Contains(query)))
                    .ToList();
            }

            ApplySorting();
        }

        private void ApplySorting()
        {
            var lessonsToSort = _filteredLessons.Any() ? _filteredLessons : _lessons;

            switch (_currentSortOption)
            {
                case "По названию (А-Я)":
                    lessonsToSort = lessonsToSort
                        .OrderBy(l => l.Title ?? "")
                        .ToList();
                    break;

                case "По названию (Я-А)":
                    lessonsToSort = lessonsToSort
                        .OrderByDescending(l => l.Title ?? "")
                        .ToList();
                    break;

                case "По дате (сначала новые)":
                    lessonsToSort = lessonsToSort
                        .OrderByDescending(l => GetLessonLastModified(l.Id))
                        .ToList();
                    break;

                case "По дате (сначала старые)":
                    lessonsToSort = lessonsToSort
                        .OrderBy(l => GetLessonLastModified(l.Id))
                        .ToList();
                    break;
            }

            if (_filteredLessons.Any())
            {
                _filteredLessons = lessonsToSort;
            }
            else
            {
                _lessons = lessonsToSort;
            }
        }

        private DateTime GetLessonLastModified(string lessonId)
        {
            if (_lessonFilePaths.TryGetValue(lessonId, out string filePath) &&
                File.Exists(filePath))
            {
                return File.GetLastWriteTime(filePath);
            }

            return DateTime.MinValue;
        }

        private void UpdateLessonsDisplay()
        {
            LessonsContainer.Children.Clear();

            var lessonsToDisplay = _filteredLessons.Any() ? _filteredLessons : _lessons;
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

            foreach (var lesson in lessonsToDisplay)
            {
                var lessonCard = CreateLessonCard(lesson);
                LessonsContainer.Children.Add(lessonCard);
            }
        }

        private Border CreateLessonCard(LessonData lesson)
        {
            var card = new Border
            {
                Style = (Style)FindResource("CardStyle"),
                Margin = new Thickness(0, 0, 0, 24),
                UseLayoutRounding = true,
                SnapsToDevicePixels = true
            };

            var grid = new Grid
            {
                UseLayoutRounding = true
            };

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var numberBorder = new Border
            {
                Width = 80,
                Height = 80,
                CornerRadius = new CornerRadius(16),
                Background = new SolidColorBrush(GetLessonColor(lesson.Id)),
                Margin = new Thickness(0, 0, 24, 0),
                UseLayoutRounding = true,
                SnapsToDevicePixels = true
            };

            var lessonsList = _filteredLessons.Any() ? _filteredLessons : _lessons;
            var lessonIndex = lessonsList.IndexOf(lesson) + 1;

            var numberText = new TextBlock
            {
                Text = lessonIndex.ToString(),
                FontSize = 32,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            numberBorder.Child = numberText;
            Grid.SetColumn(numberBorder, 0);

            var stackPanel = new StackPanel
            {
                UseLayoutRounding = true
            };

            var titleText = new TextBlock
            {
                Text = string.IsNullOrEmpty(lesson.Title) ? "Без названия" : lesson.Title,
                FontSize = 16,
                FontWeight = FontWeights.Medium,
                Foreground = new SolidColorBrush(GetLessonColor(lesson.Id)),
                Margin = new Thickness(0, 0, 0, 8)
            };

            DateTime lastModified = GetLessonLastModified(lesson.Id);

            var dateText = new TextBlock
            {
                Text = $"Изменён: {lastModified:dd.MM.yyyy HH:mm}",
                Style = (Style)FindResource("BodyTextStyle"),
                Margin = new Thickness(0, 0, 0, 12),
                FontSize = 13
            };

            stackPanel.Children.Add(titleText);
            stackPanel.Children.Add(dateText);

            Grid.SetColumn(stackPanel, 1);

            var editButton = CreateActionButton("Редактировать", "Edit", () => EditLesson(lesson));
            Grid.SetColumn(editButton, 2);

            var deleteButton = CreateDeleteButton("Удалить", "Trash", () => DeleteLesson(lesson));
            Grid.SetColumn(deleteButton, 3);

            var startButton = CreateStartButton("Начать", () => StartLesson(lesson));
            Grid.SetColumn(startButton, 4);

            grid.Children.Add(numberBorder);
            grid.Children.Add(stackPanel);
            grid.Children.Add(editButton);
            grid.Children.Add(deleteButton);
            grid.Children.Add(startButton);

            card.Child = grid;
            return card;
        }

        private Button CreateActionButton(string text, string iconName, Action onClick)
        {
            var button = new Button
            {
                Style = (Style)FindResource("NavigationButtonStyle"),
                Height = 80,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(12, 0, 0, 0),
                UseLayoutRounding = true,
                Tag = iconName
            };

            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                UseLayoutRounding = true,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var icon = new Image
            {
                Tag = iconName,
                Width = 20,
                Height = 20,
                Margin = new Thickness(0, 0, 8, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            ThemeManager.UpdateImageSource(icon, iconName);

            var buttonText = new TextBlock
            {
                Text = text,
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.Medium,
                Foreground = (SolidColorBrush)FindResource("TextPrimaryBrush")
            };

            stackPanel.Children.Add(icon);
            stackPanel.Children.Add(buttonText);
            button.Content = stackPanel;

            button.Click += (s, e) => onClick();
            return button;
        }

        private Button CreateDeleteButton(string text, string iconName, Action onClick)
        {
            var button = new Button
            {
                Style = (Style)FindResource("NavigationButtonStyle"),
                Height = 80,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(12, 0, 0, 0),
                UseLayoutRounding = true,
                Tag = iconName,
                ToolTip = "Удалить урок"
            };

            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                UseLayoutRounding = true,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var icon = new Image
            {
                Tag = iconName,
                Width = 20,
                Height = 20,
                Margin = new Thickness(0, 0, 8, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            ThemeManager.UpdateImageSource(icon, iconName);

            var buttonText = new TextBlock
            {
                Text = text,
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.Medium,
                Foreground = (SolidColorBrush)FindResource("TextPrimaryBrush")
            };

            stackPanel.Children.Add(icon);
            stackPanel.Children.Add(buttonText);
            button.Content = stackPanel;

            button.Click += (s, e) => onClick();
            return button;
        }

        private Button CreateStartButton(string text, Action onClick)
        {
            var button = new Button
            {
                Style = (Style)FindResource("NavigationButtonStyle"),
                Height = 80,
                VerticalAlignment = VerticalAlignment.Center,
                Padding = new Thickness(24, 0, 0, 0),
                Margin = new Thickness(12, 0, 0, 0),
                UseLayoutRounding = true,
                Tag = "Right"
            };

            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                UseLayoutRounding = true,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var icon = new Image
            {
                Tag = "Right",
                Width = 24,
                Height = 24,
                Margin = new Thickness(0, 0, 8, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            ThemeManager.UpdateImageSource(icon, "Right");

            var buttonText = new TextBlock
            {
                Text = text,
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.Medium,
                Foreground = (SolidColorBrush)FindResource("TextPrimaryBrush")
            };

            stackPanel.Children.Add(icon);
            stackPanel.Children.Add(buttonText);
            button.Content = stackPanel;

            button.Click += (s, e) => onClick();
            return button;
        }

        private Color GetLessonColor(string lessonId)
        {
            Color[] paletteColors = new Color[]
            {
                (Color)FindResource("PrimaryColor"),
                (Color)FindResource("SuccessColor"),
                (Color)FindResource("WarningColor"),
                (Color)FindResource("ErrorColor")
            };

            int hash = Math.Abs(lessonId.GetHashCode());
            int colorIndex = hash % paletteColors.Length;

            return paletteColors[colorIndex];
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

        private void EditLesson(LessonData lesson)
        {
            try
            {
                if (Window.GetWindow(this) is MainWindow mainWindow)
                {
                    var editLessonPage = new CreateLessonPage(lesson.Id);
                    mainWindow.MainLayout.Content = editLessonPage;
                }
            }
            catch (Exception ex)
            {
                DialogService.ShowErrorDialog(
                    $"Ошибка при открытии редактора: {ex.Message}",
                    Window.GetWindow(this)
                );
            }
        }

        private void DeleteLesson(LessonData lesson)
        {
            try
            {
                var result = DialogService.ShowConfirmDialog(
                    "Удаление урока",
                    $"Вы уверены, что хотите удалить урок '{lesson.Title}'?\nЭто действие нельзя отменить.",
                    "Удалить",
                    "Отмена",
                    Window.GetWindow(this)
                );

                if (result == true)
                {
                    string lessonFilePath = Path.Combine(_customLessonsPath, $"{lesson.Id}.json");

                    if (File.Exists(lessonFilePath))
                    {
                        File.Delete(lessonFilePath);
                        Console.WriteLine($"Удален файл урока: {lessonFilePath}");
                    }

                    DeleteLessonImages(lesson.Id);

                    _lessonFilePaths.Remove(lesson.Id);

                    LoadCustomLessons();
                    ApplySearch();
                    UpdateLessonsDisplay();

                    DialogService.ShowSuccessDialog(
                        "Урок успешно удален!",
                        Window.GetWindow(this)
                    );
                }
            }
            catch (Exception ex)
            {
                DialogService.ShowErrorDialog(
                    $"Ошибка при удаления урока: {ex.Message}",
                    Window.GetWindow(this)
                );
            }
        }

        private void DeleteLessonImages(string lessonId)
        {
            try
            {
                string templatesPath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "..", "..",
                    "ComputerVision", "Templates");

                if (Directory.Exists(templatesPath))
                {
                    var pattern = $"{lessonId}_*.*";
                    var imageFiles = Directory.GetFiles(templatesPath, pattern);

                    foreach (var file in imageFiles)
                    {
                        try
                        {
                            File.Delete(file);
                            Console.WriteLine($"Удалено изображение: {Path.GetFileName(file)}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Не удалось удалить изображение {file}: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении изображений урока: {ex.Message}");
            }
        }

        private void StartLesson(LessonData lesson)
        {
            try
            {
                string lessonFilePath = Path.Combine(_customLessonsPath, $"{lesson.Id}.json");

                if (File.Exists(lessonFilePath))
                {
                    LaunchLesson(lesson.Id);
                }
                else
                {
                    DialogService.ShowMessageDialog(
                        "Урок недоступен",
                        "Файл урока не найден.",
                        "OK",
                        Window.GetWindow(this)
                    );
                }
            }
            catch (Exception ex)
            {
                DialogService.ShowErrorDialog(
                    $"Не удалось запустить урок: {ex.Message}",
                    Window.GetWindow(this)
                );
            }
        }

        private void LaunchLesson(string lessonId)
        {
            try
            {
                OverlayWindow lessonWindow = new OverlayWindow(lessonId);

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

                bool? result = lessonWindow.ShowDialog();

                if (result == true)
                {
                    LoadCustomLessons();
                    ApplySearch();
                    UpdateLessonsDisplay();
                }
            }
            catch (Exception ex)
            {
                DialogService.ShowErrorDialog(
                    $"Не удалось запустить урок: {ex.Message}",
                    Window.GetWindow(this)
                );
            }
        }
    }
}