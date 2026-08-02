using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace DigitalEducation
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer _logoAnimationTimer;
        private bool _goingUp = true;
        private double _currentOffset = 0;
        private double _step = 0.08;
        private DispatcherTimer _dot1Timer;
        private DispatcherTimer _dot2Timer;
        private DispatcherTimer _dot3Timer;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            KeyDown += OnKeyDown;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            StartLogoAnimation();
            StartDotAnimations();
            ProgressFill.Value = 0;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void StartLogoAnimation()
        {
            _logoAnimationTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16)
            };
            _logoAnimationTimer.Tick += LogoAnimationTick;
            _logoAnimationTimer.Start();
        }

        private void LogoAnimationTick(object sender, EventArgs e)
        {
            if (_goingUp)
            {
                _currentOffset -= _step;
                if (_currentOffset <= -6)
                {
                    _goingUp = false;
                }
            }
            else
            {
                _currentOffset += _step;
                if (_currentOffset >= 0)
                {
                    _goingUp = true;
                }
            }

            LogoTranslate.Y = _currentOffset;

            double glowScale = 0.95 + (Math.Abs(_currentOffset) / 6) * 0.05;
            GlowScale.ScaleX = glowScale;
            GlowScale.ScaleY = glowScale;
        }

        private void StartDotAnimations()
        {
            _dot1Timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16)
            };
            AnimateDot(Dot1, _dot1Timer, 0);

            _dot2Timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16)
            };
            AnimateDot(Dot2, _dot2Timer, 200);

            _dot3Timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16)
            };
            AnimateDot(Dot3, _dot3Timer, 400);
        }

        private void AnimateDot(Ellipse dot, DispatcherTimer timer, int delay)
        {
            bool growing = true;
            double scale = 0.8;
            double dotStep = 0.015;

            DispatcherTimer startTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(delay)
            };
            startTimer.Tick += (s, e) =>
            {
                startTimer.Stop();
                timer.Start();
            };
            startTimer.Start();

            timer.Tick += (s, e) =>
            {
                if (growing)
                {
                    scale += dotStep;
                    if (scale >= 1.2)
                    {
                        growing = false;
                    }
                }
                else
                {
                    scale -= dotStep;
                    if (scale <= 0.8)
                    {
                        growing = true;
                    }
                }

                ScaleTransform scaleTransform = dot.RenderTransform as ScaleTransform;
                if (scaleTransform != null)
                {
                    scaleTransform.ScaleX = scale;
                    scaleTransform.ScaleY = scale;
                }

                dot.Opacity = 0.2 + (scale - 0.8) / 0.4 * 0.8;
            };
        }

        public void UpdateLoadingMessage(string message)
        {
            Dispatcher.Invoke(() =>
            {
                if (LoadingMessage != null)
                {
                    LoadingMessage.Text = message;
                }
            });
        }

        public void UpdateLoadingProgress(int percent)
        {
            Dispatcher.Invoke(() =>
            {
                if (ProgressFill != null)
                {
                    DoubleAnimation animation = new DoubleAnimation
                    {
                        From = ProgressFill.Value,
                        To = percent,
                        Duration = TimeSpan.FromMilliseconds(250),
                        EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                    };

                    ProgressFill.BeginAnimation(ProgressBar.ValueProperty, animation);
                }
            });
        }

        private void StopAllAnimations()
        {
            _logoAnimationTimer?.Stop();
            _dot1Timer?.Stop();
            _dot2Timer?.Stop();
            _dot3Timer?.Stop();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            StopAllAnimations();
            base.OnClosing(e);
        }
    }
}