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
        private readonly LessonStepCollection _steps;
        private readonly LessonStepCardFactory _stepCardFactory;
        private readonly ILessonStorage _storage;
        private Border _addStepBlock;
        private bool _isMousePressed = false;

        private string _editingLessonId;
        private bool _isEditMode = false;
        private LessonDataModel _originalLesson;
        private readonly Dictionary<int, string> _originalStepImages = new Dictionary<int, string>();
        private readonly Dictionary<int, string> _originalStepHintImages = new Dictionary<int, string>();
        private readonly Dictionary<int, string> _originalStepFolders = new Dictionary<int, string>();
        private readonly Dictionary<int, string> _originalStepHintFolders = new Dictionary<int, string>();

        public CreateLessonPage(string lessonId = null)
        {
            InitializeComponent();
            _storage = new CustomLessonFileStorage();
            _stepCardFactory = new LessonStepCardFactory(this);
            _steps = new LessonStepCollection();
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
            AppThemeManager.ThemeChanged += OnThemeChanged;
            AppThemeManager.UpdateAllIconsInContainer(this);

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
            AppThemeManager.ThemeChanged -= OnThemeChanged;

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

                if (this.PageTitleText != null)
                    this.PageTitleText.Text = "Редактирование урока";
            }
        }

        private void LoadLessonForEditing()
        {
            try
            {
                _originalLesson = _storage.LoadLesson(_editingLessonId);
                if (_originalLesson == null)
                {
                    AppDialogService.ShowErrorDialog("Файл урока не найден", Window.GetWindow(this));
                    ReturnToLessonsPage();
                    return;
                }

                TitleTextBox.Text = _originalLesson.Title;
                CompletionMessageTextBox.Text = _originalLesson.CompletionMessage ?? "";

                _steps.Clear();
                _originalStepImages.Clear();
                _originalStepHintImages.Clear();
                _originalStepFolders.Clear();
                _originalStepHintFolders.Clear();

                string projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", ".."));
                string templatesPath = Path.Combine(projectRoot, "Learning", "Engine", "Templates");

                if (_originalLesson.Steps != null)
                {
                    for (int i = 0; i < _originalLesson.Steps.Count; i++)
                    {
                        var step = _originalLesson.Steps[i];
                        var newStep = new LessonStep
                        {
                            Title = step.Title,
                            Description = step.Description,
                            Hint = step.Hint,
                            VisionTarget = step.VisionTarget,
                            VisionTargetFolder = step.VisionTargetFolder,
                            RequiresVisionValidation = step.RequiresVisionValidation,
                            VisionConfidence = step.VisionConfidence,
                            HintConfidence = step.HintConfidence,
                            RequiredMatches = step.RequiredMatches,
                            RequiredHintMatches = step.RequiredHintMatches,
                            HintType = step.HintType ?? "rectangle",
                            ShowHint = step.ShowHint
                        };

                        // Загрузка подсказки (файл)
                        if (!string.IsNullOrEmpty(step.VisionHint))
                        {
                            string[] possibleHintFiles = Directory.GetFiles(templatesPath, step.VisionHint + ".*");
                            if (possibleHintFiles.Length > 0)
                            {
                                newStep.SelectedHintFilePath = possibleHintFiles[0];
                                newStep.VisionHint = step.VisionHint;
                                _originalStepHintImages[i] = Path.GetFileName(possibleHintFiles[0]);
                            }
                        }

                        // Загрузка подсказки (папка)
                        if (!string.IsNullOrEmpty(step.VisionHintFolder))
                        {
                            string folderPath = Path.Combine(templatesPath, step.VisionHintFolder);
                            if (Directory.Exists(folderPath))
                            {
                                newStep.SelectedHintFolderPath = folderPath;
                                newStep.VisionHintFolder = step.VisionHintFolder;
                                _originalStepHintFolders[i] = step.VisionHintFolder;
                            }
                        }

                        // Устанавливаем ShowHint в true, если есть хотя бы один источник подсказки
                        if (!string.IsNullOrEmpty(newStep.SelectedHintFilePath) || !string.IsNullOrEmpty(newStep.SelectedHintFolderPath))
                        {
                            newStep.ShowHint = true;
                        }

                        // Загрузка проверки (файл)
                        if (!string.IsNullOrEmpty(step.VisionTarget))
                        {
                            string[] possibleTargetFiles = Directory.GetFiles(templatesPath, step.VisionTarget + ".*");
                            if (possibleTargetFiles.Length > 0)
                            {
                                newStep.SelectedFilePath = possibleTargetFiles[0];
                                newStep.VisionTarget = possibleTargetFiles[0];
                                _originalStepImages[i] = Path.GetFileName(possibleTargetFiles[0]);
                            }
                        }

                        // Загрузка проверки (папка)
                        if (!string.IsNullOrEmpty(step.VisionTargetFolder))
                        {
                            string folderPath = Path.Combine(templatesPath, step.VisionTargetFolder);
                            if (Directory.Exists(folderPath))
                            {
                                newStep.SelectedFolderPath = folderPath;
                                newStep.VisionTargetFolder = step.VisionTargetFolder;
                                newStep.RequiredMatches = step.RequiredMatches;
                                _originalStepFolders[i] = step.VisionTargetFolder;
                            }
                        }

                        _steps.Add(newStep);
                    }
                }

                UpdateStepsDisplay();
            }
            catch (Exception ex)
            {
                AppDialogService.ShowErrorDialog($"Ошибка загрузки урока: {ex.Message}", Window.GetWindow(this));
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
            AppThemeManager.UpdateAllIconsInContainer(this);
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
                RequiredMatches = 1,
                RequiredHintMatches = 1,
                HintType = "rectangle",
                HintImagePath = "",
                ShowHint = false
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
                        var result = AppDialogService.ShowConfirmDialog(
                            "Удаление шага",
                            "Вы уверены, что хотите удалить этот шаг?",
                            "Удалить",
                            "Отмена",
                            Window.GetWindow(this)
                        );

                        if (result == true)
                        {
                            _steps.RemoveAt(idx);
                            _originalStepImages.Remove(idx);
                            _originalStepHintImages.Remove(idx);
                            _originalStepFolders.Remove(idx);
                            _originalStepHintFolders.Remove(idx);
                        }
                    },
                    onValidationFileSelected: (idx, fileName) =>
                    {
                        var currentStep = _steps[idx];
                        currentStep.SelectedFilePath = fileName;
                        currentStep.VisionTarget = fileName;
                        currentStep.RequiresVisionValidation = true;
                        currentStep.SelectedFolderPath = null;
                        currentStep.VisionTargetFolder = null;
                        if (_isEditMode)
                            _originalStepImages[idx] = Path.GetFileName(fileName);
                    },
                    onValidationFileCleared: (idx) =>
                    {
                        var currentStep = _steps[idx];
                        currentStep.SelectedFilePath = null;
                        currentStep.VisionTarget = "";
                        currentStep.RequiresVisionValidation = false;
                        if (_isEditMode && _originalStepImages.ContainsKey(idx))
                            _originalStepImages.Remove(idx);
                    },
                    onValidationFolderSelected: (idx, folderPath) =>
                    {
                        var currentStep = _steps[idx];
                        currentStep.SelectedFolderPath = folderPath;
                        currentStep.VisionTargetFolder = Path.GetFileName(folderPath);
                        currentStep.RequiresVisionValidation = true;
                        currentStep.SelectedFilePath = null;
                        currentStep.VisionTarget = null;
                        if (_isEditMode)
                            _originalStepFolders[idx] = Path.GetFileName(folderPath);
                    },
                    onValidationFolderCleared: (idx) =>
                    {
                        var currentStep = _steps[idx];
                        currentStep.SelectedFolderPath = null;
                        currentStep.VisionTargetFolder = null;
                        currentStep.RequiresVisionValidation = false;
                        if (_isEditMode && _originalStepFolders.ContainsKey(idx))
                            _originalStepFolders.Remove(idx);
                    },
                    onHintFileSelected: (idx, fileName) =>
                    {
                        var currentStep = _steps[idx];
                        currentStep.SelectedHintFilePath = fileName;
                        currentStep.VisionHint = Path.GetFileNameWithoutExtension(fileName);
                        currentStep.ShowHint = true;
                        currentStep.SelectedHintFolderPath = null;
                        currentStep.VisionHintFolder = null;
                        if (_isEditMode)
                        {
                            string fileNameOnly = Path.GetFileName(fileName);
                            _originalStepHintImages[idx] = fileNameOnly;
                        }
                    },
                    onHintFileCleared: (idx) =>
                    {
                        var currentStep = _steps[idx];
                        currentStep.SelectedHintFilePath = null;
                        currentStep.VisionHint = null;
                        currentStep.ShowHint = false;
                        if (_isEditMode && _originalStepHintImages.ContainsKey(idx))
                            _originalStepHintImages.Remove(idx);
                    },
                    onHintFolderSelected: (idx, folderPath) =>
                    {
                        var currentStep = _steps[idx];
                        currentStep.SelectedHintFolderPath = folderPath;
                        currentStep.VisionHintFolder = Path.GetFileName(folderPath);
                        currentStep.ShowHint = true;
                        currentStep.SelectedHintFilePath = null;
                        currentStep.VisionHint = null;
                        if (_isEditMode)
                            _originalStepHintFolders[idx] = Path.GetFileName(folderPath);
                    },
                    onHintFolderCleared: (idx) =>
                    {
                        var currentStep = _steps[idx];
                        currentStep.SelectedHintFolderPath = null;
                        currentStep.VisionHintFolder = null;
                        currentStep.ShowHint = false;
                        if (_isEditMode && _originalStepHintFolders.ContainsKey(idx))
                            _originalStepHintFolders.Remove(idx);
                    }
                );

                StepsContainer.Children.Add(card);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var stepsList = new List<LessonStep>(_steps);
            var validation = CustomLessonValidator.Validate(
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

            var result = AppDialogService.ShowConfirmDialog(dialogTitle, dialogMessage, confirmButton, "Отмена", Window.GetWindow(this));
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

            var result = AppDialogService.ShowConfirmDialog(dialogTitle, dialogMessage, confirmButton, "Продолжить", Window.GetWindow(this));
            if (result == true)
                ReturnToLessonsPage();
        }

        private void SaveLessonToFile()
        {
            try
            {
                var lesson = new LessonDataModel
                {
                    Title = TitleTextBox.Text.Trim(),
                    CourseId = "Custom",
                    CompletionMessage = CompletionMessageTextBox.Text.Trim()
                };

                string lessonId = _storage.GenerateNewLessonId();
                var stepsList = new List<LessonStep>(_steps);

                lesson.Steps = stepsList;

                _storage.SaveNewLesson(lesson, stepsList);
                LessonRegistry.ReloadAllLessons();

                AppDialogService.ShowSuccessDialog("Урок успешно сохранен!", Window.GetWindow(this));
                ReturnToLessonsPage();
            }
            catch (Exception ex)
            {
                AppDialogService.ShowErrorDialog($"Ошибка при сохранении урока: {ex.Message}", Window.GetWindow(this));
            }
        }

        private void UpdateLessonFile()
        {
            try
            {
                var lesson = new LessonDataModel
                {
                    Id = _editingLessonId,
                    Title = TitleTextBox.Text.Trim(),
                    CourseId = "Custom",
                    CompletionMessage = CompletionMessageTextBox.Text.Trim()
                };

                var stepsList = new List<LessonStep>(_steps);

                lesson.Steps = stepsList;

                _storage.UpdateLesson(_editingLessonId, lesson, stepsList);
                LessonRegistry.ReloadAllLessons();

                AppDialogService.ShowSuccessDialog("Урок успешно обновлен!", Window.GetWindow(this));
                ReturnToLessonsPage();
            }
            catch (Exception ex)
            {
                AppDialogService.ShowErrorDialog($"Ошибка при обновлении урока: {ex.Message}", Window.GetWindow(this));
            }
        }
    }
}