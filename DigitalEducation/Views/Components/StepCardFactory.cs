using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DigitalEducation.Pages.CreateCustomLesson
{
    public class StepCardFactory
    {
        private readonly FrameworkElement _resourceParent;

        public StepCardFactory(FrameworkElement resourceParent)
        {
            _resourceParent = resourceParent ?? throw new ArgumentNullException(nameof(resourceParent));
        }

        public Border CreateStepCard(
            LessonStep step,
            int stepNumber,
            int stepIndex,
            Action<int> onDelete,
            Action<int, string> onImageSelected,
            Action<int> onImageCleared)
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

            var titleStack = CreateStepTitleStack(step);
            Grid.SetColumn(titleStack, 1);

            var deleteButton = CreateDeleteButton(stepIndex, onDelete);
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
            var imagePanel = CreateImagePanel(step, stepIndex, onImageSelected, onImageCleared);

            contentStack.Children.Add(descriptionPanel);
            contentStack.Children.Add(hintPanel);
            contentStack.Children.Add(imagePanel);

            mainStack.Children.Add(titleGrid);
            mainStack.Children.Add(contentStack);

            card.Child = mainStack;
            return card;
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
            var titleStack = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center
            };

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

        private StackPanel CreateDescriptionPanel(LessonStep step, int stepIndex)
        {
            var panel = new StackPanel
            {
                Margin = new Thickness(0, 0, 0, 20)
            };

            var header = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 8)
            };

            var icon = new Image
            {
                Tag = "Info",
                Width = 16,
                Height = 16,
                Margin = new Thickness(0, 0, 8, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            ThemeManager.UpdateImageSource(icon, "Info");

            var label = new TextBlock
            {
                Text = "Описание шага *",
                Style = (Style)_resourceParent.FindResource("BodyTextStyle"),
                FontWeight = FontWeights.Medium
            };

            header.Children.Add(icon);
            header.Children.Add(label);

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
            var panel = new StackPanel
            {
                Margin = new Thickness(0, 0, 0, 20)
            };

            var header = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 8)
            };

            var icon = new Image
            {
                Tag = "Info",
                Width = 16,
                Height = 16,
                Margin = new Thickness(0, 0, 8, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            ThemeManager.UpdateImageSource(icon, "Info");

            var label = new TextBlock
            {
                Text = "Подсказка (необязательно)",
                Style = (Style)_resourceParent.FindResource("BodyTextStyle"),
                FontWeight = FontWeights.Medium
            };

            header.Children.Add(icon);
            header.Children.Add(label);

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
            var panel = new StackPanel
            {
                Margin = new Thickness(0, 0, 0, 0)
            };

            var header = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 8)
            };

            var icon = new Image
            {
                Tag = "Folder",
                Width = 18,
                Height = 18,
                Margin = new Thickness(0, 0, 8, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            ThemeManager.UpdateImageSource(icon, "Folder");

            var label = new TextBlock
            {
                Text = "Изображение для проверки (необязательно)",
                Style = (Style)_resourceParent.FindResource("BodyTextStyle"),
                FontWeight = FontWeights.Medium
            };

            header.Children.Add(icon);
            header.Children.Add(label);

            var fileContainer = new Grid();
            fileContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            fileContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

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
                Text = string.IsNullOrEmpty(step.VisionTarget) ? "Файл не выбран" : Path.GetFileName(step.VisionTarget),
                Style = (Style)_resourceParent.FindResource("BodyTextStyle"),
                Foreground = (Brush)_resourceParent.FindResource("TextSecondaryBrush"),
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

            var clearButton = CreateClearImageButton(step, stepIndex, fileInfoText, onImageCleared);
            var selectButton = CreateSelectImageButton(step, stepIndex, fileInfoText, clearButton, onImageSelected);

            buttonPanel.Children.Add(clearButton);
            buttonPanel.Children.Add(selectButton);

            fileContainer.Children.Add(fileInfoBorder);
            fileContainer.Children.Add(buttonPanel);

            panel.Children.Add(header);
            panel.Children.Add(fileContainer);
            return panel;
        }

        private Button CreateClearImageButton(LessonStep step, int stepIndex, TextBlock fileInfoText,
            Action<int> onImageCleared)
        {
            var button = new Button
            {
                Style = (Style)_resourceParent.FindResource("NavigationButtonStyle"),
                Margin = new Thickness(0, 0, 8, 0),
                Visibility = string.IsNullOrEmpty(step.VisionTarget) ? Visibility.Collapsed : Visibility.Visible
            };

            var icon = new Image
            {
                Tag = "Trash",
                Width = 18,
                Height = 18,
                Margin = new Thickness(0, 0, 6, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            ThemeManager.UpdateImageSource(icon, "Trash");

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
                step.VisionTarget = "";
                step.RequiresVisionValidation = false;
                fileInfoText.Text = "Файл не выбран";
                button.Visibility = Visibility.Collapsed;
                onImageCleared?.Invoke(stepIndex);
            };

            return button;
        }

        private Button CreateSelectImageButton(LessonStep step, int stepIndex, TextBlock fileInfoText,
            Button clearButton, Action<int, string> onImageSelected)
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
            ThemeManager.UpdateImageSource(icon, "Folder");

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
                    Title = "Выберите изображение для проверки"
                };

                if (dialog.ShowDialog() == true)
                {
                    step.VisionTarget = dialog.FileName;
                    step.RequiresVisionValidation = true;
                    fileInfoText.Text = Path.GetFileName(step.VisionTarget);
                    clearButton.Visibility = Visibility.Visible;
                    onImageSelected?.Invoke(stepIndex, dialog.FileName);
                }
            };

            return button;
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
            ThemeManager.UpdateImageSource(icon, "Trash");

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
    }
}