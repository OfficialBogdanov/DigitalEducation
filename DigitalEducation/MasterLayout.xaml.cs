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

        public MasterLayout()
        {
            InitializeComponent();
            Loaded += OnMasterLayoutLoaded;
            Unloaded += OnMasterLayoutUnloaded;
        }

        private void OnMasterLayoutLoaded(object sender, RoutedEventArgs e)
        {
            ThemeManager.ThemeChanged += OnThemeChanged;
            ThemeManager.UpdateAllIconsInContainer(this);
            UpdateButtonStyles();
            ForceVisualRefresh();
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

            var activeStyle = (Style)FindResource("ActiveNavigationButtonStyle");

            switch (pageName)
            {
                case "Home":
                    if (btnHome != null) btnHome.Style = activeStyle;
                    break;
                case "Courses":
                    if (btnCourses != null) btnCourses.Style = activeStyle;
                    break;
                case "Settings":
                    if (btnSettings != null) btnSettings.Style = activeStyle;
                    break;
                case "CloseApp":
                    break;
            }
        }

        public void HandleNavigationClick(Button clickedButton)
        {
            if (clickedButton == null) return;

            if (clickedButton.Name == "btnCloseApp")
                return;

            ResetAllNavigationButtons();
            clickedButton.Style = (Style)FindResource("ActiveNavigationButtonStyle");
        }

        private void ResetAllNavigationButtons()
        {
            var defaultStyle = (Style)FindResource("NavigationButtonStyle");

            if (btnHome != null) btnHome.Style = defaultStyle;
            if (btnCourses != null) btnCourses.Style = defaultStyle;
            if (btnSettings != null) btnSettings.Style = defaultStyle;
        }

        private void OnThemeChanged(object sender, string themeName)
        {
            ThemeManager.UpdateAllIconsInContainer(this);
            UpdateButtonStyles();
            ForceVisualRefresh();
        }

        private void UpdateButtonStyles()
        {
            Dispatcher.InvokeAsync(() =>
            {
                var defaultStyle = (Style)FindResource("NavigationButtonStyle");

                if (btnHome != null)
                {
                    var currentStyle = btnHome.Style;
                    btnHome.Style = null;
                    btnHome.Style = currentStyle ?? defaultStyle;
                }

                if (btnCourses != null)
                {
                    var currentStyle = btnCourses.Style;
                    btnCourses.Style = null;
                    btnCourses.Style = currentStyle ?? defaultStyle;
                }

                if (btnSettings != null)
                {
                    var currentStyle = btnSettings.Style;
                    btnSettings.Style = null;
                    btnSettings.Style = currentStyle ?? defaultStyle;
                }

                if (btnCloseApp != null)
                {
                    var currentStyle = btnCloseApp.Style;
                    btnCloseApp.Style = null;
                    btnCloseApp.Style = currentStyle ?? defaultStyle;
                }
            }, System.Windows.Threading.DispatcherPriority.Render);
        }

        private void ForceVisualRefresh()
        {
            Dispatcher.InvokeAsync(() =>
            {
                InvalidateVisual();
                InvalidateMeasure();
                InvalidateArrange();

                UpdateLayout();

                var app = Application.Current;
                if (app != null)
                {
                    Resources = null;
                    Resources = app.Resources;
                }
            }, System.Windows.Threading.DispatcherPriority.Render);
        }
    }
}