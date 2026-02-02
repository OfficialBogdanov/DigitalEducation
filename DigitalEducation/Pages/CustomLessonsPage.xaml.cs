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

            var descriptionText = new TextBlock
            {
                Text = GetLessonDescription(lesson),
                Style = (Style)FindResource("BodyTextStyle"),
                Margin = new Thickness(0, 0, 0, 12),
                TextWrapping = TextWrapping.Wrap
            };

            var statsGrid = new Grid
            {
                Margin = new Thickness(0, 0, 0, 16),
                UseLayoutRounding = true
            };

            statsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            statsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            string lessonFilePath = Path.Combine(_customLessonsPath, $"{lesson.Id}.json");
            DateTime lastModified = File.Exists(lessonFilePath) ?
                File.GetLastWriteTime(lessonFilePath) : DateTime.Now;

            var dateText = new TextBlock
            {
                Text = $"Изменён: {lastModified:dd.MM.yyyy HH:mm}",
                Style = (Style)FindResource("BodyTextStyle")
            };

            var stepsText = new TextBlock
            {
                Text = $"{lesson.Steps?.Count ?? 0} шагов",
                Foreground = new SolidColorBrush(GetLessonColor(lesson.Id)),
                FontWeight = FontWeights.Medium
            };

            Grid.SetColumn(stepsText, 1);
            statsGrid.Children.Add(dateText);
            statsGrid.Children.Add(stepsText);

            stackPanel.Children.Add(titleText);
            stackPanel.Children.Add(descriptionText);
            stackPanel.Children.Add(statsGrid);

            Grid.SetColumn(stackPanel, 1);

            var editButton = CreateActionButton("Редактировать", "Edit", () => EditLesson(lesson));
            Grid.SetColumn(editButton, 2);

            var deleteButton = CreateActionButton("Удалить", "Trash", () => DeleteLesson(lesson));
            Grid.SetColumn(deleteButton, 3);

            grid.Children.Add(numberBorder);
            grid.Children.Add(stackPanel);
            grid.Children.Add(editButton);
            grid.Children.Add(deleteButton);

            card.Child = grid;
            return card;
        }

        private string GetLessonDescription(LessonData lesson)
        {
            if (string.IsNullOrEmpty(lesson.CompletionMessage))
                return "Урок без описания";

            string description = lesson.CompletionMessage;
            if (description.Length > 150)
                return description.Substring(0, 150) + "...";

            return description;
        }

        private Button CreateActionButton(string text, string iconName, Action onClick)
        {
            var button = new Button
            {
                Style = (Style)FindResource("NavigationButtonStyle"),
                Height = 60,
                Width = 140,
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

        private Color GetLessonColor(string lessonId)
        {
            int hash = lessonId.GetHashCode();
            byte r = (byte)((hash & 0xFF0000) >> 16);
            byte g = (byte)((hash & 0x00FF00) >> 8);
            byte b = (byte)(hash & 0x0000FF);

            return Color.FromRgb(
                (byte)((r + 100) % 256),
                (byte)((g + 150) % 256),
                (byte)((b + 200) % 256)
            );
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
            DialogService.ShowMessageDialog(
            "Редактирование урока",
            $"Редактирование урока: \nРедактор уроков в разработке.",
            "OK",
            Window.GetWindow(this)
            );
        }

        private void DeleteLesson(LessonData lesson)
        {
            var result = DialogService.ShowConfirmDialog(
            "Подтверждение удаления",
            $"Вы уверены, что хотите удалить урок?",
            "Удалить",
            "Отмена",
            Window.GetWindow(this)
            );

            if (result == true)
            {
                try
                {
                    string lessonFilePath = Path.Combine(_customLessonsPath, $"{lesson.Id}.json");

                    if (File.Exists(lessonFilePath))
                    {
                        File.Delete(lessonFilePath);

                        string imagesPath = Path.Combine(_customLessonsPath, "Templates", "VisionTargets", lesson.Id);
                        if (Directory.Exists(imagesPath))
                        {
                            Directory.Delete(imagesPath, true);
                        }

                        LoadCustomLessons();
                        UpdateLessonsDisplay();

                        DialogService.ShowSuccessDialog(
                            $"Урок успешно удален",
                            Window.GetWindow(this)
                        );
                    }
                }
                catch (Exception ex)
                {
                    DialogService.ShowErrorDialog(
                    $"Не удалось удалить урок: {ex.Message}",
                    Window.GetWindow(this)
                    );
                }
            }
        }
    }
}