using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows;

namespace DigitalEducation.ComputerVision.Services
{
    public class ScreenCapturer
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleDC(IntPtr hDC);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth, int nHeight);

        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

        [DllImport("gdi32.dll")]
        private static extern int BitBlt(IntPtr hDestDC, int x, int y, int nWidth, int nHeight,
                                         IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);

        [DllImport("gdi32.dll")]
        private static extern int DeleteDC(IntPtr hDC);

        [DllImport("gdi32.dll")]
        private static extern int DeleteObject(IntPtr hObject);

        private const int SRCCOPY = 0x00CC0020;

        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;
            }
        }

        ~ScreenCapturer()
        {
            Dispose(false);
        }

        public Bitmap CaptureScreen()
        {
            var screenWidth = (int)SystemParameters.PrimaryScreenWidth;
            var screenHeight = (int)SystemParameters.PrimaryScreenHeight;
            return CaptureRegion(new Rectangle(0, 0, screenWidth, screenHeight));
        }

        public Bitmap CaptureRegion(Rectangle region)
        {
            if (region.Width <= 0 || region.Height <= 0)
                region = new Rectangle(0, 0, (int)SystemParameters.PrimaryScreenWidth,
                                             (int)SystemParameters.PrimaryScreenHeight);

            IntPtr hDesk = GetDesktopWindow();
            IntPtr hSrc = GetWindowDC(hDesk);
            IntPtr hDest = CreateCompatibleDC(hSrc);
            IntPtr hBmp = CreateCompatibleBitmap(hSrc, region.Width, region.Height);
            IntPtr hOld = SelectObject(hDest, hBmp);

            BitBlt(hDest, 0, 0, region.Width, region.Height, hSrc, region.X, region.Y, SRCCOPY);

            Bitmap bmp = Image.FromHbitmap(hBmp);

            SelectObject(hDest, hOld);
            DeleteObject(hBmp);
            DeleteDC(hDest);
            ReleaseDC(hDesk, hSrc);

            return bmp;
        }

        public void SaveScreenshot(Bitmap bitmap, string filePath)
        {
            if (bitmap == null) return;

            try
            {
                bitmap.Save(filePath, ImageFormat.Png);
            }
            catch
            {
            }
        }
    }
}