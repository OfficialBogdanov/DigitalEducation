using System.Windows;
using System.Windows.Controls;

namespace DigitalEducation
{
    public partial class MasterLayout : UserControl
    {
        public static new readonly DependencyProperty ContentProperty =
            DependencyProperty.Register("Content", typeof(object), typeof(MasterLayout),
                new PropertyMetadata(null, OnContentChanged));

        public new object Content
        {
            get { return GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        private Style _defaultNavigationStyle;
        private Style _activeNavigationStyle;

        public MasterLayout()
        {
            InitializeComponent();

            _defaultNavigationStyle = (Style)FindResource("NavigationButtonStyle");
            _activeNavigationStyle = (Style)FindResource("ActiveNavigationButtonStyle");

            Loaded += OnMasterLayoutLoaded;
            Unloaded += OnMasterLayoutUnloaded;
        }

        private void OnMasterLayoutLoaded(object sender, RoutedEventArgs e)
        {
            ThemeManager.UpdateAllIconsInContainer(this);
            ThemeManager.ThemeChanged += OnThemeChanged;
            Loaded -= OnMasterLayoutLoaded;
        }

        private void OnMasterLayoutUnloaded(object sender, RoutedEventArgs e)
        {
            ThemeManager.ThemeChanged -= OnThemeChanged;
            Unloaded -= OnMasterLayoutUnloaded;
        }

        private static void OnContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var layout = (MasterLayout)d;
            layout.ContentArea.Content = e.NewValue;
        }

        public void SetActiveNavigation(string pageName)
        {
            ResetAllNavigationButtons();

            switch (pageName)
            {
                case "Home":
                    if (btnHome != null) btnHome.Style = _activeNavigationStyle;
                    break;
                case "Courses":
                    if (btnCourses != null) btnCourses.Style = _activeNavigationStyle;
                    break;
                case "Settings":
                    if (btnSettings != null) btnSettings.Style = _activeNavigationStyle;
                    break;
            }
        }

        private void ResetAllNavigationButtons()
        {
            if (btnHome != null) btnHome.Style = _defaultNavigationStyle;
            if (btnCourses != null) btnCourses.Style = _defaultNavigationStyle;
            if (btnSettings != null) btnSettings.Style = _defaultNavigationStyle;
        }

        private void OnThemeChanged(object sender, string themeName)
        {
            ThemeManager.UpdateAllIconsInContainer(this);
        }
    }
}