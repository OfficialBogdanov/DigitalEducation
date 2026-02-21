using DigitalEducation.Pages.CreateCustomLesson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DigitalEducation.Pages
{
    public partial class CreateLessonPage : UserControl
    {
        private readonly StepCollection _steps;
        private readonly StepCardFactory _stepCardFactory;
        private readonly ILessonStorage _storage;
        private Border _addStepBlock;
        private bool _isMousePressed = false;

        private string _editingLessonId;
        private bool _isEditMode = false;
        private LessonData _originalLesson;
        private readonly Dictionary<int, string> _originalStepImages = new Dictionary<int, string>();

        public CreateLessonPage(string lessonId = null)
        {
            InitializeComponent();
            _storage = new LessonFileStorage();
            _stepCardFactory = new StepCardFactory(this);
            _steps = new StepCollection();
            _steps.Changed += (s, e) => UpdateStepsDisplay();

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
                if (saveButtonText?.Children.Count > 1 && saveButtonText.Children[1] is TextBlock textBlock)
                {
                    textBlock.Text = "Обновить урок";
                }

                var scrollViewer = MainGrid.Children[0] as ScrollViewer;
                var stackPanel = scrollViewer?.Content as StackPanel;
                var grid = stackPanel?.Children[0] as Grid;
                var titleStack = grid?.Children[1] as StackPanel;
                var titleText = titleStack?.Children[0] as TextBlock;
                if (titleText != null)
                    titleText.Text = "Редактирование урока";
            }
        }

        private void LoadLessonForEditing()
        {
            try
            {
                _originalLesson = _storage.LoadLesson(_editingLessonId);
                if (_originalLesson == null)
                {
                    DialogService.ShowErrorDialog("Файл урока не найден", Window.GetWindow(this));
                    ReturnToLessonsPage();
                    return;
                }

                TitleTextBox.Text = _originalLesson.Title;
                CompletionMessageTextBox.Text = _originalLesson.CompletionMessage ?? "";

                _steps.Clear();
                _originalStepImages.Clear();

                if (_originalLesson.Steps != null)
                {
                    for (int i = 0; i < _originalLesson.Steps.Count; i++)
                    {
                        var step = _originalLesson.Steps[i];
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
                            _originalStepImages[i] = step.VisionTarget;
                        }
                    }
                }

                UpdateStepsDisplay();
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
                _addStepBlock.Background = (Brush)FindResource("PressedBrush");
        }

        private void AddStepBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isMousePressed)
            {
                _isMousePressed = false;
                AddNewStep();
                if (_addStepBlock != null)
                    _addStepBlock.Background = (Brush)FindResource("BackgroundBrush");
            }
        }

        private void AddStepBlock_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!_isMousePressed && _addStepBlock != null)
                _addStepBlock.Background = (Brush)FindResource("HoverBrush");
        }

        private void AddStepBlock_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!_isMousePressed && _addStepBlock != null)
                _addStepBlock.Background = (Brush)FindResource("BackgroundBrush");
            _isMousePressed = false;
        }

        private void AddNewStep()
        {
            _steps.Add(new LessonStep
            {
                Title = $"Шаг {_steps.Count + 1}",
                Description = "",
                Hint = "",
                VisionTarget = "",
                VisionTargetFolder = "",
                RequiresVisionValidation = false,
                VisionConfidence = 0.85,
                RequiredMatches = 1
            });
        }

        private void UpdateStepsDisplay()
        {
            StepsContainer.Children.Clear();

            for (int i = 0; i < _steps.Count; i++)
            {
                int index = i;
                var step = _steps[index];

                var card = _stepCardFactory.CreateStepCard(
                    step,
                    index + 1,
                    index,
                    onDelete: (idx) =>
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
                            _steps.RemoveAt(idx);
                            if (_originalStepImages.ContainsKey(idx))
                                _originalStepImages.Remove(idx);
                        }
                    },
                    onImageSelected: (idx, fileName) =>
                    {
                        if (_isEditMode)
                            _originalStepImages[idx] = Path.GetFileName(fileName);
                    },
                    onImageCleared: (idx) =>
                    {
                        if (_isEditMode && _originalStepImages.ContainsKey(idx))
                            _originalStepImages.Remove(idx);
                    }
                );

                StepsContainer.Children.Add(card);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var stepsList = new List<LessonStep>(_steps);
            var validation = LessonValidator.Validate(
                TitleTextBox.Text,
                CompletionMessageTextBox.Text,
                stepsList
            );

            TitleErrorText.Visibility = validation.TitleError != null ? Visibility.Visible : Visibility.Collapsed;
            CompletionMessageErrorText.Visibility = validation.CompletionError != null ? Visibility.Visible : Visibility.Collapsed;
            GeneralErrorText.Text = validation.GeneralError ?? "";
            GeneralErrorText.Visibility = validation.GeneralError != null ? Visibility.Visible : Visibility.Collapsed;

            if (!validation.IsValid)
                return;

            string dialogTitle = _isEditMode ? "Обновление урока" : "Сохранение урока";
            string dialogMessage = _isEditMode ? "Вы уверены, что хотите обновить урок?" : "Вы уверены, что хотите сохранить урок?";
            string confirmButton = _isEditMode ? "Обновить" : "Сохранить";

            var result = DialogService.ShowConfirmDialog(dialogTitle, dialogMessage, confirmButton, "Отмена", Window.GetWindow(this));
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

            var result = DialogService.ShowConfirmDialog(dialogTitle, dialogMessage, confirmButton, "Продолжить", Window.GetWindow(this));
            if (result == true)
                ReturnToLessonsPage();
        }

        private void SaveLessonToFile()
        {
            try
            {
                var lesson = new LessonData
                {
                    Title = TitleTextBox.Text.Trim(),
                    CourseId = "Custom",
                    CompletionMessage = CompletionMessageTextBox.Text.Trim(),
                    Steps = new List<LessonStep>()
                };

                _storage.SaveNewLesson(lesson, new List<LessonStep>(_steps));

                LessonManager.ReloadAllLessons();

                DialogService.ShowSuccessDialog("Урок успешно сохранен!", Window.GetWindow(this));
                ReturnToLessonsPage();
            }
            catch (Exception ex)
            {
                DialogService.ShowErrorDialog($"Ошибка при сохранении урока: {ex.Message}", Window.GetWindow(this));
            }
        }

        private void UpdateLessonFile()
        {
            try
            {
                var lesson = new LessonData
                {
                    Id = _editingLessonId,
                    Title = TitleTextBox.Text.Trim(),
                    CourseId = "Custom",
                    CompletionMessage = CompletionMessageTextBox.Text.Trim(),
                    Steps = new List<LessonStep>()
                };

                _storage.UpdateLesson(_editingLessonId, lesson, new List<LessonStep>(_steps));

                LessonManager.ReloadAllLessons();

                DialogService.ShowSuccessDialog("Урок успешно обновлен!", Window.GetWindow(this));
                ReturnToLessonsPage();
            }
            catch (Exception ex)
            {
                DialogService.ShowErrorDialog($"Ошибка при обновлении урока: {ex.Message}", Window.GetWindow(this));
            }
        }
    }
}