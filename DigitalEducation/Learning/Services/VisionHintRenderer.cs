using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using DigitalEducation.ComputerVision.Services;

namespace DigitalEducation
{
    public interface IHintRenderer { 
        Task ShowHint(string hintTemplateName, double confidence, string hintType = "rectangle"); 
        Task ClearHint(); 
    }

    public class VisionHintRenderer : IHintRenderer
    {
        private readonly ComputerVisionService _visionService;
        private readonly Canvas _hintCanvas;
        private readonly Rectangle _overlayRect;
        private readonly Dispatcher _dispatcher;
        private readonly Brush _defaultOverlayBrush;

        public VisionHintRenderer(ComputerVisionService visionService, Canvas hintCanvas, Rectangle overlayRect, Dispatcher dispatcher)
        {
            _visionService = visionService;
            _hintCanvas = hintCanvas;
            _overlayRect = overlayRect;
            _dispatcher = dispatcher;
            _defaultOverlayBrush = Application.Current?.FindResource("OverlayBrush") as Brush
                                   ?? new SolidColorBrush(Color.FromArgb(0x80, 0, 0, 0));
        }

        public async Task ShowHint(string hintTemplateName, double confidence, string hintType = "rectangle")
        {
            try
            {
                if (hintType == "dim")
                {
                    VisionRecognitionResult result = null;
                    if (!string.IsNullOrEmpty(hintTemplateName))
                    {
                        result = await _visionService.FindElementAsync(hintTemplateName, confidence)
                                                      .ConfigureAwait(false); 
                    }

                    await _dispatcher.InvokeAsync(() =>
                    {
                        _hintCanvas.Children.Clear();
                        _overlayRect.Fill = Brushes.Transparent; 

                        if (result != null && result.IsDetected)
                        {
                            var hole = new Rect(result.Location.X, result.Location.Y, result.Size.Width, result.Size.Height);
                            DrawDimWithHole(hole);
                        }
                        else
                        {
                            DrawDimFull();
                        }
                    });
                }
                else
                {
                    await _dispatcher.InvokeAsync(() =>
                    {
                        _overlayRect.Fill = _defaultOverlayBrush;
                        _hintCanvas.Children.Clear();
                    });

                    var result = await _visionService.FindElementAsync(hintTemplateName, confidence)
                                                      .ConfigureAwait(false);

                    if (result.IsDetected)
                    {
                        await _dispatcher.InvokeAsync(() =>
                        {
                            var bounds = new Rect(result.Location.X, result.Location.Y, result.Size.Width, result.Size.Height);
                            switch (hintType)
                            {
                                case "arrow": DrawArrow(bounds); break;
                                case "highlight": DrawHighlight(bounds); break;
                                case "corner": DrawCornerMarker(bounds); break;
                                case "glow": DrawGlow(bounds); break;
                                default: DrawRectangle(bounds); break;
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HintRenderer] Ошибка: {ex}");
            }
        }

        public async Task ClearHint()
        {
            await _dispatcher.InvokeAsync(() =>
            {
                _hintCanvas.Children.Clear();
                _overlayRect.Fill = _defaultOverlayBrush;
            });
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

        private void DrawDimWithHole(Rect hole)
        {
            double screenWidth = SystemParameters.VirtualScreenWidth;
            double screenHeight = SystemParameters.VirtualScreenHeight;
            Brush dimBrush = Application.Current?.FindResource("DimOverlayBrush") as Brush
                             ?? new SolidColorBrush(Color.FromArgb(0xCC, 0, 0, 0));

            AddDimRect(0, 0, screenWidth, hole.Top, dimBrush);
            AddDimRect(0, hole.Bottom, screenWidth, screenHeight - hole.Bottom, dimBrush);
            AddDimRect(0, hole.Top, hole.Left, hole.Height, dimBrush);
            AddDimRect(hole.Right, hole.Top, screenWidth - hole.Right, hole.Height, dimBrush);
        }

        private void DrawDimFull()
        {
            double screenWidth = SystemParameters.VirtualScreenWidth;
            double screenHeight = SystemParameters.VirtualScreenHeight;
            Brush dimBrush = Application.Current?.FindResource("DimOverlayBrush") as Brush
                             ?? new SolidColorBrush(Color.FromArgb(0xCC, 0, 0, 0));
            AddDimRect(0, 0, screenWidth, screenHeight, dimBrush);
        }

        private void AddDimRect(double x, double y, double width, double height, Brush brush)
        {
            if (width <= 0 || height <= 0) return;
            var rect = new Rectangle
            {
                Width = width,
                Height = height,
                Fill = brush,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(rect, x);
            Canvas.SetTop(rect, y);
            _hintCanvas.Children.Add(rect);
        }
    }
}