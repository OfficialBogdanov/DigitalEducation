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
                case "Progress":
                    if (btnProgress != null) btnProgress.Style = activeStyle;
                    break;
                case "Settings":
                    if (btnSettings != null) btnSettings.Style = activeStyle;
                    break;
            }
        }

        public void HandleNavigationClick(Button clickedButton)
        {
            if (clickedButton == null) return;

            ResetAllNavigationButtons();
            clickedButton.Style = (Style)FindResource("ActiveNavigationButtonStyle");
        }

        private void ResetAllNavigationButtons()
        {
            var defaultStyle = (Style)FindResource("NavigationButtonStyle");

            if (btnHome != null) btnHome.Style = defaultStyle;
            if (btnCourses != null) btnCourses.Style = defaultStyle;
            if (btnProgress != null) btnProgress.Style = defaultStyle;
            if (btnSettings != null) btnSettings.Style = defaultStyle;
        }
    }
}