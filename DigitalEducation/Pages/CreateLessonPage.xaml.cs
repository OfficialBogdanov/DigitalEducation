using DigitalEducation.Pages;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DigitalEducation.Pages
{
    public partial class CreateLessonPage : UserControl
    {
        private readonly List<LessonStep> _steps = new List<LessonStep>();
        private readonly string _customLessonsPath;
        private readonly string _templatesPath;
        private Border _addStepBlock;
        private bool _isMousePressed = false;

        private string _editingLessonId;
        private bool _isEditMode = false;
        private LessonData _originalLesson;
        private Dictionary<int, string> _originalStepImages = new Dictionary<int, string>();

        public CreateLessonPage(string lessonId = null)
        {
            InitializeComponent();

            string projectRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..");
            _customLessonsPath = Path.GetFullPath(Path.Combine(projectRoot, "Lessons", "CustomLessons"));
            _templatesPath = Path.GetFullPath(Path.Combine(_customLessonsPath, "Templates", "VisionTargets"));

            Directory.CreateDirectory(_customLessonsPath);
            Directory.CreateDirectory(_templatesPath);
            if (!string.IsNullOrEmpty(lessonId))
            {
                _isEditMode = true;
                _editingLessonId = lessonId;
            }

            this.Loaded += CreateLessonPage_Loaded;
            this.Unloaded += CreateLessonPage_Unloaded;
        }

        private void CreateLessonPage_Loaded(object sender, RoutedEventArgs e)
        {
            ThemeManager.ThemeChanged += OnThemeChanged;
            ThemeManager.UpdateAllIconsInContainer(this);

            _addStepBlock = this.FindName("AddStepBlock") as Border;

            if (_addStepBlock != null)
            {
                _addStepBlock.MouseLeftButtonDown += AddStepBlock_MouseLeftButtonDown;
                _addStepBlock.MouseLeftButtonUp += AddStepBlock_MouseLeftButtonUp;
                _addStepBlock.MouseEnter += AddStepBlock_MouseEnter;
                _addStepBlock.MouseLeave += AddStepBlock_MouseLeave;
            }

            ConfigureUIForMode();

            if (_isEditMode)
            {
                LoadLessonForEditing();
            }
            else
            {
                UpdateStepsDisplay();
            }
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

        private void ConfigureUIForMode()
        {
            if (_isEditMode)
            {
                var saveButtonText = SaveButton.Content as StackPanel;
                if (saveButtonText != null && saveButtonText.Children.Count > 1)
                {
                    var textBlock = saveButtonText.Children[1] as TextBlock;
                    if (textBlock != null)
                    {
                        textBlock.Text = "Обновить урок";
                    }
                }

                var titleGrid = MainGrid.Children[0] as ScrollViewer;
                if (titleGrid != null)
                {
                    var stackPanel = titleGrid.Content as StackPanel;
                    if (stackPanel != null)
                    {
                        var grid = stackPanel.Children[0] as Grid;
                        if (grid != null)
                        {
                            var titleStack = grid.Children[1] as StackPanel;
                            if (titleStack != null)
                            {
                                var titleText = titleStack.Children[0] as TextBlock;
                                if (titleText != null)
                                {
                                    titleText.Text = "Редактирование урока";
                                }
                            }
                        }
                    }
                }
            }
        }

        private void LoadLessonForEditing()
        {
            try
            {
                string lessonFilePath = Path.Combine(_customLessonsPath, $"{_editingLessonId}.json");

                if (!File.Exists(lessonFilePath))
                {
                    DialogService.ShowErrorDialog("Файл урока не найден", Window.GetWindow(this));
                    ReturnToLessonsPage();
                    return;
                }

                string jsonContent = File.ReadAllText(lessonFilePath, Encoding.UTF8);
                _originalLesson = JsonSerializer.Deserialize<LessonData>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (_originalLesson != null)
                {
                    TitleTextBox.Text = _originalLesson.Title;
                    CompletionMessageTextBox.Text = _originalLesson.CompletionMessage ?? "";

                    if (_originalLesson.Steps != null)
                    {
                        _steps.Clear();
                        _originalStepImages.Clear();

                        int stepIndex = 0;
                        foreach (var step in _originalLesson.Steps)
                        {
                            _steps.Add(new LessonStep
                            {
                                Title = step.Title,
                                Description = step.Description,
                                Hint = step.Hint,
                                VisionTarget = step.VisionTarget,
                                VisionTargetFolder = step.VisionTargetFolder,
                                RequiresVisionValidation = step.RequiresVisionValidation,
                                VisionConfidence = step.VisionConfidence,
                                RequiredMatches = step.RequiredMatches
                            });

                            if (!string.IsNullOrEmpty(step.VisionTarget))
                            {
                                _originalStepImages[stepIndex] = step.VisionTarget;
                            }

                            stepIndex++;
                        }
                    }

                    UpdateStepsDisplay();
                }
                else
                {
                    DialogService.ShowErrorDialog("Не удалось загрузить данные урока", Window.GetWindow(this));
                    ReturnToLessonsPage();
                }
            }
            catch (Exception ex)
            {
                DialogService.ShowErrorDialog($"Ошибка загрузки урока: {ex.Message}", Window.GetWindow(this));
                ReturnToLessonsPage();
            }
        }

        private void ReturnToLessonsPage()
        {
            if (Window.GetWindow(this) is MainWindow mainWindow)
            {
                mainWindow.LoadCustomLessonsPage();
            }
        }

        private void OnThemeChanged(object sender, string themeName)
        {
            ThemeManager.UpdateAllIconsInContainer(this);
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

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateLesson())
            {
                return;
            }

            string dialogTitle = _isEditMode ? "Обновление урока" : "Сохранение урока";
            string dialogMessage = _isEditMode
                ? "Вы уверены, что хотите обновить урок?"
                : "Вы уверены, что хотите сохранить урок?";
            string confirmButton = _isEditMode ? "Обновить" : "Сохранить";

            var result = DialogService.ShowConfirmDialog(
                dialogTitle,
                dialogMessage,
                confirmButton,
                "Отмена",
                Window.GetWindow(this)
            );

            if (result == true)
            {
                if (_isEditMode)
                    UpdateLessonFile();
                else
                    SaveLessonToFile();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            string dialogTitle = _isEditMode ? "Отмена редактирования" : "Отмена создания";
            string dialogMessage = _isEditMode
                ? "Вы уверены, что хотите отменить редактирование урока? Все несохраненные изменения будут потеряны."
                : "Вы уверены, что хотите отменить создание урока? Все несохраненные данные будут потеряны.";
            string confirmButton = _isEditMode ? "Отменить редактирование" : "Отменить создание";

            var result = DialogService.ShowConfirmDialog(
                dialogTitle,
                dialogMessage,
                confirmButton,
                "Продолжить",
                Window.GetWindow(this)
            );

            if (result == true)
            {
                ReturnToLessonsPage();
            }
        }

        private void AddNewStep()
        {
            var step = new LessonStep
            {
                Title = $"Шаг {_steps.Count + 1}",
                Description = "",
                Hint = "",
                VisionTarget = "",
                VisionTargetFolder = "",
                RequiresVisionValidation = false,
                VisionConfidence = 0.85,
                RequiredMatches = 1
            };

            _steps.Add(step);
            UpdateStepsDisplay();
        }

        private void UpdateStepsDisplay()
        {
            StepsContainer.Children.Clear();

            for (int i = 0; i < _steps.Count; i++)
            {
                var stepCard = CreateStepCard(_steps[i], i + 1, i);
                StepsContainer.Children.Add(stepCard);
            }
        }

        private Border CreateStepCard(LessonStep step, int stepNumber, int stepIndex)
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

            var iconBorder = CreateStepIcon(stepNumber);
            Grid.SetColumn(iconBorder, 0);

            var titleStack = CreateStepTitleStack(step, stepNumber);
            Grid.SetColumn(titleStack, 1);

            var deleteButton = CreateDeleteButton(step, stepIndex);
            Grid.SetColumn(deleteButton, 2);

            titleGrid.Children.Add(iconBorder);
            titleGrid.Children.Add(titleStack);
            titleGrid.Children.Add(deleteButton);

            var contentStack = new StackPanel
            {
                UseLayoutRounding = true
            };

            var descriptionPanel = CreateDescriptionPanel(step, stepIndex);
            var hintPanel = CreateHintPanel(step, stepIndex);
            var imagePanel = CreateImagePanel(step, stepIndex);

            contentStack.Children.Add(descriptionPanel);
            contentStack.Children.Add(hintPanel);
            contentStack.Children.Add(imagePanel);

            mainStack.Children.Add(titleGrid);
            mainStack.Children.Add(contentStack);

            card.Child = mainStack;
            return card;
        }

        private Button CreateDeleteButton(LessonStep step, int stepIndex)
        {
            var deleteButton = new Button
            {
                Style = (Style)FindResource("NavigationButtonStyle"),
                Padding = new Thickness(16, 0, 0, 0),
                Margin = new Thickness(8, 0, 0, 0),
                UseLayoutRounding = true,
                Cursor = Cursors.Hand,
                ToolTip = "Удалить шаг"
            };

            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var deleteIcon = new Image
            {
                Tag = "Trash",
                Width = 18,
                Height = 18,
                Margin = new Thickness(0, 0, 8, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            ThemeManager.UpdateImageSource(deleteIcon, "Trash");

            var deleteText = new TextBlock
            {
                Text = "Удалить",
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 14,
                FontWeight = FontWeights.Medium,
                Foreground = (Brush)FindResource("TextPrimaryBrush")
            };

            stackPanel.Children.Add(deleteIcon);
            stackPanel.Children.Add(deleteText);

            deleteButton.Content = stackPanel;

            deleteButton.Click += (s, e) =>
            {
                var result = DialogService.ShowConfirmDialog(
                    "Удаление шага",
                    "Вы уверены, что хотите удалить этот шаг?",
                    "Удалить",
                    "Отмена",
                    Window.GetWindow(this)
                );

                if (result == true)
                {
                    int index = _steps.IndexOf(step);
                    if (index >= 0)
                    {
                        _steps.RemoveAt(index);

                        if (_originalStepImages.ContainsKey(stepIndex))
                        {
                            _originalStepImages.Remove(stepIndex);
                        }

                        UpdateStepsDisplay();
                    }
                }
            };

            return deleteButton;
        }

        private Border CreateStepIcon(int stepNumber)
        {
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
            return iconBorder;
        }

        private StackPanel CreateStepTitleStack(LessonStep step, int stepNumber)
        {
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
            return titleStack;
        }

        private StackPanel CreateDescriptionPanel(LessonStep step, int stepIndex)
        {
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
            return descriptionPanel;
        }

        private StackPanel CreateHintPanel(LessonStep step, int stepIndex)
        {
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
            return hintPanel;
        }

        private StackPanel CreateImagePanel(LessonStep step, int stepIndex)
        {
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
                Width = 18,
                Height = 18,
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

            var fileContainer = new Grid();
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

            string displayText;
            if (!string.IsNullOrEmpty(step.VisionTarget))
            {
                if (File.Exists(step.VisionTarget))
                {
                    displayText = Path.GetFileName(step.VisionTarget);
                }
                else if (_isEditMode && _originalStepImages.ContainsKey(stepIndex))
                {
                    displayText = _originalStepImages[stepIndex];
                }
                else
                {
                    displayText = step.VisionTarget;
                }
            }
            else
            {
                displayText = "Файл не выбран";
            }

            var fileInfoText = new TextBlock
            {
                Text = displayText,
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

            var clearImageButton = CreateClearImageButton(step, stepIndex, fileInfoText);
            var selectImageButton = CreateSelectImageButton(step, stepIndex, fileInfoText, clearImageButton);

            buttonPanel.Children.Add(clearImageButton);
            buttonPanel.Children.Add(selectImageButton);

            fileContainer.Children.Add(fileInfoBorder);
            fileContainer.Children.Add(buttonPanel);

            imagePanel.Children.Add(imageHeader);
            imagePanel.Children.Add(fileContainer);
            return imagePanel;
        }

        private Button CreateClearImageButton(LessonStep step, int stepIndex, TextBlock fileInfoText)
        {
            var clearImageButton = new Button
            {
                Style = (Style)FindResource("NavigationButtonStyle"),
                Margin = new Thickness(0, 0, 8, 0),
                Visibility = string.IsNullOrEmpty(step.VisionTarget) &&
                            !(_isEditMode && _originalStepImages.ContainsKey(stepIndex))
                    ? Visibility.Collapsed
                    : Visibility.Visible
            };

            var clearIcon = new Image
            {
                Tag = "Trash",
                Width = 18,
                Height = 18,
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

                if (_isEditMode && _originalStepImages.ContainsKey(stepIndex))
                {
                    _originalStepImages.Remove(stepIndex);
                }

                fileInfoText.Text = "Файл не выбран";
                clearImageButton.Visibility = Visibility.Collapsed;
            };

            return clearImageButton;
        }

        private Button CreateSelectImageButton(LessonStep step, int stepIndex, TextBlock fileInfoText, Button clearImageButton)
        {
            var selectImageButton = new Button
            {
                Style = (Style)FindResource("NavigationButtonStyle")
            };

            var selectIcon = new Image
            {
                Tag = "Folder",
                Width = 18,
                Height = 18,
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

                    if (_isEditMode)
                    {
                        _originalStepImages[stepIndex] = Path.GetFileName(step.VisionTarget);
                    }
                }
            };

            return selectImageButton;
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

        private void SaveLessonToFile()
        {
            try
            {
                string lessonTitle = TitleTextBox.Text.Trim();

                string lessonId = GenerateNumericLessonId(lessonTitle);

                var lesson = new LessonData
                {
                    Id = lessonId,
                    Title = lessonTitle,
                    CourseId = "Custom",
                    Steps = new List<LessonStep>(),
                    CompletionMessage = CompletionMessageTextBox.Text.Trim()
                };

                string rootTemplatesPath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "..", "..",
                    "ComputerVision", "Templates");

                Directory.CreateDirectory(rootTemplatesPath);

                string oldTemplatesPath = Path.Combine(_customLessonsPath, "Templates");
                if (Directory.Exists(oldTemplatesPath))
                {
                    try
                    {
                        Directory.Delete(oldTemplatesPath, true);
                        Console.WriteLine($"Удалена старая папка шаблонов: {oldTemplatesPath}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Не удалось удалить старую папку шаблонов: {ex.Message}");
                    }
                }

                var stepsForJson = new List<object>();
                int screenshotCounter = 1;

                foreach (var step in _steps)
                {
                    var stepDict = new Dictionary<string, object>
                    {
                        ["title"] = step.Title,
                        ["description"] = step.Description
                    };

                    if (!string.IsNullOrWhiteSpace(step.Hint))
                        stepDict["hint"] = step.Hint;

                    if (!string.IsNullOrEmpty(step.VisionTarget) && File.Exists(step.VisionTarget))
                    {
                        string extension = Path.GetExtension(step.VisionTarget).ToLower();

                        string numericFileName = $"{lessonId}_{screenshotCounter:000}{extension}";
                        string destPath = Path.Combine(rootTemplatesPath, numericFileName);

                        File.Copy(step.VisionTarget, destPath, true);
                        Console.WriteLine($"Скопирован файл в Templates: {Path.GetFileName(step.VisionTarget)} -> {numericFileName}");

                        stepDict["visionTarget"] = numericFileName;
                        stepDict["visionConfidence"] = 0.85;
                        stepDict["requiresVisionValidation"] = true;

                        screenshotCounter++;
                    }

                    else if (!string.IsNullOrEmpty(step.VisionTargetFolder))
                    {
                        stepDict["visionTargetFolder"] = step.VisionTargetFolder;
                        stepDict["requiredMatches"] = 1;
                        stepDict["visionConfidence"] = 0.8;
                        stepDict["requiresVisionValidation"] = true;
                    }

                    stepsForJson.Add(stepDict);
                }

                var jsonObject = new
                {
                    id = lesson.Id,
                    title = lesson.Title,
                    courseId = lesson.CourseId,
                    completionMessage = lesson.CompletionMessage,
                    steps = stepsForJson
                };

                string filePath = Path.Combine(_customLessonsPath, $"{lesson.Id}.json");

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                string json = JsonSerializer.Serialize(jsonObject, options);
                File.WriteAllText(filePath, json, Encoding.UTF8);

                Console.WriteLine($"Урок сохранен: {filePath}");

                DialogService.ShowSuccessDialog(
                    "Урок успешно сохранен!",
                    Window.GetWindow(this)
                );

                ReturnToLessonsPage();
            }
            catch (Exception ex)
            {
                DialogService.ShowErrorDialog(
                    $"Ошибка при сохранении урока: {ex.Message}",
                    Window.GetWindow(this)
                );
            }
        }

        private void UpdateLessonFile()
        {
            try
            {
                string lessonTitle = TitleTextBox.Text.Trim();

                string lessonId = _editingLessonId;

                var lesson = new LessonData
                {
                    Id = lessonId,
                    Title = lessonTitle,
                    CourseId = "Custom",
                    Steps = new List<LessonStep>(),
                    CompletionMessage = CompletionMessageTextBox.Text.Trim()
                };

                string rootTemplatesPath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "..", "..",
                    "ComputerVision", "Templates");

                Directory.CreateDirectory(rootTemplatesPath);

                var stepsForJson = new List<object>();
                int screenshotCounter = 1;

                for (int i = 0; i < _steps.Count; i++)
                {
                    var step = _steps[i];
                    var stepDict = new Dictionary<string, object>
                    {
                        ["title"] = step.Title,
                        ["description"] = step.Description
                    };

                    if (!string.IsNullOrWhiteSpace(step.Hint))
                        stepDict["hint"] = step.Hint;

                    if (!string.IsNullOrEmpty(step.VisionTarget) && File.Exists(step.VisionTarget))
                    {
                        string extension = Path.GetExtension(step.VisionTarget).ToLower();

                        string numericFileName;
                        if (_originalStepImages.ContainsKey(i) &&
                            !string.IsNullOrEmpty(_originalStepImages[i]) &&
                            step.VisionTarget.Contains(_originalStepImages[i]))
                        {
                            numericFileName = _originalStepImages[i];
                        }
                        else
                        {
                            numericFileName = $"{lessonId}_{screenshotCounter:000}{extension}";
                            string destPath = Path.Combine(rootTemplatesPath, numericFileName);

                            File.Copy(step.VisionTarget, destPath, true);
                            Console.WriteLine($"Скопирован файл в Templates: {Path.GetFileName(step.VisionTarget)} -> {numericFileName}");

                            screenshotCounter++;
                        }

                        stepDict["visionTarget"] = numericFileName;
                        stepDict["visionConfidence"] = 0.85;
                        stepDict["requiresVisionValidation"] = true;
                    }
                    else if (_originalStepImages.ContainsKey(i) &&
                            !string.IsNullOrEmpty(_originalStepImages[i]))
                    {
                        stepDict["visionTarget"] = _originalStepImages[i];
                        stepDict["visionConfidence"] = 0.85;
                        stepDict["requiresVisionValidation"] = true;
                    }

                    else if (!string.IsNullOrEmpty(step.VisionTargetFolder))
                    {
                        stepDict["visionTargetFolder"] = step.VisionTargetFolder;
                        stepDict["requiredMatches"] = 1;
                        stepDict["visionConfidence"] = 0.8;
                        stepDict["requiresVisionValidation"] = true;
                    }

                    stepsForJson.Add(stepDict);
                }

                var jsonObject = new
                {
                    id = lesson.Id,
                    title = lesson.Title,
                    courseId = lesson.CourseId,
                    completionMessage = lesson.CompletionMessage,
                    steps = stepsForJson
                };

                string filePath = Path.Combine(_customLessonsPath, $"{lesson.Id}.json");

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                string json = JsonSerializer.Serialize(jsonObject, options);
                File.WriteAllText(filePath, json, Encoding.UTF8);

                Console.WriteLine($"Урок обновлен: {filePath}");

                DialogService.ShowSuccessDialog(
                    "Урок успешно обновлен!",
                    Window.GetWindow(this)
                );

                ReturnToLessonsPage();
            }
            catch (Exception ex)
            {
                DialogService.ShowErrorDialog(
                    $"Ошибка при обновлении урока: {ex.Message}",
                    Window.GetWindow(this)
                );
            }
        }

        private string GenerateNumericLessonId(string lessonTitle)
        {
            int titleHash = Math.Abs(lessonTitle.GetHashCode());

            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");

            string combined = $"{titleHash}{timestamp}";
            string numericOnly = new string(combined.Where(char.IsDigit).ToArray());

            return numericOnly.Length > 15 ? numericOnly.Substring(0, 15) : numericOnly;
        }
    }
}