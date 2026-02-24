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
        Task ShowHint(string hintTemplateName, double confidence, string hintType = "rectangle");
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

        public async Task ShowHint(string hintTemplateName, double confidence, string hintType = "rectangle")
        {
            if (_visionService == null || string.IsNullOrEmpty(hintTemplateName))
                return;

            try
            {
                var result = await _visionService.FindElementAsync(hintTemplateName, confidence);

                if (result.IsDetected)
                {
                    await _dispatcher.InvokeAsync(() =>
                    {
                        _hintCanvas.Children.Clear();
                        var bounds = new Rect(result.Location.X, result.Location.Y, result.Size.Width, result.Size.Height);
                        switch (hintType)
                        {
                            case "arrow": DrawArrow(bounds); break;
                            case "highlight": DrawHighlight(bounds); break;
                            case "corner": DrawCornerMarker(bounds); break;
                            case "glow": DrawGlow(bounds); break;
                            case "dim": DrawDimBackground(bounds); break;
                            default: DrawRectangle(bounds); break;
                        }
                    });
                }
            }
            catch
            {
            }
        }

        private void DrawRectangle(Rect bounds)
        {
            var rect = new Rectangle
            {
                Width = bounds.Width,
                Height = bounds.Height,
                Stroke = new SolidColorBrush(Color.FromRgb(0x42, 0xA5, 0xF5)),
                StrokeThickness = 4,
                Fill = Brushes.Transparent,
                StrokeDashArray = new DoubleCollection { 5, 3 },
                RadiusX = 6,
                RadiusY = 6,
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Color.FromRgb(0x42, 0xA5, 0xF5),
                    BlurRadius = 15,
                    ShadowDepth = 2,
                    Opacity = 0.5
                }
            };
            Canvas.SetLeft(rect, bounds.X);
            Canvas.SetTop(rect, bounds.Y);
            _hintCanvas.Children.Add(rect);
        }

        private void DrawArrow(Rect bounds)
        {
            var arrow = new Polygon
            {
                Points = new PointCollection
                {
                    new Point(0, 0),
                    new Point(25, 12),
                    new Point(0, 24)
                },
                Fill = new SolidColorBrush(Color.FromRgb(0x66, 0xCC, 0x66)),
                Stroke = Brushes.White,
                StrokeThickness = 1.5,
                Opacity = 0.9,
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Colors.Black,
                    BlurRadius = 8,
                    ShadowDepth = 2,
                    Opacity = 0.3
                }
            };
            Canvas.SetLeft(arrow, bounds.Left - 30);
            Canvas.SetTop(arrow, bounds.Top + (bounds.Height - 24) / 2);
            _hintCanvas.Children.Add(arrow);
        }

        private void DrawHighlight(Rect bounds)
        {
            var highlight = new Rectangle
            {
                Width = bounds.Width,
                Height = bounds.Height,
                Fill = new RadialGradientBrush
                {
                    GradientStops = new GradientStopCollection
                    {
                        new GradientStop(Color.FromArgb(0x88, 0xFF, 0xFF, 0x00), 0.0),
                        new GradientStop(Color.FromArgb(0x22, 0xFF, 0xFF, 0x00), 1.0)
                    },
                    Center = new Point(0.5, 0.5),
                    RadiusX = 0.7,
                    RadiusY = 0.7
                },
                Opacity = 0.7
            };
            Canvas.SetLeft(highlight, bounds.X);
            Canvas.SetTop(highlight, bounds.Y);
            _hintCanvas.Children.Add(highlight);
        }

        private void DrawCornerMarker(Rect bounds)
        {
            var marker = new Rectangle
            {
                Width = 16,
                Height = 16,
                Fill = new SolidColorBrush(Color.FromRgb(0xFF, 0xA5, 0x00)),
                RadiusX = 4,
                RadiusY = 4,
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Colors.Orange,
                    BlurRadius = 12,
                    ShadowDepth = 2,
                    Opacity = 0.6
                }
            };
            Canvas.SetLeft(marker, bounds.Left - 8);
            Canvas.SetTop(marker, bounds.Top - 8);
            _hintCanvas.Children.Add(marker);
        }

        private void DrawGlow(Rect bounds)
        {
            var outerGlow = new Rectangle
            {
                Width = bounds.Width + 16,
                Height = bounds.Height + 16,
                Fill = new SolidColorBrush(Color.FromArgb(0x44, 0x1E, 0x90, 0xFF)),
                RadiusX = 10,
                RadiusY = 10,
                Effect = new System.Windows.Media.Effects.BlurEffect { Radius = 12 }
            };
            Canvas.SetLeft(outerGlow, bounds.X - 8);
            Canvas.SetTop(outerGlow, bounds.Y - 8);
            _hintCanvas.Children.Add(outerGlow);

            var innerGlow = new Rectangle
            {
                Width = bounds.Width + 6,
                Height = bounds.Height + 6,
                Fill = new SolidColorBrush(Color.FromArgb(0xAA, 0x1E, 0x90, 0xFF)),
                RadiusX = 8,
                RadiusY = 8,
                Effect = new System.Windows.Media.Effects.BlurEffect { Radius = 4 }
            };
            Canvas.SetLeft(innerGlow, bounds.X - 3);
            Canvas.SetTop(innerGlow, bounds.Y - 3);
            _hintCanvas.Children.Add(innerGlow);
        }

        private void DrawDimBackground(Rect bounds)
        {
            var fullScreen = new Rectangle
            {
                Width = SystemParameters.VirtualScreenWidth,
                Height = SystemParameters.VirtualScreenHeight,
                Fill = new SolidColorBrush(Color.FromArgb(0xAA, 0x00, 0x00, 0x00))
            };
            Canvas.SetLeft(fullScreen, SystemParameters.VirtualScreenLeft);
            Canvas.SetTop(fullScreen, SystemParameters.VirtualScreenTop);
            _hintCanvas.Children.Add(fullScreen);

            var clipGeometry = new RectangleGeometry(new Rect(0, 0, SystemParameters.VirtualScreenWidth, SystemParameters.VirtualScreenHeight));
            var holeGeometry = new RectangleGeometry(bounds);
            var combinedGeometry = new CombinedGeometry(GeometryCombineMode.Exclude, clipGeometry, holeGeometry);
            fullScreen.Clip = combinedGeometry;
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