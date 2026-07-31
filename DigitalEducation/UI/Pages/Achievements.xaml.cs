using DigitalEducation.Core.Managers;
using System;
using System.Windows;
using System.Windows.Controls;

namespace DigitalEducation.UI.Pages
{
    public partial class Achievements : Window
    {
        private Base _baseLogic;

        public Achievements()
        {
            InitializeComponent();
            _baseLogic = new Base(this, "achievements");
        }

        protected override void OnClosed(EventArgs e)
        {
            _baseLogic.Cleanup();
            base.OnClosed(e);
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            FilterAll.IsChecked = true;
        }

        private void SearchInput_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void Filter_Checked(object sender, RoutedEventArgs e)
        {
        }
    }
}