using DigitalEducation.ComputerVision.Services;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace DigitalEducation
{
    public interface IHintRenderer
    {
        Task ShowHint(string hintTemplateName, double confidence);
        Task ClearHint();
    }

    public class HintRenderer : IHintRenderer
    {
        private readonly VisionService _visionService;
        private readonly Canvas _hintCanvas;
        private readonly Dispatcher _dispatcher;

        public HintRenderer(VisionService visionService, Canvas hintCanvas, Dispatcher dispatcher)
        {
            _visionService = visionService;
            _hintCanvas = hintCanvas;
            _dispatcher = dispatcher;
        }

        public async Task ShowHint(string hintTemplateName, double confidence)
        {
            if (_visionService == null || string.IsNullOrEmpty(hintTemplateName))
                return;

            try
            {
                var hintResult = await _visionService.FindElementAsync(hintTemplateName, confidence);

                if (hintResult.IsDetected)
                {
                    await _dispatcher.InvokeAsync(() =>
                    {
                        var screenBounds = new Rect(
                            hintResult.Location.X,
                            hintResult.Location.Y,
                            hintResult.Size.Width,
                            hintResult.Size.Height
                        );

                        var hintRect = new Rectangle
                        {
                            Width = screenBounds.Width,
                            Height = screenBounds.Height,
                            Stroke = Brushes.DodgerBlue,
                            StrokeThickness = 3,
                            Fill = Brushes.Transparent,
                            StrokeDashArray = new DoubleCollection(new double[] { 4, 4 })
                        };

                        _hintCanvas.Children.Clear();
                        Canvas.SetLeft(hintRect, screenBounds.X);
                        Canvas.SetTop(hintRect, screenBounds.Y);
                        _hintCanvas.Children.Add(hintRect);
                    });
                }
            }
            catch
            {
            }
        }

        public async Task ClearHint()
        {
            await _dispatcher.InvokeAsync(() =>
            {
                _hintCanvas.Children.Clear();
            });
        }
    }
}