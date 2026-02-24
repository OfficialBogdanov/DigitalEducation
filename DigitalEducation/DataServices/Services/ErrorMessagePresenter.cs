using System.Windows;

namespace DigitalEducation
{
    public interface IErrorPresenter
    {
        void ShowError(string message);
    }

    public class ErrorMessagePresenter : IErrorPresenter
    {
        private readonly Window _ownerWindow;

        public ErrorMessagePresenter(Window ownerWindow)
        {
            _ownerWindow = ownerWindow;
        }

        public void ShowError(string message)
        {
            AppDialogService.ShowErrorDialog(message, _ownerWindow);
        }
    }
}