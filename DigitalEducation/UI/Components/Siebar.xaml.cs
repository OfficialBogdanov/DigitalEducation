using DigitalEducation.Core.Managers;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DigitalEducation.UI.Components
{
    public partial class Sidebar : UserControl
    {
        private string _currentPage = "home";
        private Dictionary<Button, TextBlock> _navTextBlocks = new Dictionary<Button, TextBlock>();
        private Dictionary<Button, Path> _navPaths = new Dictionary<Button, Path>();

        public static readonly DependencyProperty CurrentPageProperty =
            DependencyProperty.Register("CurrentPage", typeof(string), typeof(Sidebar),
                new PropertyMetadata("home", OnCurrentPageChanged));

        public string CurrentPage
        {
            get { return (string)GetValue(CurrentPageProperty); }
            set { SetValue(CurrentPageProperty, value); }
        }

        private static void OnCurrentPageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Sidebar sidebar = d as Sidebar;
            string page = e.NewValue as string;
            if (!string.IsNullOrEmpty(page))
            {
                sidebar?.SetActivePage(page);
            }
        }

        public Sidebar()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            Loaded += (s, e) => CacheElements();
            SubscribeHoverEvents();

            Theme.ThemeChanged += OnThemeChanged;
            Palette.PaletteChanged += OnPaletteChanged;
        }

        private void OnThemeChanged(object sender, EventArgs e)
        {
            UpdateAllItems();
        }

        private void OnPaletteChanged(object sender, EventArgs e)
        {
            UpdateAllItems();
        }

        private void UpdateAllItems()
        {
            foreach (KeyValuePair<Button, TextBlock> kvp in _navTextBlocks)
            {
                Button button = kvp.Key;
                TextBlock textBlock = kvp.Value;
                bool isActive = button.Tag?.ToString() == "True";

                textBlock.Foreground = isActive ?
                    (Brush)FindResource("TextPrimary") :
                    (Brush)FindResource("TextSecondary");
            }

            foreach (KeyValuePair<Button, Path> kvp in _navPaths)
            {
                Button button = kvp.Key;
                Path path = kvp.Value;
                bool isActive = button.Tag?.ToString() == "True";

                path.Stroke = isActive ?
                    (Brush)FindResource("TextPrimary") :
                    (Brush)FindResource("TextSecondary");
            }

            ResetLogoutButton();
        }

        private void CacheElements()
        {
            (Button btn, TextBlock text, Path path)[] navButtons = new (Button, TextBlock, Path)[]
            {
                (NavHome, HomeText, HomePath),
                (NavCourses, CoursesText, CoursesPath),
                (NavConstructor, ConstructorText, ConstructorPath),
                (NavProgress, ProgressText, ProgressPath),
                (NavAchievements, AchievementsText, AchievementsPath),
                (NavSettings, SettingsText, SettingsPath)
            };

            foreach (var item in navButtons)
            {
                _navTextBlocks[item.btn] = item.text;
                _navPaths[item.btn] = item.path;
            }
        }

        private void SubscribeHoverEvents()
        {
            Button[] navButtons = new Button[]
            {
                NavHome, NavCourses, NavConstructor, NavProgress, NavAchievements, NavSettings
            };

            foreach (Button btn in navButtons)
            {
                btn.MouseEnter += OnNavButtonMouseEnter;
                btn.MouseLeave += OnNavButtonMouseLeave;
            }

            LogoutButton.MouseEnter += OnLogoutButtonMouseEnter;
            LogoutButton.MouseLeave += OnLogoutButtonMouseLeave;
        }

        private void OnNavButtonMouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Button button = sender as Button;
            if (button == null) return;

            if (_navTextBlocks.TryGetValue(button, out TextBlock textBlock) && textBlock != null)
            {
                textBlock.Foreground = (Brush)FindResource("TextPrimary");
            }

            if (_navPaths.TryGetValue(button, out Path path) && path != null)
            {
                path.Stroke = (Brush)FindResource("TextPrimary");
            }
        }

        private void OnNavButtonMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Button button = sender as Button;
            if (button == null) return;

            bool isActive = button.Tag?.ToString() == "True";

            if (_navTextBlocks.TryGetValue(button, out TextBlock textBlock) && textBlock != null)
            {
                textBlock.Foreground = isActive ?
                    (Brush)FindResource("TextPrimary") :
                    (Brush)FindResource("TextSecondary");
            }

            if (_navPaths.TryGetValue(button, out Path path) && path != null)
            {
                path.Stroke = isActive ?
                    (Brush)FindResource("TextPrimary") :
                    (Brush)FindResource("TextSecondary");
            }
        }

        private void OnLogoutButtonMouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (LogoutText != null)
            {
                LogoutText.Foreground = (Brush)FindResource("LogoutTextHover");
            }

            if (LogoutPath != null)
            {
                LogoutPath.Stroke = (Brush)FindResource("LogoutTextHover");
            }

            LogoutButton.Tag = "True";
        }

        private void OnLogoutButtonMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (LogoutText != null)
            {
                LogoutText.Foreground = (Brush)FindResource("TextSecondary");
            }

            if (LogoutPath != null)
            {
                LogoutPath.Stroke = (Brush)FindResource("TextSecondary");
            }

            LogoutButton.Tag = "False";
        }

        private void ResetLogoutButton()
        {
            if (LogoutText != null)
            {
                LogoutText.Foreground = (Brush)FindResource("TextSecondary");
            }

            if (LogoutPath != null)
            {
                LogoutPath.Stroke = (Brush)FindResource("TextSecondary");
            }

            LogoutButton.Tag = "False";
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            CacheElements();
            SubscribeHoverEvents();

            if (!string.IsNullOrEmpty(CurrentPage))
            {
                SetActivePage(CurrentPage);
            }
            else
            {
                SetActivePage("home");
            }
        }

        public void SetActivePage(string page)
        {
            _currentPage = page;

            (Button btn, string page)[] buttons = new (Button, string)[]
            {
                (NavHome, "home"),
                (NavCourses, "courses"),
                (NavConstructor, "constructor"),
                (NavProgress, "progress"),
                (NavAchievements, "achievements"),
                (NavSettings, "settings")
            };

            foreach (var item in buttons)
            {
                bool isActive = item.page == page;
                item.btn.Tag = isActive.ToString();

                if (_navTextBlocks.TryGetValue(item.btn, out TextBlock textBlock) && textBlock != null)
                {
                    textBlock.Foreground = isActive ?
                        (Brush)FindResource("TextPrimary") :
                        (Brush)FindResource("TextSecondary");
                }

                if (_navPaths.TryGetValue(item.btn, out Path path) && path != null)
                {
                    path.Stroke = isActive ?
                        (Brush)FindResource("TextPrimary") :
                        (Brush)FindResource("TextSecondary");
                }
            }

            ResetLogoutButton();
        }

        private void Logo_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            PageChangedEventArgs args = new PageChangedEventArgs("home");
            PageChanged?.Invoke(this, args);
        }

        private void NavItem_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            string page = button?.DataContext?.ToString();

            if (!string.IsNullOrEmpty(page) && page != _currentPage)
            {
                SetActivePage(page);
                CurrentPage = page;
                PageChanged?.Invoke(this, new PageChangedEventArgs(page));
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            LogoutRequested?.Invoke(this, e);
        }

        public event PageChangedEventHandler PageChanged;
        public event RoutedEventHandler LogoutRequested;
    }

    public delegate void PageChangedEventHandler(object sender, PageChangedEventArgs e);

    public class PageChangedEventArgs : RoutedEventArgs
    {
        public string Page { get; }

        public PageChangedEventArgs(string page)
        {
            Page = page;
        }
    }
}