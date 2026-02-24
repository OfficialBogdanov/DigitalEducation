using System;
using System.Windows;

namespace DigitalEducation
{
    public class DialogOptions
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string ConfirmText { get; set; } = "OK";
        public string CancelText { get; set; }
        public bool ShowCancel => !string.IsNullOrEmpty(CancelText);
    }

    public interface IDialogContentFactory
    {
        UIElement CreateContent(DialogOptions options, Action<bool?> resultCallback);
    }

    public class DefaultDialogContentFactory : IDialogContentFactory
    {
        public UIElement CreateContent(DialogOptions options, Action<bool?> resultCallback)
        {
            var dialog = new ConfirmDialog
            {
                Title = options.Title,
                Message = options.Message,
                ConfirmButtonText = options.ConfirmText,
                CancelButtonText = options.CancelText,
                ShowCancelButton = options.ShowCancel
            };
            dialog.SetResultCallback(resultCallback);
            return dialog;
        }
    }
}