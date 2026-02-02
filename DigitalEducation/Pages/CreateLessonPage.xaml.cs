using DigitalEducation.Pages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace DigitalEducation.Pages
{
    public partial class CreateLessonPage : UserControl
    {
        private readonly List<LessonStep> _steps = new List<LessonStep>();
        private string _selectedImagePath = "";
        private readonly string _customLessonsPath;
        private readonly string _templatesPath;
        private Border _addStepBlock;
        private bool _isMousePressed = false;

        public CreateLessonPage()
        {
            InitializeComponent();

            string projectRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..");
            _customLessonsPath = Path.GetFullPath(Path.Combine(projectRoot, "Lessons", "CustomLessons"));
            _templatesPath = Path.GetFullPath(Path.Combine(_customLessonsPath, "Templates", "VisionTargets"));

            Directory.CreateDirectory(_customLessonsPath);
            Directory.CreateDirectory(_templatesPath);

            this.Loaded += CreateLessonPage_Loaded;
            this.Unloaded += CreateLessonPage_Unloaded;
        }

        private void CreateLessonPage_Loaded(object sender, RoutedEventArgs e)
        {
            ThemeManager.ThemeChanged += OnThemeChanged;
            ThemeManager.UpdateAllIconsInContainer(this);

            _addStepBlock = FindAddStepBlock();

            if (_addStepBlock != null)
            {
                _addStepBlock.MouseLeftButtonDown += AddStepBlock_MouseLeftButtonDown;
                _addStepBlock.MouseLeftButtonUp += AddStepBlock_MouseLeftButtonUp;
                _addStepBlock.MouseEnter += AddStepBlock_MouseEnter;
                _addStepBlock.MouseLeave += AddStepBlock_MouseLeave;
            }

            UpdateStepsDisplay();
        }

        private void CreateLessonPage_Unloaded(object sender, RoutedEventArgs e)
        {
            ThemeManager.ThemeChanged -= OnThemeChanged;

            if (_addStepBlock != null)
            {
                _addStepBlock.MouseLeftButtonDown -= AddStepBlock_MouseLeftButtonDown;
                _addStepBlock.MouseLeftButtonUp -= AddStepBlock_MouseLeftButtonUp;
                _addStepBlock.MouseEnter -= AddStepBlock_MouseEnter;
                _addStepBlock.MouseLeave -= AddStepBlock_MouseLeave;
            }
        }

        private void OnThemeChanged(object sender, string themeName)
        {
            ThemeManager.UpdateAllIconsInContainer(this);
        }

        private Border FindAddStepBlock()
        {
            return this.FindName("AddStepBlock") as Border;
        }

        private T FindVisualChild<T>(DependencyObject parent, Func<T, bool> predicate) where T : DependencyObject
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T t && predicate(t))
                {
                    return t;
                }

                var result = FindVisualChild(child, predicate);
                if (result != null) return result;
            }

            return null;
        }

        private void AddStepBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isMousePressed = true;
            if (_addStepBlock != null)
            {
                _addStepBlock.Background = (Brush)FindResource("PressedBrush");
            }
        }

        private void AddStepBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isMousePressed)
            {
                _isMousePressed = false;

                AddNewStep();

                if (_addStepBlock != null)
                {
                    _addStepBlock.Background = (Brush)FindResource("BackgroundBrush");
                }
            }
        }

        private void AddStepBlock_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!_isMousePressed && _addStepBlock != null)
            {
                _addStepBlock.Background = (Brush)FindResource("HoverBrush");
            }
        }

        private void AddStepBlock_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!_isMousePressed && _addStepBlock != null)
            {
                _addStepBlock.Background = (Brush)FindResource("BackgroundBrush");
            }
            _isMousePressed = false;
        }

        private void AddNewStep()
        {
            var step = new LessonStep
            {
                Title = $"Шаг {_steps.Count + 1}",
                Description = "",
                Hint = "",
                VisionTarget = "",
                VisionConfidence = 0.85,
                RequiresVisionValidation = false,
                VisionTargetFolder = "",
                RequiredMatches = 1,
                ShowHint = true,
                HintConfidence = 0.8
            };

            _steps.Add(step);
            UpdateStepsDisplay();
        }

        private void UpdateStepsDisplay()
        {
            StepsContainer.Children.Clear();

            for (int i = 0; i < _steps.Count; i++)
            {
                var step = _steps[i];
                var stepCard = CreateStepCard(step, i + 1);
                StepsContainer.Children.Add(stepCard);
            }
        }

        private Border CreateStepCard(LessonStep step, int stepNumber)
        {
            var card = new Border
            {
                Style = (Style)FindResource("CardStyle"),
                Margin = new Thickness(0, 0, 0, 32),
                UseLayoutRounding = true,
                SnapsToDevicePixels = true
            };

            var mainStack = new StackPanel
            {
                UseLayoutRounding = true,
                Margin = new Thickness(24)
            };

            var titleGrid = new Grid
            {
                UseLayoutRounding = true,
                Margin = new Thickness(0, 0, 0, 24)
            };

            titleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            titleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            titleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var iconBorder = new Border
            {
                Width = 48,
                Height = 48,
                Background = (Brush)FindResource("CustomBrush"),
                CornerRadius = new CornerRadius(12),
                Margin = new Thickness(0, 0, 16, 0),
                UseLayoutRounding = true,
                SnapsToDevicePixels = true
            };

            var iconText = new TextBlock
            {
                Text = stepNumber.ToString(),
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            iconBorder.Child = iconText;
            Grid.SetColumn(iconBorder, 0);

            var titleStack = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center
            };

            var stepTitle = new TextBlock
            {
                Text = step.Title,
                Style = (Style)FindResource("SubtitleTextStyle"),
                Foreground = (Brush)FindResource("CustomBrush"),
                Margin = new Thickness(0, 0, 0, 4)
            };

            var stepSubtitle = new TextBlock
            {
                Text = "Заполните информацию о практическом шаге",
                Style = (Style)FindResource("BodyTextStyle"),
                Foreground = (Brush)FindResource("TextSecondaryBrush")
            };

            titleStack.Children.Add(stepTitle);
            titleStack.Children.Add(stepSubtitle);
            Grid.SetColumn(titleStack, 1);

            var deleteButton = new Button
            {
                Style = (Style)FindResource("NavigationButtonStyle"),
                Height = 40,
                Width = 40,
                Padding = new Thickness(0),
                VerticalAlignment = VerticalAlignment.Center,
                Tag = "Trash",
                ToolTip = "Удалить шаг"
            };

            var deleteIcon = new Image
            {
                Tag = "Trash",
                Width = 20,
                Height = 20,
                VerticalAlignment = VerticalAlignment.Center
            };

            ThemeManager.UpdateImageSource(deleteIcon, "Trash");
            deleteButton.Content = deleteIcon;

            deleteButton.Click += (s, e) =>
            {
                _steps.Remove(step);
                UpdateStepsDisplay();
            };

            Grid.SetColumn(deleteButton, 2);

            titleGrid.Children.Add(iconBorder);
            titleGrid.Children.Add(titleStack);
            titleGrid.Children.Add(deleteButton);

            var contentStack = new StackPanel
            {
                UseLayoutRounding = true
            };

            var descriptionPanel = new StackPanel
            {
                Margin = new Thickness(0, 0, 0, 20)
            };

            var descriptionHeader = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 8)
            };

            var descriptionIcon = new Image
            {
                Tag = "Info",
                Width = 16,
                Height = 16,
                Margin = new Thickness(0, 0, 8, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            ThemeManager.UpdateImageSource(descriptionIcon, "Info");

            var descriptionLabel = new TextBlock
            {
                Text = "Описание шага *",
                Style = (Style)FindResource("BodyTextStyle"),
                FontWeight = FontWeights.Medium
            };

            descriptionHeader.Children.Add(descriptionIcon);
            descriptionHeader.Children.Add(descriptionLabel);

            var descriptionBox = new TextBox
            {
                Text = step.Description,
                Style = (Style)FindResource("RoundedMultiLineTextBox"),
                Height = 120,
                MaxLength = 1000
            };

            descriptionBox.TextChanged += (s, e) =>
            {
                step.Description = descriptionBox.Text;
            };

            descriptionPanel.Children.Add(descriptionHeader);
            descriptionPanel.Children.Add(descriptionBox);

            var hintPanel = new StackPanel
            {
                Margin = new Thickness(0, 0, 0, 20)
            };

            var hintHeader = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 8)
            };

            var hintIcon = new Image
            {
                Tag = "Info",
                Width = 16,
                Height = 16,
                Margin = new Thickness(0, 0, 8, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            ThemeManager.UpdateImageSource(hintIcon, "Info");

            var hintLabel = new TextBlock
            {
                Text = "Подсказка (необязательно)",
                Style = (Style)FindResource("BodyTextStyle"),
                FontWeight = FontWeights.Medium
            };

            hintHeader.Children.Add(hintIcon);
            hintHeader.Children.Add(hintLabel);

            var hintBox = new TextBox
            {
                Text = step.Hint,
                Style = (Style)FindResource("RoundedMultiLineTextBox"),
                Height = 100,
                MaxLength = 500
            };

            hintBox.TextChanged += (s, e) =>
            {
                step.Hint = hintBox.Text;
            };

            hintPanel.Children.Add(hintHeader);
            hintPanel.Children.Add(hintBox);

            var imagePanel = new StackPanel
            {
                Margin = new Thickness(0, 0, 0, 0)
            };

            var imageHeader = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 8)
            };

            var imageIcon = new Image
            {
                Tag = "Folder",
                Width = 16,
                Height = 16,
                Margin = new Thickness(0, 0, 8, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            ThemeManager.UpdateImageSource(imageIcon, "Folder");

            var imageLabel = new TextBlock
            {
                Text = "Изображение для проверки (необязательно)",
                Style = (Style)FindResource("BodyTextStyle"),
                FontWeight = FontWeights.Medium
            };

            imageHeader.Children.Add(imageIcon);
            imageHeader.Children.Add(imageLabel);

            var fileContainer = new Grid
            {
            };

            fileContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            fileContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var fileInfoBorder = new Border
            {
                Background = (Brush)FindResource("BackgroundLightBrush"),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(16),
                BorderBrush = (Brush)FindResource("SurfaceBorderBrush"),
                BorderThickness = new Thickness(1)
            };

            var fileInfoText = new TextBlock
            {
                Text = string.IsNullOrEmpty(step.VisionTarget) ? "Файл не выбран" : Path.GetFileName(step.VisionTarget),
                Style = (Style)FindResource("BodyTextStyle"),
                Foreground = (Brush)FindResource("TextSecondaryBrush"),
                VerticalAlignment = VerticalAlignment.Center
            };

            fileInfoBorder.Child = fileInfoText;
            Grid.SetColumn(fileInfoBorder, 0);

            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(12, 0, 0, 0)
            };
            Grid.SetColumn(buttonPanel, 1);

            var clearImageButton = new Button
            {
                Style = (Style)FindResource("NavigationButtonStyle"),
                Height = 40,
                Margin = new Thickness(0, 0, 8, 0),
                Visibility = string.IsNullOrEmpty(step.VisionTarget) ? Visibility.Collapsed : Visibility.Visible
            };

            var clearIcon = new Image
            {
                Tag = "Trash",
                Width = 16,
                Height = 16,
                Margin = new Thickness(0, 0, 6, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            ThemeManager.UpdateImageSource(clearIcon, "Trash");

            var clearText = new TextBlock
            {
                Text = "Очистить",
                FontSize = 14,
                VerticalAlignment = VerticalAlignment.Center
            };

            var clearStack = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };
            clearStack.Children.Add(clearIcon);
            clearStack.Children.Add(clearText);
            clearImageButton.Content = clearStack;

            clearImageButton.Click += (s, e) =>
            {
                step.VisionTarget = "";
                step.RequiresVisionValidation = false;
                fileInfoText.Text = "Файл не выбран";
                clearImageButton.Visibility = Visibility.Collapsed;
            };

            var selectImageButton = new Button
            {
                Style = (Style)FindResource("NavigationButtonStyle"),
                Height = 40
            };

            var selectIcon = new Image
            {
                Tag = "Folder",
                Width = 16,
                Height = 16,
                Margin = new Thickness(0, 0, 6, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            ThemeManager.UpdateImageSource(selectIcon, "Folder");

            var selectText = new TextBlock
            {
                Text = "Выбрать файл",
                FontSize = 14,
                VerticalAlignment = VerticalAlignment.Center
            };

            var selectStack = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };
            selectStack.Children.Add(selectIcon);
            selectStack.Children.Add(selectText);
            selectImageButton.Content = selectStack;

            selectImageButton.Click += (s, e) =>
            {
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "Изображения (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp|Все файлы (*.*)|*.*",
                    Title = "Выберите изображение для проверки"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    step.VisionTarget = openFileDialog.FileName;
                    step.RequiresVisionValidation = true;
                    fileInfoText.Text = Path.GetFileName(step.VisionTarget);
                    clearImageButton.Visibility = Visibility.Visible;
                }
            };

            buttonPanel.Children.Add(clearImageButton);
            buttonPanel.Children.Add(selectImageButton);

            fileContainer.Children.Add(fileInfoBorder);
            fileContainer.Children.Add(buttonPanel);

            imagePanel.Children.Add(imageHeader);
            imagePanel.Children.Add(fileContainer);

            contentStack.Children.Add(descriptionPanel);
            contentStack.Children.Add(hintPanel);
            contentStack.Children.Add(imagePanel);

            mainStack.Children.Add(titleGrid);
            mainStack.Children.Add(contentStack);

            card.Child = mainStack;
            return card;
        }

        private bool ValidateLesson()
        {
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
            {
                TitleErrorText.Visibility = Visibility.Visible;
                isValid = false;
            }
            else
            {
                TitleErrorText.Visibility = Visibility.Collapsed;
            }

            if (string.IsNullOrWhiteSpace(CompletionMessageTextBox.Text))
            {
                CompletionMessageErrorText.Visibility = Visibility.Visible;
                isValid = false;
            }
            else
            {
                CompletionMessageErrorText.Visibility = Visibility.Collapsed;
            }

            if (_steps.Count == 0)
            {
                GeneralErrorText.Text = "Пожалуйста, добавьте хотя бы один шаг";
                GeneralErrorText.Visibility = Visibility.Visible;
                isValid = false;
            }
            else
            {
                GeneralErrorText.Visibility = Visibility.Collapsed;
            }

            foreach (var step in _steps)
            {
                if (string.IsNullOrWhiteSpace(step.Description))
                {
                    GeneralErrorText.Text = "Пожалуйста, заполните описание во всех шагах";
                    GeneralErrorText.Visibility = Visibility.Visible;
                    isValid = false;
                    break;
                }
                else
                {
                    GeneralErrorText.Visibility = Visibility.Collapsed;
                }
            }

            return isValid;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateLesson())
            {
                return;
            }

            var result = DialogService.ShowConfirmDialog(
                "Сохранение урока",
                "Вы уверены, что хотите сохранить урок?",
                "Сохранить",
                "Отмена",
                Window.GetWindow(this)
            );

            if (result == true)
            {
                SaveLessonToFile();
            }
        }

        private void SaveLessonToFile()
        {
            try
            {
                var lesson = new LessonData
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = TitleTextBox.Text.Trim(),
                    CourseId = "Custom",
                    Steps = _steps,
                    CompletionMessage = CompletionMessageTextBox.Text.Trim()
                };

                string lessonImagesPath = Path.Combine(_templatesPath, lesson.Id);
                Directory.CreateDirectory(lessonImagesPath);

                foreach (var step in lesson.Steps)
                {
                    if (!string.IsNullOrEmpty(step.VisionTarget) && File.Exists(step.VisionTarget))
                    {
                        string fileName = Path.GetFileName(step.VisionTarget);
                        string destPath = Path.Combine(lessonImagesPath, fileName);
                        File.Copy(step.VisionTarget, destPath, true);
                        step.VisionTarget = fileName;
                    }
                }

                string filePath = Path.Combine(_customLessonsPath, $"{lesson.Id}.json");
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNameCaseInsensitive = true,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                };

                string json = JsonSerializer.Serialize(lesson, options);
                File.WriteAllText(filePath, json, Encoding.UTF8);

                System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(LessonManager).TypeHandle);

                DialogService.ShowSuccessDialog(
                    "Урок успешно сохранен!",
                    Window.GetWindow(this)
                );

                if (Window.GetWindow(this) is MainWindow mainWindow)
                {
                    mainWindow.LoadCustomLessonsPage();
                }
            }
            catch (Exception ex)
            {
                DialogService.ShowErrorDialog(
                    $"Ошибка при сохранении урока: {ex.Message}",
                    Window.GetWindow(this)
                );
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            var result = DialogService.ShowConfirmDialog(
                "Отмена создания",
                "Вы уверены, что хотите отменить создание урока? Все несохраненные данные будут потеряны.",
                "Отменить",
                "Продолжить",
                Window.GetWindow(this)
            );

            if (result == true)
            {
                if (Window.GetWindow(this) is MainWindow mainWindow)
                {
                    mainWindow.LoadCustomLessonsPage();
                }
            }
        }
    }
}