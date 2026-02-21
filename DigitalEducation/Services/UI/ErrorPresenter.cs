using System.Windows;

namespace DigitalEducation
{
    public interface IErrorPresenter
    {
        void ShowError(string message);
    }

    public class ErrorPresenter : IErrorPresenter
    {
        private readonly Window _ownerWindow;

        public ErrorPresenter(Window ownerWindow)
        {
            _ownerWindow = ownerWindow;
        }

        public void ShowError(string message)
        {
            DialogService.ShowErrorDialog(message, _ownerWindow);
        }
    }
}