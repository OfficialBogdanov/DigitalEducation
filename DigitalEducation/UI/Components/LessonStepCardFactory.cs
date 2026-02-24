using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DigitalEducation.Pages.CreateCustomLesson
{
    public class LessonStepCardFactory
    {
        private readonly FrameworkElement _resourceParent;
        private static readonly Dictionary<string, string> HintTypeIconNames = new Dictionary<string, string>
        {
            { "rectangle", "Rectangle" },
            { "arrow", "Arrow" },
            { "highlight", "Highlight" },
            { "corner", "Corner" },
            { "glow", "Glow" },
            { "dim", "Dim" }
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
            Action<int, string> onImageSelected,
            Action<int> onImageCleared,
            Action<int, string> onHintImageSelected,
            Action<int> onHintImageCleared)
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
            contentStack.Children.Add(CreateImagePanel(step, stepIndex, onImageSelected, onImageCleared));

            contentStack.Children.Add(CreateHintImagePanel(step, stepIndex, onHintImageSelected, onHintImageCleared));

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

        private StackPanel CreateImagePanel(LessonStep step, int stepIndex,
            Action<int, string> onImageSelected, Action<int> onImageCleared)
        {
            var panel = new StackPanel { Margin = new Thickness(0, 0, 0, 20) };
            var header = CreateHeader("Folder", "Изображение для проверки (необязательно)");

            var fileContainer = CreateFileSelector(
                step.VisionTarget,
                (file) => { step.VisionTarget = file; step.RequiresVisionValidation = true; },
                () => { step.VisionTarget = ""; step.RequiresVisionValidation = false; },
                stepIndex,
                onImageSelected,
                onImageCleared
            );

            panel.Children.Add(header);
            panel.Children.Add(fileContainer);
            return panel;
        }

        private StackPanel CreateHintImagePanel(LessonStep step, int stepIndex,
            Action<int, string> onHintImageSelected, Action<int> onHintImageCleared)
        {
            var panel = new StackPanel { Margin = new Thickness(0, 0, 0, 20) };
            var header = CreateHeader("Folder", "Изображение для визуальной подсказки (необязательно)");

            var fileContainer = CreateFileSelector(
                step.HintImagePath, 
                (file) => { step.HintImagePath = file; step.ShowHint = true; },
                () => { step.HintImagePath = ""; step.ShowHint = false; step.VisionHint = ""; },
                stepIndex,
                onHintImageSelected,
                onHintImageCleared
            );

            var typeSelector = CreateHintTypeSelector(step);

            panel.Children.Add(header);
            panel.Children.Add(fileContainer);
            panel.Children.Add(typeSelector);
            return panel;
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

        private Grid CreateFileSelector(string currentFilePath, Action<string> onFileSelected, Action onFileCleared,
            int stepIndex, Action<int, string> onImageSelected, Action<int> onImageCleared)
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var fileInfoBorder = new Border
            {
                Background = (Brush)_resourceParent.FindResource("BackgroundLightBrush"),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(16),
                BorderBrush = (Brush)_resourceParent.FindResource("SurfaceBorderBrush"),
                BorderThickness = new Thickness(1)
            };
            var fileInfoText = new TextBlock
            {
                Text = string.IsNullOrEmpty(currentFilePath) ? "Файл не выбран" : Path.GetFileName(currentFilePath),
                Style = (Style)_resourceParent.FindResource("BodyTextStyle"),
                Foreground = (Brush)_resourceParent.FindResource("TextSecondaryBrush"),
                VerticalAlignment = VerticalAlignment.Center
            };
            fileInfoBorder.Child = fileInfoText;
            Grid.SetColumn(fileInfoBorder, 0);

            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(12, 0, 0, 0) };
            Grid.SetColumn(buttonPanel, 1);

            var clearButton = CreateClearButton(currentFilePath, fileInfoText, onFileCleared, stepIndex, onImageCleared);
            var selectButton = CreateSelectButton(fileInfoText, clearButton, onFileSelected, stepIndex, onImageSelected);

            buttonPanel.Children.Add(clearButton);
            buttonPanel.Children.Add(selectButton);

            grid.Children.Add(fileInfoBorder);
            grid.Children.Add(buttonPanel);
            return grid;
        }

        private Button CreateClearButton(string currentFilePath, TextBlock fileInfoText, Action onFileCleared,
            int stepIndex, Action<int> onImageCleared)
        {
            var button = new Button
            {
                Style = (Style)_resourceParent.FindResource("NavigationButtonStyle"),
                Margin = new Thickness(0, 0, 8, 0),
                Visibility = string.IsNullOrEmpty(currentFilePath) ? Visibility.Collapsed : Visibility.Visible
            };

            var icon = new Image
            {
                Tag = "Trash",
                Width = 18,
                Height = 18,
                Margin = new Thickness(0, 0, 6, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            AppThemeManager.UpdateImageSource(icon, "Trash");

            var text = new TextBlock
            {
                Text = "Очистить",
                FontSize = 14,
                VerticalAlignment = VerticalAlignment.Center
            };

            var stack = new StackPanel { Orientation = Orientation.Horizontal };
            stack.Children.Add(icon);
            stack.Children.Add(text);
            button.Content = stack;

            button.Click += (s, e) =>
            {
                onFileCleared?.Invoke();
                fileInfoText.Text = "Файл не выбран";
                button.Visibility = Visibility.Collapsed;
                onImageCleared?.Invoke(stepIndex);
            };

            return button;
        }

        private Button CreateSelectButton(TextBlock fileInfoText, Button clearButton, Action<string> onFileSelected,
            int stepIndex, Action<int, string> onImageSelected)
        {
            var button = new Button
            {
                Style = (Style)_resourceParent.FindResource("NavigationButtonStyle")
            };

            var icon = new Image
            {
                Tag = "Folder",
                Width = 18,
                Height = 18,
                Margin = new Thickness(0, 0, 6, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            AppThemeManager.UpdateImageSource(icon, "Folder");

            var text = new TextBlock
            {
                Text = "Выбрать файл",
                FontSize = 14,
                VerticalAlignment = VerticalAlignment.Center
            };

            var stack = new StackPanel { Orientation = Orientation.Horizontal };
            stack.Children.Add(icon);
            stack.Children.Add(text);
            button.Content = stack;

            button.Click += (s, e) =>
            {
                var dialog = new OpenFileDialog
                {
                    Filter = "Изображения (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp|Все файлы (*.*)|*.*",
                    Title = "Выберите изображение"
                };

                if (dialog.ShowDialog() == true)
                {
                    onFileSelected?.Invoke(dialog.FileName);
                    fileInfoText.Text = Path.GetFileName(dialog.FileName);
                    clearButton.Visibility = Visibility.Visible;
                    onImageSelected?.Invoke(stepIndex, dialog.FileName);
                }
            };

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
                { "highlight", "Подсветка" },
                { "corner", "Уголок" },
                { "glow", "Свечение" },
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
    }
}