using DigitalEducation.Core.Managers;
using DigitalEducation.UI.Components;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace DigitalEducation.UI.Pages
{
    public class Base
    {
        private Window _window;
        private string _currentPage;
        private bool _isSidebarOpen = false;
        private BlurEffect _contentBlur;
        private Grid _sidebarContainer;
        private Button _menuToggle;
        private Border _overlay;
        private Modal _modalWindow;
        private Sidebar _sidebarControl;
        private Grid _contentContainer;

        public Base(Window window, string currentPage)
        {
            _window = window;
            _currentPage = currentPage;
            _window.Loaded += OnPageLoaded;
            _window.KeyDown += OnKeyDown;
            Theme.ThemeChanged += OnThemeChanged;
            Palette.PaletteChanged += OnPaletteChanged;
        }

        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            FindControls();

            if (_contentContainer != null)
            {
                _contentBlur = new BlurEffect { Radius = 0 };
                _contentContainer.Effect = _contentBlur;
            }

            if (_sidebarControl != null)
            {
                _sidebarControl.CurrentPage = _currentPage;
                _sidebarControl.PageChanged += OnSidebarPageChanged;
                _sidebarControl.LogoutRequested += OnSidebarLogoutRequested;
            }

            _isSidebarOpen = false;
            if (_sidebarContainer != null)
            {
                _sidebarContainer.Margin = new Thickness(-300, 0, 0, 0);
            }
            if (_menuToggle != null)
            {
                _menuToggle.Margin = new Thickness(0, 24, 0, 0);
            }
            if (_overlay != null)
            {
                _overlay.Opacity = 0;
                _overlay.Visibility = Visibility.Collapsed;
            }
            if (_contentBlur != null)
            {
                _contentBlur.Radius = 0;
            }
        }

        private void FindControls()
        {
            _contentContainer = _window.FindName("ContentContainer") as Grid;
            _sidebarContainer = _window.FindName("SidebarContainer") as Grid;
            _menuToggle = _window.FindName("MenuToggle") as Button;
            _overlay = _window.FindName("Overlay") as Border;
            _modalWindow = _window.FindName("ModalWindow") as Modal;
            _sidebarControl = _window.FindName("SidebarControl") as Sidebar;

            if (_menuToggle != null)
                _menuToggle.Click += MenuToggle_Click;
        }

        private void OnSidebarPageChanged(object sender, PageChangedEventArgs e)
        {
            CloseSidebar();
            NavigateTo(e.Page);
        }

        private void OnSidebarLogoutRequested(object sender, RoutedEventArgs e)
        {
            ShowModal(
                title: "Подтверждение",
                message: "Вы уверены, что хотите выйти?",
                confirmText: "Выйти",
                cancelText: "Отмена",
                onConfirm: () => Application.Current.Shutdown()
            );
        }

        public void NavigateTo(string page)
        {
            Window currentWindow = _window;

            switch (page)
            {
                case "home":
                    Navigation.NavigateToMain(currentWindow);
                    break;
                case "courses":
                    Navigation.NavigateToCourses(currentWindow);
                    break;
                case "settings":
                    Navigation.NavigateToSettings(currentWindow);
                    break;
                case "achievements":
                    Navigation.NavigateToAchievements(currentWindow);
                    break;
                case "constructor":
                    Navigation.NavigateToConstructor(currentWindow);
                    break;
                default:
                    break;
            }
        }

        private void MenuToggle_Click(object sender, RoutedEventArgs e)
        {
            if (_isSidebarOpen)
                CloseSidebar();
            else
                OpenSidebar();
        }

        private void OpenSidebar()
        {
            if (_isSidebarOpen || _sidebarContainer == null) return;
            _isSidebarOpen = true;

            ThicknessAnimation sidebarAnimation = new ThicknessAnimation
            {
                From = new Thickness(-300, 0, 0, 0),
                To = new Thickness(0, 0, 0, 0),
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            _sidebarContainer.BeginAnimation(Grid.MarginProperty, sidebarAnimation);

            if (_menuToggle != null)
            {
                ThicknessAnimation buttonAnimation = new ThicknessAnimation
                {
                    From = new Thickness(0, 24, 0, 0),
                    To = new Thickness(300, 24, 0, 0),
                    Duration = TimeSpan.FromMilliseconds(300),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                _menuToggle.BeginAnimation(Button.MarginProperty, buttonAnimation);
            }

            if (_overlay != null)
            {
                _overlay.Visibility = Visibility.Visible;
                DoubleAnimation fadeAnimation = new DoubleAnimation
                {
                    From = 0,
                    To = 0.5,
                    Duration = TimeSpan.FromMilliseconds(300)
                };
                _overlay.BeginAnimation(Border.OpacityProperty, fadeAnimation);
            }

            if (_contentBlur != null)
            {
                DoubleAnimation blurAnimation = new DoubleAnimation
                {
                    From = 0,
                    To = 8,
                    Duration = TimeSpan.FromMilliseconds(300),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                _contentBlur.BeginAnimation(BlurEffect.RadiusProperty, blurAnimation);
            }

            AnimateMenuIcon(true);
        }

        private void CloseSidebar()
        {
            if (!_isSidebarOpen || _sidebarContainer == null) return;
            _isSidebarOpen = false;

            ThicknessAnimation sidebarAnimation = new ThicknessAnimation
            {
                From = new Thickness(0, 0, 0, 0),
                To = new Thickness(-300, 0, 0, 0),
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };
            sidebarAnimation.Completed += (s, e) =>
            {
                _sidebarContainer.Margin = new Thickness(-300, 0, 0, 0);
            };
            _sidebarContainer.BeginAnimation(Grid.MarginProperty, sidebarAnimation);

            if (_menuToggle != null)
            {
                ThicknessAnimation buttonAnimation = new ThicknessAnimation
                {
                    From = new Thickness(300, 24, 0, 0),
                    To = new Thickness(0, 24, 0, 0),
                    Duration = TimeSpan.FromMilliseconds(300),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
                };
                buttonAnimation.Completed += (s, e) =>
                {
                    _menuToggle.Margin = new Thickness(0, 24, 0, 0);
                };
                _menuToggle.BeginAnimation(Button.MarginProperty, buttonAnimation);
            }

            if (_overlay != null)
            {
                DoubleAnimation fadeAnimation = new DoubleAnimation
                {
                    From = 0.5,
                    To = 0,
                    Duration = TimeSpan.FromMilliseconds(300)
                };
                fadeAnimation.Completed += (s, e) =>
                {
                    _overlay.Visibility = Visibility.Collapsed;
                    _overlay.Opacity = 0;
                };
                _overlay.BeginAnimation(Border.OpacityProperty, fadeAnimation);
            }

            if (_contentBlur != null)
            {
                DoubleAnimation blurAnimation = new DoubleAnimation
                {
                    From = 8,
                    To = 0,
                    Duration = TimeSpan.FromMilliseconds(300),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
                };
                blurAnimation.Completed += (s, e) =>
                {
                    _contentBlur.Radius = 0;
                };
                _contentBlur.BeginAnimation(BlurEffect.RadiusProperty, blurAnimation);
            }

            AnimateMenuIcon(false);
        }

        private void AnimateMenuIcon(bool isOpen)
        {
            if (_menuToggle == null) return;

            ControlTemplate template = _menuToggle.Template;
            if (template == null) return;

            Viewbox burgerIcon = template.FindName("BurgerIcon", _menuToggle) as Viewbox;
            Viewbox closeIcon = template.FindName("CloseIcon", _menuToggle) as Viewbox;

            if (burgerIcon == null || closeIcon == null) return;

            if (isOpen)
            {
                closeIcon.Visibility = Visibility.Visible;
                AnimateOpacity(burgerIcon, 1, 0, () => burgerIcon.Visibility = Visibility.Collapsed);
                AnimateOpacity(closeIcon, 0, 1);
            }
            else
            {
                burgerIcon.Visibility = Visibility.Visible;
                AnimateOpacity(closeIcon, 1, 0, () => closeIcon.Visibility = Visibility.Collapsed);
                AnimateOpacity(burgerIcon, 0, 1);
            }
        }

        private void AnimateOpacity(UIElement element, double from, double to, Action onComplete = null)
        {
            DoubleAnimation animation = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new CubicEase { EasingMode = from < to ? EasingMode.EaseOut : EasingMode.EaseIn }
            };

            if (onComplete != null)
                animation.Completed += (s, e) => onComplete();

            element.BeginAnimation(UIElement.OpacityProperty, animation);
        }

        public void ShowModal(string title, string message, string confirmText = "OK",
                              string cancelText = "Отмена", Action onConfirm = null, Action onCancel = null)
        {
            if (_modalWindow == null) return;
            _modalWindow.Show(title, message, confirmText, cancelText, onConfirm, onCancel);
        }

        public void ShowModalCustom(string title, object content, string confirmText = "OK",
                                    string cancelText = "Отмена", Action onConfirm = null, Action onCancel = null)
        {
            if (_modalWindow == null) return;
            _modalWindow.ShowCustom(title, content, confirmText, cancelText, onConfirm, onCancel);
        }

        public void CloseModal()
        {
            _modalWindow?.Close();
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                CloseSidebar();
        }

        private void OnThemeChanged(object sender, EventArgs e) { }

        private void OnPaletteChanged(object sender, EventArgs e) { }

        public void Cleanup()
        {
            if (_sidebarControl != null)
            {
                _sidebarControl.PageChanged -= OnSidebarPageChanged;
                _sidebarControl.LogoutRequested -= OnSidebarLogoutRequested;
            }

            if (_menuToggle != null)
                _menuToggle.Click -= MenuToggle_Click;

            Theme.ThemeChanged -= OnThemeChanged;
            Palette.PaletteChanged -= OnPaletteChanged;
        }
    }
}