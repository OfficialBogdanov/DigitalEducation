using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DigitalEducation.Pages.CreateCustomLesson
{
    public class LessonStepCardFactory
    {
        private readonly FrameworkElement _resourceParent;
        private static readonly Dictionary<string, string> HintTypeIconNames = new Dictionary<string, string>
        {
            { "rectangle", "Maximize" },
            { "arrow", "Right" },
            { "highlight", "Highlight" },
            { "corner", "Layout" },
            { "glow", "Glow" },
            { "dim", "Layers" }
        };

        public LessonStepCardFactory(FrameworkElement resourceParent)
        {
            _resourceParent = resourceParent ?? throw new ArgumentNullException(nameof(resourceParent));
        }

        public Border CreateStepCard(
            LessonStep step,
            int stepNumber,
            int stepIndex,
            Action<int> onDelete,
            Action<int, string> onValidationFileSelected,
            Action<int> onValidationFileCleared,
            Action<int, string> onValidationFolderSelected,
            Action<int> onValidationFolderCleared,
            Action<int, string> onHintFileSelected,
            Action<int> onHintFileCleared,
            Action<int, string> onHintFolderSelected,
            Action<int> onHintFolderCleared)
        {
            var card = new Border
            {
                Style = (Style)_resourceParent.FindResource("CardStyle"),
                Margin = new Thickness(0, 0, 0, 32),
                UseLayoutRounding = true,
                SnapsToDevicePixels = true
            };

            var mainStack = new StackPanel
            {
                UseLayoutRounding = true,
                Margin = new Thickness(24)
            };

            var titleGrid = CreateTitleGrid(step, stepNumber, stepIndex, onDelete);
            var contentStack = new StackPanel { UseLayoutRounding = true };

            contentStack.Children.Add(CreateDescriptionPanel(step, stepIndex));
            contentStack.Children.Add(CreateHintPanel(step, stepIndex));
            contentStack.Children.Add(CreateValidationSourcePanel(step, stepIndex, onValidationFileSelected, onValidationFileCleared, onValidationFolderSelected, onValidationFolderCleared));
            contentStack.Children.Add(CreateHintSourcePanel(step, stepIndex, onHintFileSelected, onHintFileCleared, onHintFolderSelected, onHintFolderCleared));

            mainStack.Children.Add(titleGrid);
            mainStack.Children.Add(contentStack);

            card.Child = mainStack;
            return card;
        }

        private Grid CreateTitleGrid(LessonStep step, int stepNumber, int stepIndex, Action<int> onDelete)
        {
            var grid = new Grid { UseLayoutRounding = true, Margin = new Thickness(0, 0, 0, 24) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var iconBorder = CreateStepIcon(stepNumber);
            Grid.SetColumn(iconBorder, 0);

            var titleStack = CreateStepTitleStack(step);
            Grid.SetColumn(titleStack, 1);

            var deleteButton = CreateDeleteButton(stepIndex, onDelete);
            Grid.SetColumn(deleteButton, 2);

            grid.Children.Add(iconBorder);
            grid.Children.Add(titleStack);
            grid.Children.Add(deleteButton);
            return grid;
        }

        private Border CreateStepIcon(int stepNumber)
        {
            var iconBorder = new Border
            {
                Width = 48,
                Height = 48,
                Background = (Brush)_resourceParent.FindResource("CustomBrush"),
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

        private StackPanel CreateStepTitleStack(LessonStep step)
        {
            var titleStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            var stepTitle = new TextBlock
            {
                Text = step.Title,
                Style = (Style)_resourceParent.FindResource("SubtitleTextStyle"),
                Foreground = (Brush)_resourceParent.FindResource("CustomBrush"),
                Margin = new Thickness(0, 0, 0, 4)
            };
            var stepSubtitle = new TextBlock
            {
                Text = "Заполните информацию о практическом шаге",
                Style = (Style)_resourceParent.FindResource("BodyTextStyle"),
                Foreground = (Brush)_resourceParent.FindResource("TextSecondaryBrush")
            };
            titleStack.Children.Add(stepTitle);
            titleStack.Children.Add(stepSubtitle);
            return titleStack;
        }

        private Button CreateDeleteButton(int stepIndex, Action<int> onDelete)
        {
            var button = new Button
            {
                Style = (Style)_resourceParent.FindResource("NavigationButtonStyle"),
                Padding = new Thickness(16, 0, 0, 0),
                Margin = new Thickness(8, 0, 0, 0),
                UseLayoutRounding = true,
                Cursor = Cursors.Hand,
                ToolTip = "Удалить шаг"
            };

            var icon = new Image
            {
                Tag = "Trash",
                Width = 18,
                Height = 18,
                Margin = new Thickness(0, 0, 8, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            AppThemeManager.UpdateImageSource(icon, "Trash");

            var text = new TextBlock
            {
                Text = "Удалить",
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 14,
                FontWeight = FontWeights.Medium,
                Foreground = (Brush)_resourceParent.FindResource("TextPrimaryBrush")
            };

            var stack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            stack.Children.Add(icon);
            stack.Children.Add(text);
            button.Content = stack;

            button.Click += (s, e) => onDelete?.Invoke(stepIndex);
            return button;
        }

        private StackPanel CreateDescriptionPanel(LessonStep step, int stepIndex)
        {
            var panel = new StackPanel { Margin = new Thickness(0, 0, 0, 20) };
            var header = CreateHeader("Info", "Описание шага *");
            var textBox = new TextBox
            {
                Text = step.Description,
                Style = (Style)_resourceParent.FindResource("RoundedMultiLineTextBox"),
                Height = 120,
                MaxLength = 1000
            };
            textBox.TextChanged += (s, e) => step.Description = textBox.Text;
            panel.Children.Add(header);
            panel.Children.Add(textBox);
            return panel;
        }

        private StackPanel CreateHintPanel(LessonStep step, int stepIndex)
        {
            var panel = new StackPanel { Margin = new Thickness(0, 0, 0, 20) };
            var header = CreateHeader("Info", "Подсказка (необязательно)");
            var textBox = new TextBox
            {
                Text = step.Hint,
                Style = (Style)_resourceParent.FindResource("RoundedMultiLineTextBox"),
                Height = 100,
                MaxLength = 500
            };
            textBox.TextChanged += (s, e) => step.Hint = textBox.Text;
            panel.Children.Add(header);
            panel.Children.Add(textBox);
            return panel;
        }

        private StackPanel CreateValidationSourcePanel(LessonStep step, int stepIndex,
            Action<int, string> onFileSelected, Action<int> onFileCleared,
            Action<int, string> onFolderSelected, Action<int> onFolderCleared)
        {
            var panel = new StackPanel { Margin = new Thickness(0, 0, 0, 20) };
            var header = CreateHeader("Image", "Изображение для проверки (выберите файл или папку)");

            var sourceGrid = new Grid();
            sourceGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            sourceGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var pathText = new TextBlock
            {
                Text = GetValidationDisplayPath(step),
                Style = (Style)_resourceParent.FindResource("BodyTextStyle"),
                Foreground = (Brush)_resourceParent.FindResource("TextSecondaryBrush"),
                VerticalAlignment = VerticalAlignment.Center
            };
            var pathBorder = new Border
            {
                Background = (Brush)_resourceParent.FindResource("BackgroundLightBrush"),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(16),
                BorderBrush = (Brush)_resourceParent.FindResource("SurfaceBorderBrush"),
                BorderThickness = new Thickness(1),
                Child = pathText
            };
            Grid.SetColumn(pathBorder, 0);

            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(12, 0, 0, 0) };
            Grid.SetColumn(buttonPanel, 1);

            var clearButton = CreateIconButton("Trash", "Очистить", null);
            clearButton.Visibility = (step.SelectedFilePath != null || step.SelectedFolderPath != null) ? Visibility.Visible : Visibility.Collapsed;
            clearButton.Click += (s, e) =>
            {
                step.SelectedFilePath = null;
                step.SelectedFolderPath = null;
                step.VisionTarget = null;
                step.VisionTargetFolder = null;
                step.RequiresVisionValidation = false;
                pathText.Text = "Ничего не выбрано";
                clearButton.Visibility = Visibility.Collapsed;
                HideValidationOptions(panel);
                onFileCleared?.Invoke(stepIndex);
                onFolderCleared?.Invoke(stepIndex);
            };

            var fileButton = CreateIconButton("Image", "Выбрать файл", () =>
            {
                var dialog = new OpenFileDialog
                {
                    Filter = "Изображения (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp|Все файлы (*.*)|*.*",
                    Title = "Выберите изображение"
                };
                if (dialog.ShowDialog() == true)
                {
                    step.SelectedFilePath = dialog.FileName;
                    step.VisionTarget = dialog.FileName;
                    step.RequiresVisionValidation = true;
                    step.SelectedFolderPath = null;
                    step.VisionTargetFolder = null;
                    pathText.Text = Path.GetFileName(dialog.FileName);
                    clearButton.Visibility = Visibility.Visible;
                    ShowValidationFileOptions(panel, step);
                    onFileSelected?.Invoke(stepIndex, dialog.FileName);
                    onFolderCleared?.Invoke(stepIndex);
                }
            });

            var folderButton = CreateIconButton("Folder", "Выбрать папку", () =>
            {
                var dialog = new System.Windows.Forms.FolderBrowserDialog();
                dialog.Description = "Выберите папку с PNG-шаблонами";
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    step.SelectedFolderPath = dialog.SelectedPath;
                    step.VisionTargetFolder = Path.GetFileName(dialog.SelectedPath);
                    step.RequiresVisionValidation = true;
                    step.SelectedFilePath = null;
                    step.VisionTarget = null;
                    pathText.Text = Path.GetFileName(dialog.SelectedPath);
                    clearButton.Visibility = Visibility.Visible;
                    ShowValidationFolderOptions(panel, step);
                    onFolderSelected?.Invoke(stepIndex, dialog.SelectedPath);
                    onFileCleared?.Invoke(stepIndex);
                }
            });

            buttonPanel.Children.Add(clearButton);
            buttonPanel.Children.Add(fileButton);
            buttonPanel.Children.Add(folderButton);

            sourceGrid.Children.Add(pathBorder);
            sourceGrid.Children.Add(buttonPanel);

            panel.Children.Add(header);
            panel.Children.Add(sourceGrid);

            var optionsContainer = new StackPanel { Name = "ValidationOptionsContainer", Margin = new Thickness(0, 12, 0, 0) };
            panel.Children.Add(optionsContainer);

            if (step.SelectedFilePath != null)
                ShowValidationFileOptions(panel, step);
            else if (step.SelectedFolderPath != null)
                ShowValidationFolderOptions(panel, step);

            return panel;
        }

        private void ShowValidationFileOptions(StackPanel parentPanel, LessonStep step)
        {
            var container = FindValidationOptionsContainer(parentPanel);
            if (container == null) return;
            container.Children.Clear();
            container.Children.Add(CreateConfidenceField(
                "Точность совпадения (0.6 - 0.9):",
                step.VisionConfidence,
                val => step.VisionConfidence = val,
                "Задайте минимальную точность совпадения изображения (от 0.6 до 0.9)"
            ));
        }

        private void ShowValidationFolderOptions(StackPanel parentPanel, LessonStep step)
        {
            var container = FindValidationOptionsContainer(parentPanel);
            if (container == null) return;
            container.Children.Clear();
            container.Children.Add(CreateIntegerField(
                "Необходимое количество совпадений:",
                step.RequiredMatches,
                val => step.RequiredMatches = val,
                "Сколько элементов из папки должно быть найдено одновременно (минимум 1)"
            ));
            container.Children.Add(CreateConfidenceField(
                "Точность совпадения (0.6 - 0.9):",
                step.VisionConfidence,
                val => step.VisionConfidence = val,
                "Задайте минимальную точность для поиска элементов из папки"
            ));
        }

        private void HideValidationOptions(StackPanel parentPanel)
        {
            var container = FindValidationOptionsContainer(parentPanel);
            container?.Children.Clear();
        }

        private StackPanel FindValidationOptionsContainer(StackPanel parentPanel)
        {
            foreach (var child in parentPanel.Children)
            {
                if (child is StackPanel sp && sp.Name == "ValidationOptionsContainer")
                    return sp;
            }
            return null;
        }

        private string GetValidationDisplayPath(LessonStep step)
        {
            if (!string.IsNullOrEmpty(step.SelectedFilePath))
                return Path.GetFileName(step.SelectedFilePath);
            if (!string.IsNullOrEmpty(step.SelectedFolderPath))
                return Path.GetFileName(step.SelectedFolderPath);
            return "Ничего не выбрано";
        }

        private StackPanel CreateHintSourcePanel(LessonStep step, int stepIndex,
            Action<int, string> onFileSelected, Action<int> onFileCleared,
            Action<int, string> onFolderSelected, Action<int> onFolderCleared)
        {
            var panel = new StackPanel { Margin = new Thickness(0, 0, 0, 20) };
            var header = CreateHeader("Image", "Изображение для визуальной подсказки (выберите файл или папку)");

            var sourceGrid = new Grid();
            sourceGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            sourceGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var pathText = new TextBlock
            {
                Text = GetHintDisplayPath(step),
                Style = (Style)_resourceParent.FindResource("BodyTextStyle"),
                Foreground = (Brush)_resourceParent.FindResource("TextSecondaryBrush"),
                VerticalAlignment = VerticalAlignment.Center
            };
            var pathBorder = new Border
            {
                Background = (Brush)_resourceParent.FindResource("BackgroundLightBrush"),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(16),
                BorderBrush = (Brush)_resourceParent.FindResource("SurfaceBorderBrush"),
                BorderThickness = new Thickness(1),
                Child = pathText
            };
            Grid.SetColumn(pathBorder, 0);

            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(12, 0, 0, 0) };
            Grid.SetColumn(buttonPanel, 1);

            var clearButton = CreateIconButton("Trash", "Очистить", null);
            clearButton.Visibility = (step.SelectedHintFilePath != null || step.SelectedHintFolderPath != null) ? Visibility.Visible : Visibility.Collapsed;
            clearButton.Click += (s, e) =>
            {
                step.SelectedHintFilePath = null;
                step.SelectedHintFolderPath = null;
                step.VisionHint = null;
                step.VisionHintFolder = null;
                step.ShowHint = false;
                pathText.Text = "Ничего не выбрано";
                clearButton.Visibility = Visibility.Collapsed;
                HideHintOptions(panel);
                onFileCleared?.Invoke(stepIndex);
                onFolderCleared?.Invoke(stepIndex);
            };

            var fileButton = CreateIconButton("Image", "Выбрать файл", () =>
            {
                var dialog = new OpenFileDialog
                {
                    Filter = "Изображения (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp|Все файлы (*.*)|*.*",
                    Title = "Выберите изображение для подсказки"
                };
                if (dialog.ShowDialog() == true)
                {
                    step.SelectedHintFilePath = dialog.FileName;
                    step.VisionHint = Path.GetFileNameWithoutExtension(dialog.FileName);
                    step.ShowHint = true;
                    step.SelectedHintFolderPath = null;
                    step.VisionHintFolder = null;
                    pathText.Text = Path.GetFileName(dialog.FileName);
                    clearButton.Visibility = Visibility.Visible;
                    ShowHintFileOptions(panel, step);
                    onFileSelected?.Invoke(stepIndex, dialog.FileName);
                    onFolderCleared?.Invoke(stepIndex);
                }
            });

            var folderButton = CreateIconButton("Folder", "Выбрать папку", () =>
            {
                var dialog = new System.Windows.Forms.FolderBrowserDialog();
                dialog.Description = "Выберите папку с PNG-шаблонами для подсказки";
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    step.SelectedHintFolderPath = dialog.SelectedPath;
                    step.VisionHintFolder = Path.GetFileName(dialog.SelectedPath);
                    step.ShowHint = true;
                    step.SelectedHintFilePath = null;
                    step.VisionHint = null;
                    pathText.Text = Path.GetFileName(dialog.SelectedPath);
                    clearButton.Visibility = Visibility.Visible;
                    ShowHintFolderOptions(panel, step);
                    onFolderSelected?.Invoke(stepIndex, dialog.SelectedPath);
                    onFileCleared?.Invoke(stepIndex);
                }
            });

            buttonPanel.Children.Add(clearButton);
            buttonPanel.Children.Add(fileButton);
            buttonPanel.Children.Add(folderButton);

            sourceGrid.Children.Add(pathBorder);
            sourceGrid.Children.Add(buttonPanel);

            panel.Children.Add(header);
            panel.Children.Add(sourceGrid);

            var optionsContainer = new StackPanel { Name = "HintOptionsContainer", Margin = new Thickness(0, 12, 0, 0) };
            panel.Children.Add(optionsContainer);

            if (step.SelectedHintFilePath != null)
                ShowHintFileOptions(panel, step);
            else if (step.SelectedHintFolderPath != null)
                ShowHintFolderOptions(panel, step);

            return panel;
        }

        private void ShowHintFileOptions(StackPanel parentPanel, LessonStep step)
        {
            var container = FindHintOptionsContainer(parentPanel);
            if (container == null) return;
            container.Children.Clear();
            container.Children.Add(CreateConfidenceField(
                "Точность подсказки (0.6 - 0.9):",
                step.HintConfidence,
                val => step.HintConfidence = val,
                "Задайте точность для поиска изображения-подсказки (от 0.6 до 0.9)"
            ));
            container.Children.Add(CreateHintTypeSelector(step));
        }

        private void ShowHintFolderOptions(StackPanel parentPanel, LessonStep step)
        {
            var container = FindHintOptionsContainer(parentPanel);
            if (container == null) return;
            container.Children.Clear();
            container.Children.Add(CreateIntegerField(
                "Необходимое количество совпадений:",
                step.RequiredHintMatches,
                val => step.RequiredHintMatches = val,
                "Сколько элементов из папки должно быть найдено для показа подсказки (минимум 1)"
            ));
            container.Children.Add(CreateConfidenceField(
                "Точность подсказки (0.6 - 0.9):",
                step.HintConfidence,
                val => step.HintConfidence = val,
                "Задайте точность для поиска изображений в папке (от 0.6 до 0.9)"
            ));
            container.Children.Add(CreateHintTypeSelector(step));
        }

        private void HideHintOptions(StackPanel parentPanel)
        {
            var container = FindHintOptionsContainer(parentPanel);
            container?.Children.Clear();
        }

        private StackPanel FindHintOptionsContainer(StackPanel parentPanel)
        {
            foreach (var child in parentPanel.Children)
            {
                if (child is StackPanel sp && sp.Name == "HintOptionsContainer")
                    return sp;
            }
            return null;
        }

        private string GetHintDisplayPath(LessonStep step)
        {
            if (!string.IsNullOrEmpty(step.SelectedHintFilePath))
                return Path.GetFileName(step.SelectedHintFilePath);
            if (!string.IsNullOrEmpty(step.SelectedHintFolderPath))
                return Path.GetFileName(step.SelectedHintFolderPath);
            return "Ничего не выбрано";
        }

        private StackPanel CreateHeader(string iconName, string text)
        {
            var stack = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };
            var icon = new Image
            {
                Tag = iconName,
                Width = 16,
                Height = 16,
                Margin = new Thickness(0, 0, 8, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            AppThemeManager.UpdateImageSource(icon, iconName);
            var label = new TextBlock
            {
                Text = text,
                Style = (Style)_resourceParent.FindResource("BodyTextStyle"),
                FontWeight = FontWeights.Medium
            };
            stack.Children.Add(icon);
            stack.Children.Add(label);
            return stack;
        }

        private Button CreateIconButton(string iconName, string text, Action onClick)
        {
            var button = new Button
            {
                Style = (Style)_resourceParent.FindResource("NavigationButtonStyle")
            };
            var stack = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
            var icon = new Image
            {
                Tag = iconName,
                Width = 18,
                Height = 18,
                Margin = new Thickness(0, 0, 6, 0)
            };
            AppThemeManager.UpdateImageSource(icon, iconName);
            var label = new TextBlock
            {
                Text = text,
                FontSize = 14,
                VerticalAlignment = VerticalAlignment.Center
            };
            stack.Children.Add(icon);
            stack.Children.Add(label);
            button.Content = stack;
            if (onClick != null)
                button.Click += (s, e) => onClick();
            return button;
        }

        private StackPanel CreateHintTypeSelector(LessonStep step)
        {
            var panel = new StackPanel { Margin = new Thickness(0, 20, 0, 0) };
            var header = CreateHeader("HintType", "Тип подсказки");
            panel.Children.Add(header);

            var types = new Dictionary<string, string>
            {
                { "rectangle", "Прямоугольник" },
                { "arrow", "Стрелка" },
                { "corner", "Уголок" },
                { "dim", "Затемнение" }
            };

            var wrapPanel = new WrapPanel { Margin = new Thickness(0, 8, 0, 0) };
            var buttons = new Dictionary<string, Button>();

            foreach (var type in types)
            {
                var button = new Button
                {
                    Style = (Style)_resourceParent.FindResource("HintTypeButtonStyle"),
                    Tag = type.Key,
                    Cursor = Cursors.Hand,
                    UseLayoutRounding = true
                };

                var stack = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    VerticalAlignment = VerticalAlignment.Center
                };

                var icon = new Image
                {
                    Tag = HintTypeIconNames[type.Key],
                    Width = 18,
                    Height = 18,
                    Margin = new Thickness(0, 0, 8, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };
                AppThemeManager.UpdateImageSource(icon, HintTypeIconNames[type.Key]);

                var text = new TextBlock
                {
                    Text = type.Value,
                    FontSize = 14,
                    FontWeight = FontWeights.Medium,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = (Brush)_resourceParent.FindResource("TextPrimaryBrush")
                };

                stack.Children.Add(icon);
                stack.Children.Add(text);
                button.Content = stack;

                button.Click += (s, e) =>
                {
                    foreach (var btn in buttons.Values)
                    {
                        btn.Style = (Style)_resourceParent.FindResource("HintTypeButtonStyle");
                    }
                    button.Style = (Style)_resourceParent.FindResource("ActiveHintTypeButtonStyle");
                    step.HintType = (string)button.Tag;
                };

                if (step.HintType == type.Key)
                {
                    button.Style = (Style)_resourceParent.FindResource("ActiveHintTypeButtonStyle");
                }

                buttons[type.Key] = button;
                wrapPanel.Children.Add(button);
            }

            panel.Children.Add(wrapPanel);
            return panel;
        }

        private StackPanel CreateConfidenceField(string labelText, double initialValue, Action<double> onValueChanged, string tooltip)
        {
            var panel = new StackPanel { Margin = new Thickness(0, 12, 0, 0) };

            var header = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 4) };
            var icon = new Image
            {
                Tag = "Confidence",
                Width = 16,
                Height = 16,
                Margin = new Thickness(0, 0, 8, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            AppThemeManager.UpdateImageSource(icon, "Chart");
            var label = new TextBlock
            {
                Text = labelText,
                Style = (Style)_resourceParent.FindResource("BodyTextStyle"),
                FontWeight = FontWeights.Medium
            };
            header.Children.Add(icon);
            header.Children.Add(label);

            var textBox = new TextBox
            {
                Style = (Style)_resourceParent.FindResource("RoundedTextBox"),
                Height = 48,
                Width = 120,
                HorizontalAlignment = HorizontalAlignment.Left,
                Text = initialValue.ToString("F2"),
                ToolTip = tooltip
            };

            textBox.LostFocus += (s, e) =>
            {
                if (double.TryParse(textBox.Text, out double value))
                {
                    if (value < 0.6) value = 0.6;
                    if (value > 0.9) value = 0.9;
                    textBox.Text = value.ToString("F2");
                    onValueChanged(value);
                }
                else
                {
                    textBox.Text = initialValue.ToString("F2");
                    onValueChanged(initialValue);
                }
            };

            panel.Children.Add(header);
            panel.Children.Add(textBox);
            return panel;
        }

        private StackPanel CreateIntegerField(string labelText, int initialValue, Action<int> onValueChanged, string tooltip)
        {
            var panel = new StackPanel { Margin = new Thickness(0, 12, 0, 0) };

            var header = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 4) };
            var icon = new Image
            {
                Tag = "Number",
                Width = 16,
                Height = 16,
                Margin = new Thickness(0, 0, 8, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            AppThemeManager.UpdateImageSource(icon, "List");
            var label = new TextBlock
            {
                Text = labelText,
                Style = (Style)_resourceParent.FindResource("BodyTextStyle"),
                FontWeight = FontWeights.Medium
            };
            header.Children.Add(icon);
            header.Children.Add(label);

            var textBox = new TextBox
            {
                Style = (Style)_resourceParent.FindResource("RoundedTextBox"),
                Height = 48,
                Width = 120,
                HorizontalAlignment = HorizontalAlignment.Left,
                Text = initialValue.ToString(),
                ToolTip = tooltip
            };

            textBox.LostFocus += (s, e) =>
            {
                if (int.TryParse(textBox.Text, out int value))
                {
                    if (value < 1) value = 1;
                    textBox.Text = value.ToString();
                    onValueChanged(value);
                }
                else
                {
                    textBox.Text = initialValue.ToString();
                    onValueChanged(initialValue);
                }
            };

            panel.Children.Add(header);
            panel.Children.Add(textBox);
            return panel;
        }
    }
}