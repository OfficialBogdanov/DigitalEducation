using DigitalEducation.Core.Managers;
using DigitalEducation.Core.Models;
using DigitalEducation.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DigitalEducation.UI.Pages
{
    public partial class Course : Window
    {
        private Base _baseLogic;
        private CourseData _course;
        private string _courseId;
        private FontSize _fontSize;

        public Course(string courseId)
        {
            InitializeComponent();
            _baseLogic = new Base(this, "courses");
            _courseId = courseId;

            App app = (App)Application.Current;
            _fontSize = app.GetFontSizeService();

            if (_fontSize != null)
            {
                _fontSize.FontSizeChanged += OnFontSizeChanged;
            }

            Loaded += OnPageLoaded;
        }

        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            LoadCourseData();
        }

        private void LoadCourseData()
        {
            _course = CourseLoader.Instance.GetCourse(_courseId);

            if (_course == null)
            {
                _baseLogic.ShowModal(
                    title: "Ошибка",
                    message: $"Курс \"{_courseId}\" не найден",
                    confirmText: "OK",
                    cancelText: null,
                    onConfirm: () => _baseLogic.NavigateTo("courses")
                );
                return;
            }

            CourseTitle.Text = _course.Title;
            CourseDescription.Text = _course.Description;
            BreadcrumbTitle.Text = _course.Title;

            CourseLevel.Text = _course.Level ?? "Не указан";
            CourseTime.Text = _course.EstimatedTime ?? "Не указано";
            CourseLessonsCount.Text = $"{_course.Lessons?.Count ?? 0} уроков";

            SetIcon(_course.Icon);
            UpdateProgress();
            RenderLessons();
        }

        private void SetIcon(string iconName)
        {
            string data = GetIconData(iconName);
            if (!string.IsNullOrEmpty(data))
            {
                CourseIconPath.Data = Geometry.Parse(data);
            }
        }

        private string GetIconData(string iconName)
        {
            switch (iconName?.ToLower())
            {
                case "folder":
                    return "M4 19.5A2.5 2.5 0 016.5 17H20M6.5 2H20v20H6.5A2.5 2.5 0 014 19.5v-15A2.5 2.5 0 016.5 2z M4 10h16";
                case "computer":
                    return "M4 19.5A2.5 2.5 0 016.5 17H20M6.5 2H20v20H6.5A2.5 2.5 0 014 19.5v-15A2.5 2.5 0 016.5 2z";
                case "monitor":
                    return "M4 19.5A2.5 2.5 0 016.5 17H20M6.5 2H20v20H6.5A2.5 2.5 0 014 19.5v-15A2.5 2.5 0 016.5 2z M8 21h8 M12 17v4";
                case "shield":
                    return "M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z M9 12l2 2 4-4";
                case "ai":
                    return "M12 2a2 2 0 012 2c0 .74-.4 1.39-1 1.73V7h1a7 7 0 017 7h1a1 1 0 011 1v3a1 1 0 01-1 1h-1v1a2 2 0 01-2 2H5a2 2 0 01-2-2v-1H2a1 1 0 01-1-1v-3a1 1 0 011-1h1a7 7 0 017-7h1V5.73c-.6-.34-1-.99-1-1.73a2 2 0 012-2z M9 15v1 M15 15v1 M12 15v2";
                case "data":
                    return "M21 12v-2a5 5 0 00-5-5H8a5 5 0 00-5 5v2 M12 16a5 5 0 100-10 5 5 0 000 10z M12 11v5";
                case "code":
                    return "M12 2a15.3 15.3 0 014 10 15.3 15.3 0 01-4 10 15.3 15.3 0 01-4-10 15.3 15.3 0 014-10z M2 8h20 M2 16h20";
                case "marketing":
                    return "M12 2v4M12 18v4M4.93 4.93l2.83 2.83M16.24 16.24l2.83 2.83M2 12h4M18 12h4M4.93 19.07l2.83-2.83M16.24 7.76l2.83-2.83";
                case "cloud":
                    return "M17.5 19H9a7 7 0 110-14h2.5a7 7 0 016 10.5 M14.5 19H20a2 2 0 002-2v-2a2 2 0 00-2-2h-2.5";
                case "blockchain":
                    return "M12 12a10 10 0 100-20 10 10 0 000 20z M12 6v12 M8 10h8 M8 14h8";
                default:
                    return "M4 19.5A2.5 2.5 0 016.5 17H20M6.5 2H20v20H6.5A2.5 2.5 0 014 19.5v-15A2.5 2.5 0 016.5 2z";
            }
        }

        private void UpdateProgress()
        {
            if (_course?.Lessons == null) return;

            int total = _course.Lessons.Count;
            int completed = 0;
            int totalTime = 0;

            foreach (LessonData lesson in _course.Lessons)
            {
                totalTime += lesson.EstimatedMinutes;

                string[] mockCompleted = new[] { "FilesLesson1", "FilesLesson2", "FilesLesson3" };
                if (mockCompleted.Contains(lesson.Id))
                {
                    completed++;
                }
            }

            int percent = total > 0 ? (int)((double)completed / total * 100) : 0;

            ProgressStats.Text = $"{completed} из {total} уроков";
            CourseProgressBar.Value = percent;
            ProgressPercent.Text = $"{percent}%";
            ProgressTime.Text = $"{totalTime} мин";
            CourseProgressText.Text = $"{percent}% завершено";
        }

        private void RenderLessons()
        {
            LessonsList.Children.Clear();

            if (_course?.Lessons == null || _course.Lessons.Count == 0)
            {
                EmptyState.Visibility = Visibility.Visible;
                return;
            }

            EmptyState.Visibility = Visibility.Collapsed;

            List<LessonData> sortedLessons = _course.Lessons.OrderBy(l => l.Order).ToList();

            for (int i = 0; i < sortedLessons.Count; i++)
            {
                LessonData lesson = sortedLessons[i];
                Border card = CreateLessonCard(lesson, i + 1);
                LessonsList.Children.Add(card);
            }
        }

        private Border CreateLessonCard(LessonData lesson, int number)
        {
            Border border = new Border
            {
                Background = (Brush)FindResource("BgSurfaceGlass"),
                BorderBrush = (Brush)FindResource("BorderSubtle"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(20, 18, 20, 18),
                Margin = new Thickness(0, 0, 0, 12)
            };

            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            Border numberBorder = new Border
            {
                Width = 48,
                Height = 48,
                CornerRadius = new CornerRadius(12),
                Background = (Brush)FindResource("GradientPrimary"),
                Margin = new Thickness(0, 0, 16, 0),
                VerticalAlignment = VerticalAlignment.Top
            };
            TextBlock numberText = new TextBlock
            {
                Text = number.ToString(),
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            numberBorder.Child = numberText;
            Grid.SetColumn(numberBorder, 0);
            grid.Children.Add(numberBorder);

            StackPanel infoStack = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Top,
                MaxWidth = 450,
                HorizontalAlignment = HorizontalAlignment.Left
            };

            StackPanel titlePanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 4)
            };

            TextBlock titleText = new TextBlock
            {
                Text = lesson.Title,
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Foreground = (Brush)FindResource("TextPrimary"),
                TextTrimming = TextTrimming.CharacterEllipsis,
                VerticalAlignment = VerticalAlignment.Center,
                MaxWidth = 350
            };
            titlePanel.Children.Add(titleText);

            Border statusBorder = new Border
            {
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(10, 2, 10, 2),
                VerticalAlignment = VerticalAlignment.Center
            };
            TextBlock statusText = new TextBlock
            {
                Text = "Не пройден",
                FontSize = 11,
                FontWeight = FontWeights.SemiBold
            };

            string[] mockCompleted = new[] { "FilesLesson1", "FilesLesson2", "FilesLesson3" };
            bool isCompleted = mockCompleted.Contains(lesson.Id);

            if (isCompleted)
            {
                statusBorder.Background = new SolidColorBrush(Color.FromRgb(40, 167, 69));
                statusText.Text = "✓ Пройден";
                statusText.Foreground = Brushes.White;
            }
            else
            {
                statusBorder.Background = (Brush)FindResource("BgHover");
                statusText.Foreground = (Brush)FindResource("TextSecondary");
            }

            statusBorder.Child = statusText;
            titlePanel.Children.Add(statusBorder);
            infoStack.Children.Add(titlePanel);

            TextBlock descText = new TextBlock
            {
                Text = lesson.Description ?? "",
                FontSize = 13,
                Foreground = (Brush)FindResource("TextSecondary"),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 8),
                TextTrimming = TextTrimming.WordEllipsis,
                MaxWidth = 450,
                MaxHeight = 40
            };
            infoStack.Children.Add(descText);

            WrapPanel metaPanel = new WrapPanel();

            Border timeBorder = CreateMetaItem(
                "M12 2v4M12 22v-4M4 12H2M22 12h-2M19.07 4.93l-2.83 2.83M4.93 19.07l2.83-2.83M4.93 4.93l2.83 2.83M19.07 19.07l-2.83-2.83",
                $"{lesson.EstimatedMinutes} мин"
            );
            metaPanel.Children.Add(timeBorder);

            Border stepsBorder = CreateMetaItem(
                "M4 19.5A2.5 2.5 0 016.5 17H20M6.5 2H20v20H6.5A2.5 2.5 0 014 19.5v-15A2.5 2.5 0 016.5 2z",
                $"{lesson.Steps?.Count ?? 0} шагов"
            );
            metaPanel.Children.Add(stepsBorder);

            if (!string.IsNullOrEmpty(lesson.Difficulty))
            {
                Border diffBorder = CreateMetaItem(
                    "M12 2a2 2 0 012 2c0 .74-.4 1.39-1 1.73V7h1a7 7 0 017 7h1a1 1 0 011 1v3a1 1 0 01-1 1h-1v1a2 2 0 01-2 2H5a2 2 0 01-2-2v-1H2a1 1 0 01-1-1v-3a1 1 0 011-1h1a7 7 0 017-7h1V5.73c-.6-.34-1-.99-1-1.73a2 2 0 012-2z",
                    lesson.Difficulty
                );
                metaPanel.Children.Add(diffBorder);
            }

            infoStack.Children.Add(metaPanel);

            Grid.SetColumn(infoStack, 1);
            grid.Children.Add(infoStack);

            Button actionButton = new Button
            {
                Content = isCompleted ? "Повторить" : "Начать",
                Style = (Style)FindResource(isCompleted ? "SecondaryButtonSmall" : "PrimaryButtonSmall"),
                Tag = lesson.Id,
                MinHeight = 36,
                Padding = new Thickness(24, 6, 24, 6),
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 4, 0, 0)
            };
            actionButton.Click += LessonButton_Click;

            Grid.SetColumn(actionButton, 2);
            grid.Children.Add(actionButton);

            border.Child = grid;
            return border;
        }

        private Border CreateMetaItem(string iconData, string text)
        {
            Border border = new Border
            {
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(8, 2, 12, 2),
                Margin = new Thickness(0, 0, 12, 4)
            };

            StackPanel stack = new StackPanel { Orientation = Orientation.Horizontal };

            Viewbox iconViewbox = new Viewbox
            {
                Width = 14,
                Height = 14,
                Margin = new Thickness(0, 0, 6, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            Path path = new Path
            {
                Stroke = (Brush)FindResource("TextTertiary"),
                StrokeThickness = 1.5,
                StrokeLineJoin = PenLineJoin.Round,
                Data = Geometry.Parse(iconData)
            };
            iconViewbox.Child = path;
            stack.Children.Add(iconViewbox);

            TextBlock textBlock = new TextBlock
            {
                Text = text,
                Style = (Style)FindResource("CaptionSmall"),
                Foreground = (Brush)FindResource("TextSecondary"),
                VerticalAlignment = VerticalAlignment.Center
            };
            stack.Children.Add(textBlock);

            border.Child = stack;
            return border;
        }

        private void LessonButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button == null) return;

            string lessonId = button.Tag?.ToString();
            if (string.IsNullOrEmpty(lessonId)) return;

            LessonData lesson = _course?.Lessons?.FirstOrDefault(l => l.Id == lessonId);
            if (lesson == null) return;

            string action = button.Content?.ToString() == "Повторить" ? "повторение" : "начало";

            _baseLogic.ShowModal(
                title: $"{char.ToUpper(action[0]) + action.Substring(1)} урока",
                message: $"Вы собираетесь {action} урок \"{lesson.Title}\". Продолжить?",
                confirmText: "Продолжить",
                cancelText: "Отмена",
                onConfirm: () =>
                {
                    _baseLogic.ShowModal(
                        title: "Успешно",
                        message: $"Урок \"{lesson.Title}\" открыт",
                        confirmText: "OK",
                        cancelText: null
                    );
                }
            );
        }

        private void BackToCourses_Click(object sender, RoutedEventArgs e)
        {
            _baseLogic.NavigateTo("courses");
        }

        private void OnFontSizeChanged(object sender, double size)
        {
            if (IsLoaded && _course != null)
            {
                RenderLessons();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            if (_fontSize != null)
            {
                _fontSize.FontSizeChanged -= OnFontSizeChanged;
            }
            _baseLogic.Cleanup();
            base.OnClosed(e);
        }
    }
}