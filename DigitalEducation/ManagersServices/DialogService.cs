using System;
using System.Windows;
using System.Windows.Media;

namespace DigitalEducation
{
    public static class DialogService
    {
        public static bool? ShowConfirmDialog(string title, string message,
                                              string confirmText = "Подтвердить",
                                              string cancelText = "Отмена",
                                              Window owner = null)
        {
            var confirmDialog = new ConfirmDialog
            {
                Title = title,
                Message = message,
                ConfirmButtonText = confirmText,
                CancelButtonText = cancelText,
                ShowCancelButton = !string.IsNullOrEmpty(cancelText)
            };

            var dialogWindow = new Window
            {
                Title = "",
                Content = confirmDialog,
                WindowStyle = WindowStyle.None,
                ResizeMode = ResizeMode.NoResize,
                SizeToContent = SizeToContent.WidthAndHeight,
                Topmost = true,
                ShowInTaskbar = false,
                AllowsTransparency = true,
                Background = Brushes.Transparent,
                Owner = owner ?? Application.Current.MainWindow,
                WindowStartupLocation = owner == null ?
                    WindowStartupLocation.CenterScreen :
                    WindowStartupLocation.CenterOwner
            };

            bool? result = null;

            confirmDialog.DialogResultChanged += (s, dialogResult) =>
            {
                result = dialogResult;
                dialogWindow.DialogResult = dialogResult;
                dialogWindow.Close();
            };

            dialogWindow.ShowDialog();
            return result;
        }

        public static void ShowMessageDialog(string title, string message,
                                           string buttonText = "OK",
                                           Window owner = null)
        {
            var confirmDialog = new ConfirmDialog
            {
                Title = title,
                Message = message,
                ConfirmButtonText = buttonText,
                CancelButtonText = null,
                ShowCancelButton = false
            };

            var dialogWindow = new Window
            {
                Title = "",
                Content = confirmDialog,
                WindowStyle = WindowStyle.None,
                ResizeMode = ResizeMode.NoResize,
                SizeToContent = SizeToContent.WidthAndHeight,
                Topmost = true,
                ShowInTaskbar = false,
                AllowsTransparency = true,
                Background = Brushes.Transparent,
                Owner = owner ?? Application.Current.MainWindow,
                WindowStartupLocation = owner == null ?
                    WindowStartupLocation.CenterScreen :
                    WindowStartupLocation.CenterOwner
            };

            bool? result = null;

            confirmDialog.DialogResultChanged += (s, dialogResult) =>
            {
                result = dialogResult;
                dialogWindow.Close();
            };

            dialogWindow.ShowDialog();
        }

        public static void ShowErrorDialog(string message, Window owner = null)
        {
            ShowMessageDialog("Ошибка", message, "OK", owner);
        }

        public static void ShowSuccessDialog(string message, Window owner = null)
        {
            ShowMessageDialog("Успех", message, "OK", owner);
        }
    }
}