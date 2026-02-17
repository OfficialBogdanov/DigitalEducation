using System;
using System.Windows;
using System.Windows.Media;

namespace DigitalEducation
{
    public interface IDialogService
    {
        bool? ShowConfirmDialog(string title, string message, string confirmText = "Подтвердить", string cancelText = "Отмена", Window owner = null);
        void ShowMessageDialog(string title, string message, string buttonText = "OK", Window owner = null);
        void ShowErrorDialog(string message, Window owner = null);
        void ShowSuccessDialog(string message, Window owner = null);
    }

    public class DialogServiceImpl : IDialogService
    {
        private readonly IDialogContentFactory _contentFactory;

        public DialogServiceImpl() : this(new DefaultDialogContentFactory()) { }

        public DialogServiceImpl(IDialogContentFactory contentFactory)
        {
            _contentFactory = contentFactory;
        }

        private Window CreateWindow(UIElement content, Window owner)
        {
            Window ownerWindow = owner ?? Application.Current?.MainWindow;
            return new Window
            {
                Content = content,
                WindowStyle = WindowStyle.None,
                ResizeMode = ResizeMode.NoResize,
                SizeToContent = SizeToContent.WidthAndHeight,
                Topmost = true,
                ShowInTaskbar = false,
                AllowsTransparency = true,
                Background = Brushes.Transparent,
                Owner = ownerWindow,
                WindowStartupLocation = ownerWindow == null ? WindowStartupLocation.CenterScreen : WindowStartupLocation.CenterOwner
            };
        }

        private bool? ShowDialog(DialogOptions options, Window owner)
        {
            var window = CreateWindow(null, owner);
            var content = _contentFactory.CreateContent(options, r => window.DialogResult = r);
            window.Content = content;
            return window.ShowDialog();
        }

        public bool? ShowConfirmDialog(string title, string message, string confirmText = "Подтвердить", string cancelText = "Отмена", Window owner = null)
        {
            var options = new DialogOptions
            {
                Title = title,
                Message = message,
                ConfirmText = confirmText,
                CancelText = cancelText
            };
            return ShowDialog(options, owner);
        }

        public void ShowMessageDialog(string title, string message, string buttonText = "OK", Window owner = null)
        {
            var options = new DialogOptions
            {
                Title = title,
                Message = message,
                ConfirmText = buttonText,
                CancelText = null
            };
            ShowDialog(options, owner);
        }

        public void ShowErrorDialog(string message, Window owner = null)
        {
            ShowMessageDialog("Ошибка", message, "OK", owner);
        }

        public void ShowSuccessDialog(string message, Window owner = null)
        {
            ShowMessageDialog("Успех", message, "OK", owner);
        }
    }

    public static class DialogService
    {
        private static IDialogService _current;

        public static IDialogService Current
        {
            get
            {
                if (_current == null)
                    _current = new DialogServiceImpl();
                return _current;
            }
            set => _current = value ?? throw new ArgumentNullException(nameof(value));
        }

        public static bool? ShowConfirmDialog(string title, string message, string confirmText = "Подтвердить", string cancelText = "Отмена", Window owner = null)
        {
            return Current.ShowConfirmDialog(title, message, confirmText, cancelText, owner);
        }

        public static void ShowMessageDialog(string title, string message, string buttonText = "OK", Window owner = null)
        {
            Current.ShowMessageDialog(title, message, buttonText, owner);
        }

        public static void ShowErrorDialog(string message, Window owner = null)
        {
            Current.ShowErrorDialog(message, owner);
        }

        public static void ShowSuccessDialog(string message, Window owner = null)
        {
            Current.ShowSuccessDialog(message, owner);
        }
    }
}