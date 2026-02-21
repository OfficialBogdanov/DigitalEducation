using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DigitalEducation
{
    public class LessonCardFactory : ILessonCardFactory
    {
        private readonly FrameworkElement _resourceParent;

        public LessonCardFactory(FrameworkElement resourceParent)
        {
            _resourceParent = resourceParent ?? throw new ArgumentNullException(nameof(resourceParent));
        }

        public FrameworkElement CreateLessonCard(
            LessonData lesson,
            int number,
            Action<LessonData> onEdit,
            Action<LessonData> onDelete,
            Action<LessonData> onStart)
        {
            var card = new Border
            {
                Style = (Style)_resourceParent.FindResource("CardStyle"),
                Margin = new Thickness(0, 0, 0, 24),
                UseLayoutRounding = true,
                SnapsToDevicePixels = true
            };

            var grid = new Grid { UseLayoutRounding = true };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
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
                Text = number.ToString(),
                FontSize = 32,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            numberBorder.Child = numberText;
            Grid.SetColumn(numberBorder, 0);

            var stackPanel = new StackPanel { UseLayoutRounding = true };
            var titleText = new TextBlock
            {
                Text = string.IsNullOrEmpty(lesson.Title) ? "Без названия" : lesson.Title,
                FontSize = 16,
                FontWeight = FontWeights.Medium,
                Foreground = new SolidColorBrush(GetLessonColor(lesson.Id)),
                Margin = new Thickness(0, 0, 0, 8)
            };
            var stepsCountText = new TextBlock
            {
                Text = $"Шагов: {lesson.Steps?.Count ?? 0}",
                Style = (Style)_resourceParent.FindResource("BodyTextStyle"),
                Margin = new Thickness(0, 0, 0, 12),
                FontSize = 13
            };
            stackPanel.Children.Add(titleText);
            stackPanel.Children.Add(stepsCountText);
            Grid.SetColumn(stackPanel, 1);

            var editButton = CreateActionButton("Редактировать", "Edit", () => onEdit?.Invoke(lesson));
            Grid.SetColumn(editButton, 2);

            var deleteButton = CreateDeleteButton("Удалить", "Trash", () => onDelete?.Invoke(lesson));
            Grid.SetColumn(deleteButton, 3);

            var startButton = CreateStartButton("Начать", () => onStart?.Invoke(lesson));
            Grid.SetColumn(startButton, 4);

            grid.Children.Add(numberBorder);
            grid.Children.Add(stackPanel);
            grid.Children.Add(editButton);
            grid.Children.Add(deleteButton);
            grid.Children.Add(startButton);

            card.Child = grid;
            return card;
        }

        private Button CreateActionButton(string text, string iconName, Action onClick)
        {
            var button = new Button
            {
                Style = (Style)_resourceParent.FindResource("NavigationButtonStyle"),
                Height = 80,
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
                Foreground = (Brush)_resourceParent.FindResource("TextPrimaryBrush")
            };

            stackPanel.Children.Add(icon);
            stackPanel.Children.Add(buttonText);
            button.Content = stackPanel;

            button.Click += (s, e) => onClick?.Invoke();
            return button;
        }

        private Button CreateDeleteButton(string text, string iconName, Action onClick)
        {
            var button = new Button
            {
                Style = (Style)_resourceParent.FindResource("NavigationButtonStyle"),
                Height = 80,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(12, 0, 0, 0),
                UseLayoutRounding = true,
                Tag = iconName,
                ToolTip = "Удалить урок"
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
                Foreground = (Brush)_resourceParent.FindResource("TextPrimaryBrush")
            };

            stackPanel.Children.Add(icon);
            stackPanel.Children.Add(buttonText);
            button.Content = stackPanel;

            button.Click += (s, e) => onClick?.Invoke();
            return button;
        }

        private Button CreateStartButton(string text, Action onClick)
        {
            var button = new Button
            {
                Style = (Style)_resourceParent.FindResource("NavigationButtonStyle"),
                Height = 80,
                VerticalAlignment = VerticalAlignment.Center,
                Padding = new Thickness(24, 0, 0, 0),
                Margin = new Thickness(12, 0, 0, 0),
                UseLayoutRounding = true,
                Tag = "Right"
            };

            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                UseLayoutRounding = true,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var icon = new Image
            {
                Tag = "Right",
                Width = 24,
                Height = 24,
                Margin = new Thickness(0, 0, 8, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            ThemeManager.UpdateImageSource(icon, "Right");

            var buttonText = new TextBlock
            {
                Text = text,
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.Medium,
                Foreground = (Brush)_resourceParent.FindResource("TextPrimaryBrush")
            };

            stackPanel.Children.Add(icon);
            stackPanel.Children.Add(buttonText);
            button.Content = stackPanel;

            button.Click += (s, e) => onClick?.Invoke();
            return button;
        }

        private Color GetLessonColor(string lessonId)
        {
            Color[] paletteColors = new Color[]
            {
                (Color)_resourceParent.FindResource("PrimaryColor"),
                (Color)_resourceParent.FindResource("SuccessColor"),
                (Color)_resourceParent.FindResource("WarningColor"),
                (Color)_resourceParent.FindResource("ErrorColor")
            };

            int hash = Math.Abs(lessonId.GetHashCode());
            int colorIndex = hash % paletteColors.Length;
            return paletteColors[colorIndex];
        }
    }
}