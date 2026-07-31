using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media;

namespace DigitalEducation.UI.Components
{
    public partial class Modal : UserControl
    {
        private Action _onConfirm;
        private Action _onCancel;

        public Modal()
        {
            InitializeComponent();
        }

        public void Show(string title, string message, string confirmText = "OK",
                         string cancelText = "Отмена", Action onConfirm = null, Action onCancel = null)
        {
            _onConfirm = onConfirm;
            _onCancel = onCancel;

            TitleText.Text = title ?? "Подтверждение";
            BodyText.Text = message ?? "Вы уверены?";
            ConfirmButton.Content = confirmText ?? "OK";
            CancelButton.Content = cancelText ?? "Отмена";

            ShowModal();
        }

        public void ShowCustom(string title, object content, string confirmText = "OK",
                               string cancelText = "Отмена", Action onConfirm = null, Action onCancel = null)
        {
            _onConfirm = onConfirm;
            _onCancel = onCancel;

            TitleText.Text = title ?? "Подтверждение";

            if (content is string stringContent)
            {
                BodyText.Text = stringContent;
            }
            else
            {
                BodyText.Text = content?.ToString() ?? "Вы уверены?";
            }

            ConfirmButton.Content = confirmText ?? "OK";
            CancelButton.Content = cancelText ?? "Отмена";

            ShowModal();
        }

        private void ShowModal()
        {
            Overlay.Visibility = Visibility.Visible;
            ModalContainer.Visibility = Visibility.Visible;

            TransformGroup transformGroup = ModalBox.RenderTransform as TransformGroup;
            if (transformGroup != null)
            {
                ScaleTransform scaleTransform = null;
                TranslateTransform translateTransform = null;

                foreach (Transform transform in transformGroup.Children)
                {
                    if (transform is ScaleTransform st)
                        scaleTransform = st;
                    else if (transform is TranslateTransform tt)
                        translateTransform = tt;
                }

                if (scaleTransform != null)
                {
                    DoubleAnimation scaleAnimation = new DoubleAnimation
                    {
                        From = 0.95,
                        To = 1,
                        Duration = TimeSpan.FromMilliseconds(300),
                        EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                    };
                    scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
                    scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
                }

                if (translateTransform != null)
                {
                    DoubleAnimation translateAnimation = new DoubleAnimation
                    {
                        From = 20,
                        To = 0,
                        Duration = TimeSpan.FromMilliseconds(300),
                        EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                    };
                    translateTransform.BeginAnimation(TranslateTransform.YProperty, translateAnimation);
                }
            }

            DoubleAnimation overlayAnimation = new DoubleAnimation
            {
                From = 0,
                To = 0.6,
                Duration = TimeSpan.FromMilliseconds(300)
            };
            Overlay.BeginAnimation(Border.OpacityProperty, overlayAnimation);

            DoubleAnimation fadeAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(300)
            };
            ModalContainer.BeginAnimation(Grid.OpacityProperty, fadeAnimation);
        }

        private void CloseModal()
        {
            TransformGroup transformGroup = ModalBox.RenderTransform as TransformGroup;
            if (transformGroup != null)
            {
                ScaleTransform scaleTransform = null;
                TranslateTransform translateTransform = null;

                foreach (Transform transform in transformGroup.Children)
                {
                    if (transform is ScaleTransform st)
                        scaleTransform = st;
                    else if (transform is TranslateTransform tt)
                        translateTransform = tt;
                }

                if (scaleTransform != null)
                {
                    DoubleAnimation scaleAnimation = new DoubleAnimation
                    {
                        From = 1,
                        To = 0.95,
                        Duration = TimeSpan.FromMilliseconds(300),
                        EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
                    };
                    scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
                    scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
                }

                if (translateTransform != null)
                {
                    DoubleAnimation translateAnimation = new DoubleAnimation
                    {
                        From = 0,
                        To = 20,
                        Duration = TimeSpan.FromMilliseconds(300),
                        EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
                    };
                    translateTransform.BeginAnimation(TranslateTransform.YProperty, translateAnimation);
                }
            }

            DoubleAnimation overlayAnimation = new DoubleAnimation
            {
                From = 0.6,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300)
            };
            overlayAnimation.Completed += (s, e) =>
            {
                Overlay.Visibility = Visibility.Collapsed;
            };
            Overlay.BeginAnimation(Border.OpacityProperty, overlayAnimation);

            DoubleAnimation fadeAnimation = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300)
            };
            fadeAnimation.Completed += (s, e) =>
            {
                ModalContainer.Visibility = Visibility.Collapsed;
            };
            ModalContainer.BeginAnimation(Grid.OpacityProperty, fadeAnimation);
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            _onConfirm?.Invoke();
            CloseModal();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _onCancel?.Invoke();
            CloseModal();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            _onCancel?.Invoke();
            CloseModal();
        }

        public void Close()
        {
            CloseModal();
        }
    }
}