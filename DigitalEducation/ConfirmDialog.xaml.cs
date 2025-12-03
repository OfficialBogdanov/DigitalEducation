using System;
using System.Windows;
using System.Windows.Controls;

namespace DigitalEducation
{
    public partial class ConfirmDialog : UserControl
    {
        public event EventHandler<bool> DialogResultChanged;

        public ConfirmDialog()
        {
            InitializeComponent();
        }

        public string Title
        {
            get => TitleText.Text;
            set => TitleText.Text = value;
        }

        public string Message
        {
            get => MessageText.Text;
            set => MessageText.Text = value;
        }

        public string ConfirmButtonText
        {
            get => ConfirmButton.Content.ToString();
            set => ConfirmButton.Content = value;
        }

        public string CancelButtonText
        {
            get => CancelButton.Content.ToString();
            set => CancelButton.Content = value;
        }

        private void OnConfirmClick(object sender, RoutedEventArgs e)
        {
            DialogResultChanged?.Invoke(this, true);
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            DialogResultChanged?.Invoke(this, false);
        }
    }
}