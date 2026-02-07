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
    public partial class CustomLessonsPage : UserControl
    {
        private readonly string _customLessonsPath;
        private List<LessonData> _lessons = new List<LessonData>();

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

        private void LoadCustomLessons()
        {
            _lessons.Clear();

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

                        _lessons.Add(lesson);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка загрузки урока {filePath}: {ex.Message}");
                }
            }

            _lessons = _lessons.OrderByDescending(l =>
                File.GetLastWriteTime(Path.Combine(_customLessonsPath, $"{l.Id}.json"))).ToList();
        }

        private void UpdateLessonsDisplay()
        {
            LessonsContainer.Children.Clear();

            if (_lessons.Count == 0)
            {
                EmptyStateBorder.Visibility = Visibility.Visible;
                return;
            }

            EmptyStateBorder.Visibility = Visibility.Collapsed;

            foreach (var lesson in _lessons)
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

            var numberText = new TextBlock
            {
                Text = (_lessons.IndexOf(lesson) + 1).ToString(),
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

            string lessonFilePath = Path.Combine(_customLessonsPath, $"{lesson.Id}.json");
            DateTime lastModified = File.Exists(lessonFilePath) ?
                File.GetLastWriteTime(lessonFilePath) : DateTime.Now;

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

                    LoadCustomLessons();
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
                    $"Ошибка при удалении урока: {ex.Message}",
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
                    UpdateLessonsDisplay();

                    DialogService.ShowSuccessDialog(
                        "Урок успешно завершен!",
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
    }
}