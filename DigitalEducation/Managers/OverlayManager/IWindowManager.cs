using System;

namespace DigitalEducation
{
    public interface IWindowManager
    {
        void MinimizeAllWindows(IntPtr excludeWindowHandle);
        void RestoreWindows();
    }
}