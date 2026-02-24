using System;
using System.Windows;
using System.Windows.Controls;

namespace DigitalEducation
{
    public partial class ConfirmDialog : UserControl
    {
        private Action<bool?> _resultCallback;

        public ConfirmDialog()
        {
            InitializeComponent();
        }

        public void SetResultCallback(Action<bool?> callback)
        {
            _resultCallback = callback;
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

        public bool ShowCancelButton
        {
            get => CancelButton.Visibility == Visibility.Visible;
            set => CancelButton.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
        }

        private void OnConfirmClick(object sender, RoutedEventArgs e)
        {
            _resultCallback?.Invoke(true);
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            _resultCallback?.Invoke(false);
        }
    }
}