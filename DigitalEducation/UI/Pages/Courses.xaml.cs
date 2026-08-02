using DigitalEducation.Core.Managers;
using DigitalEducation.Core.Models;
using DigitalEducation.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DigitalEducation.UI.Pages
{
    public partial class Courses : Window
    {
        private Base _baseLogic;
        private List<CourseData> _courses;
        private string _currentFilter = "all";
        private string _currentSearch = "";
        private FontSize _fontSize;
        private bool _isDataLoaded = false;
        private bool _isRendered = false;

        public Courses()
        {
            InitializeComponent();
            _baseLogic = new Base(this, "courses");

            App app = (App)Application.Current;
            _fontSize = app.GetFontSizeService();

            if (_fontSize != null)
            {
                _fontSize.FontSizeChanged += OnFontSizeChanged;
            }

            CourseLoader.Instance.LoadingCompleted += OnCoursesLoaded;

            if (CourseLoader.Instance.IsLoaded)
            {
                LoadCoursesData();
            }

            this.Loaded += OnPageLoaded;
        }

        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            if (_isDataLoaded && !_isRendered)
            {
                RenderCourses(_courses);
                _isRendered = true;
            }
        }

        private void OnCoursesLoaded(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (!_isDataLoaded)
                {
                    LoadCoursesData();

                    if (this.IsLoaded)
                    {
                        RenderCourses(_courses);
                        _isRendered = true;
                    }
                }
            });
        }

        private void LoadCoursesData()
        {
            if (_isDataLoaded) return;

            _courses = CourseLoader.Instance.GetAllCourses();
            UpdateStats();
            _isDataLoaded = true;
        }

        protected override void OnClosed(EventArgs e)
        {
            if (_fontSize != null)
            {
                _fontSize.FontSizeChanged -= OnFontSizeChanged;
            }

            CourseLoader.Instance.LoadingCompleted -= OnCoursesLoaded;
            this.Loaded -= OnPageLoaded;

            _baseLogic.Cleanup();
            base.OnClosed(e);
        }

        private void OnFontSizeChanged(object sender, double size)
        {
            if (_isDataLoaded && _isRendered)
            {
                ApplyFilters();
            }
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            if (!_isDataLoaded)
            {
                LoadCoursesData();
            }

            if (_isDataLoaded && !_isRendered)
            {
                RenderCourses(_courses);
                _isRendered = true;
            }
        }

        private void UpdateStats()
        {
            if (_courses == null) return;

            int total = _courses.Count;
            int inProgress = 0;
            int completed = 0;
            int notStarted = total - inProgress - completed;

            StatTotal.Text = total.ToString();
            StatInProgress.Text = inProgress.ToString();
            StatCompleted.Text = completed.ToString();
            StatNotStarted.Text = notStarted.ToString();
        }

        private void ApplyFilters()
        {
            if (_courses == null || !_isDataLoaded) return;

            IEnumerable<CourseData> filtered = _courses.AsEnumerable();

            if (_currentFilter != "all")
            {
            }

            if (!string.IsNullOrWhiteSpace(_currentSearch))
            {
                string search = _currentSearch.ToLower().Trim();
                filtered = filtered.Where(c =>
                    c.Title.ToLower().Contains(search) ||
                    c.Description.ToLower().Contains(search)
                );
            }

            RenderCourses(filtered.ToList());
        }

        private void RenderCourses(List<CourseData> courses)
        {
            CoursesList.Children.Clear();

            if (courses.Count == 0)
            {
                EmptyState.Visibility = Visibility.Visible;
                return;
            }

            EmptyState.Visibility = Visibility.Collapsed;

            foreach (CourseData course in courses)
            {
                Border card = CreateCourseCard(course);
                CoursesList.Children.Add(card);
            }
        }

        private Border CreateCourseCard(CourseData course)
        {
            double fontSize = _fontSize?.CurrentSize ?? 16;

            Border border = new Border
            {
                Background = (Brush)FindResource("BgSurfaceGlass"),
                BorderBrush = (Brush)FindResource("BorderSubtle"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(24, 28, 24, 28),
                Margin = new Thickness(0, 0, 0, 20)
            };

            Grid mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            DockPanel topDockPanel = new DockPanel
            {
                Margin = new Thickness(0, 0, 0, 16),
                LastChildFill = true
            };

            Border iconBorder = new Border
            {
                Width = 56,
                Height = 56,
                CornerRadius = new CornerRadius(12),
                Background = (Brush)FindResource("GradientPrimary"),
                Margin = new Thickness(0, 0, 20, 0),
                VerticalAlignment = VerticalAlignment.Top
            };

            Viewbox iconViewbox = new Viewbox { Margin = new Thickness(14) };
            iconViewbox.Child = CreateIconPath(course.Icon ?? "computer", Colors.White, 2);
            iconBorder.Child = iconViewbox;

            DockPanel.SetDock(iconBorder, Dock.Left);
            topDockPanel.Children.Add(iconBorder);

            StackPanel contentPanel = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Top
            };
            topDockPanel.Children.Add(contentPanel);

            StackPanel headerPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 4)
            };

            TextBlock titleText = new TextBlock
            {
                Text = course.Title,
                Style = (Style)FindResource("Heading4"),
                Foreground = (Brush)FindResource("TextPrimary"),
                Margin = new Thickness(0, 0, 12, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            headerPanel.Children.Add(titleText);

            Border statusBadge = new Border
            {
                Background = (Brush)FindResource("BgHover"),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(12, 2, 12, 2),
                VerticalAlignment = VerticalAlignment.Center
            };
            TextBlock statusText = new TextBlock
            {
                Text = "Новый",
                Foreground = (Brush)FindResource("TextTertiary"),
                FontSize = 12,
                FontWeight = FontWeights.SemiBold
            };
            statusBadge.Child = statusText;
            headerPanel.Children.Add(statusBadge);

            contentPanel.Children.Add(headerPanel);

            double maxDescWidth = Math.Max(350, 600 - (fontSize - 16) * 10);

            TextBlock descText = new TextBlock
            {
                Text = course.Description,
                Style = (Style)FindResource("BodyTextSmall"),
                Foreground = (Brush)FindResource("TextSecondary"),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 8),
                MaxWidth = maxDescWidth,
                TextTrimming = TextTrimming.CharacterEllipsis,
                ToolTip = course.Description,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            contentPanel.Children.Add(descText);

            double metaPanelWidth = Math.Max(350, 600 - (fontSize - 16) * 10);
            WrapPanel metaPanel = new WrapPanel
            {
                Margin = new Thickness(0, 4, 0, 0),
                MaxWidth = metaPanelWidth,
                HorizontalAlignment = HorizontalAlignment.Left
            };

            Border lessonsItem = CreateMetaItem(
                "M4 19.5A2.5 2.5 0 016.5 17H20M6.5 2H20v20H6.5A2.5 2.5 0 014 19.5v-15A2.5 2.5 0 016.5 2z",
                $"{course.Lessons?.Count ?? 0} уроков"
            );
            metaPanel.Children.Add(lessonsItem);

            Border levelItem = CreateMetaItem(
                "M12 2v4M12 22v-4M4 12H2M22 12h-2M19.07 4.93l-2.83 2.83M4.93 19.07l2.83-2.83M4.93 4.93l2.83 2.83M19.07 19.07l-2.83-2.83",
                course.Level ?? "Не указан"
            );
            metaPanel.Children.Add(levelItem);

            Border timeItem = CreateMetaItem(
                "M12 2v4M12 22v-4M4 12H2M22 12h-2M19.07 4.93l-2.83 2.83M4.93 19.07l2.83-2.83M4.93 4.93l2.83 2.83M19.07 19.07l-2.83-2.83",
                course.EstimatedTime ?? "Не указано"
            );
            metaPanel.Children.Add(timeItem);

            contentPanel.Children.Add(metaPanel);

            Grid.SetRow(topDockPanel, 0);
            mainGrid.Children.Add(topDockPanel);

            Border divider = new Border
            {
                Height = 1,
                Background = (Brush)FindResource("BorderSubtle"),
                Margin = new Thickness(0, 16, 0, 16)
            };
            Grid.SetRow(divider, 1);
            mainGrid.Children.Add(divider);

            Grid bottomPanel = new Grid();
            bottomPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            bottomPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            StackPanel progressPanel = new StackPanel { Margin = new Thickness(0, 0, 20, 0) };

            Grid progressHeader = new Grid();
            progressHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            progressHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            TextBlock progressLabel = new TextBlock
            {
                Text = "Прогресс",
                Style = (Style)FindResource("CaptionSmall"),
                Foreground = (Brush)FindResource("TextSecondary")
            };
            Grid.SetColumn(progressLabel, 0);
            progressHeader.Children.Add(progressLabel);

            TextBlock progressValue = new TextBlock
            {
                Text = $"0 из {course.Lessons?.Count ?? 0}",
                Style = (Style)FindResource("CaptionSmall"),
                Foreground = (Brush)FindResource("TextSecondary"),
                FontWeight = FontWeights.SemiBold,
                FontFamily = (FontFamily)FindResource("FontBodySemiBold")
            };
            Grid.SetColumn(progressValue, 1);
            progressHeader.Children.Add(progressValue);

            progressPanel.Children.Add(progressHeader);

            ProgressBar progressBar = new ProgressBar
            {
                Style = (Style)FindResource("ProgressBarLarge"),
                Value = 0,
                Maximum = 100,
                Height = 4,
                Margin = new Thickness(0, 4, 0, 0)
            };
            progressPanel.Children.Add(progressBar);

            Grid.SetColumn(progressPanel, 0);
            bottomPanel.Children.Add(progressPanel);

            Button actionButton = new Button
            {
                Content = "Начать обучение",
                Style = (Style)FindResource("PrimaryButtonSmall"),
                Tag = course.Id,
                MinHeight = 40,
                Padding = new Thickness(32, 8, 32, 8)
            };
            actionButton.Click += CourseButton_Click;

            Grid.SetColumn(actionButton, 1);
            bottomPanel.Children.Add(actionButton);

            Grid.SetRow(bottomPanel, 2);
            mainGrid.Children.Add(bottomPanel);

            border.Child = mainGrid;
            return border;
        }

        private Border CreateMetaItem(string iconData, string text)
        {
            Border border = new Border
            {
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(8, 4, 12, 4),
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

            System.Windows.Shapes.Path path = new System.Windows.Shapes.Path
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

        private FrameworkElement CreateIconPath(string iconName, Color color, double thickness)
        {
            System.Windows.Shapes.Path path = new System.Windows.Shapes.Path
            {
                Stroke = new SolidColorBrush(color),
                StrokeThickness = thickness,
                StrokeLineJoin = PenLineJoin.Round
            };

            string data = GetIconData(iconName);
            if (!string.IsNullOrEmpty(data))
            {
                path.Data = Geometry.Parse(data);
            }

            return path;
        }

        private string GetIconData(string iconName)
        {
            switch (iconName?.ToLower())
            {
                case "computer":
                    return "M4 19.5A2.5 2.5 0 016.5 17H20M6.5 2H20v20H6.5A2.5 2.5 0 014 19.5v-15A2.5 2.5 0 016.5 2z";
                case "folder":
                    return "M4 19.5A2.5 2.5 0 016.5 17H20M6.5 2H20v20H6.5A2.5 2.5 0 014 19.5v-15A2.5 2.5 0 016.5 2z M4 10h16";
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

        private void CourseButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button == null) return;

            string courseId = button.Tag?.ToString();
            if (string.IsNullOrEmpty(courseId)) return;

            CourseData course = _courses.FirstOrDefault(c => c.Id == courseId);
            if (course == null) return;

            Course coursePage = new Course(courseId);
            coursePage.Show();
            Close();
        }

        private void SearchInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            _currentSearch = SearchInput.Text;
            ApplyFilters();
        }

        private void Filter_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radio = sender as RadioButton;
            if (radio == null || !radio.IsChecked.Value) return;

            _currentFilter = radio.Tag?.ToString() ?? "all";
            ApplyFilters();
        }
    }
}