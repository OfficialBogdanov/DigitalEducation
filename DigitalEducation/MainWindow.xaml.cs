using DigitalEducation.Core.Managers;
using DigitalEducation.UI.Components;
using System;
using System.Threading.Tasks;
using System.Windows;
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
            _ = SimulateLoading();
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

        private async Task SimulateLoading()
        {
            string[] messages = new string[]
            {
                "Загрузка ресурсов",
                "Инициализация модулей",
                "Подготовка данных",
                "Загрузка приложения"
            };

            double progress = 0;
            double step = 100.0 / (messages.Length * 2);

            for (int i = 0; i < messages.Length; i++)
            {
                await Dispatcher.InvokeAsync(() =>
                {
                    LoadingMessage.Text = messages[i];
                });

                double targetProgress = Math.Min(progress + step * 2, 100);

                while (progress < targetProgress)
                {
                    progress = Math.Min(progress + 2, targetProgress);

                    await Dispatcher.InvokeAsync(() =>
                    {
                        ProgressFill.Value = progress;
                    });

                    await Task.Delay(20);
                }

                if (i < messages.Length - 1)
                {
                    await Task.Delay(300);
                }
            }

            await Task.Delay(500);

            await Dispatcher.InvokeAsync(() =>
            {
                StopAllAnimations();
                UI.Pages.Main mainWindow = new UI.Pages.Main();
                mainWindow.Show();
                Close();
            });
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            StopAllAnimations();
            Close();
        }

        private void StopAllAnimations()
        {
            if (_logoAnimationTimer != null)
            {
                _logoAnimationTimer.Stop();
            }
            if (_dot1Timer != null)
            {
                _dot1Timer.Stop();
            }
            if (_dot2Timer != null)
            {
                _dot2Timer.Stop();
            }
            if (_dot3Timer != null)
            {
                _dot3Timer.Stop();
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            StopAllAnimations();
            base.OnClosing(e);
        }
    }
}