using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace DigitalEducation
{
    public class OverlayWindowManager
    {
        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        private const int SW_MINIMIZE = 6;
        private const int SW_FORCEMINIMIZE = 11;
        private const int SW_SHOWNORMAL = 1;

        public void MinimizeAllWindows(IntPtr excludeWindowHandle)
        {
            try
            {
                EnumWindows(new EnumWindowsProc((hWnd, lParam) =>
                {
                    if (hWnd == IntPtr.Zero || hWnd == excludeWindowHandle)
                        return true;

                    if (IsWindowVisible(hWnd))
                    {
                        StringBuilder sb = new StringBuilder(256);
                        GetWindowText(hWnd, sb, sb.Capacity);
                        string title = sb.ToString();

                        if (!string.IsNullOrEmpty(title) &&
                            !title.Contains("Program Manager") &&
                            !title.Contains("Microsoft Text Input Application"))
                        {
                            ShowWindowAsync(hWnd, SW_MINIMIZE);
                        }
                    }
                    return true;
                }), IntPtr.Zero);

                MinimizeTaskbar();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сворачивании окон: {ex.Message}");
            }
        }

        public void RestoreWindows()
        {
            try
            {
                RestoreTaskbar();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при восстановлении окон: {ex.Message}");
            }
        }

        private void MinimizeTaskbar()
        {
            IntPtr taskbarHandle = FindWindow("Shell_TrayWnd", null);
            if (taskbarHandle != IntPtr.Zero)
            {
                ShowWindow(taskbarHandle, SW_MINIMIZE);
            }

            IntPtr secondaryTaskbar = FindWindow("NotifyIconOverflowWindow", null);
            if (secondaryTaskbar != IntPtr.Zero)
            {
                ShowWindow(secondaryTaskbar, SW_MINIMIZE);
            }
        }

        private void RestoreTaskbar()
        {
            IntPtr taskbarHandle = FindWindow("Shell_TrayWnd", null);
            if (taskbarHandle != IntPtr.Zero)
            {
                ShowWindow(taskbarHandle, SW_SHOWNORMAL);
            }

            IntPtr secondaryTaskbar = FindWindow("NotifyIconOverflowWindow", null);
            if (secondaryTaskbar != IntPtr.Zero)
            {
                ShowWindow(secondaryTaskbar, SW_SHOWNORMAL);
            }
        }
    }
}