using System.Windows.Media;
using System.Windows;
using System.Runtime.InteropServices;

namespace SpeedLR.Utils
{
    public static class DpiHelper
    {
        [DllImport("Shcore.dll")]
        static extern int GetScaleFactorForMonitor(IntPtr hmonitor, out int dpi);

        [DllImport("User32.dll")]
        static extern IntPtr MonitorFromPoint(POINT pt, uint dwFlags);

        [DllImport("User32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetCursorPos(ref POINT lpPoint);

        [DllImport("Shcore.dll")]
        private static extern IntPtr GetDpiForMonitor(IntPtr hmonitor, MonitorDpiType dpiType, ref uint dpiX, ref uint dpiY);


        public enum MonitorDpiType
        {
            EffectiveDpi = 0,
            AngularDpi = 1,
            RawDpi = 2,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        // Win32 API constants for MonitorFromPoint
        private const int MONITOR_DEFAULTTONULL = 0x00000000;
        private const int MONITOR_DEFAULTTOPRIMARY = 0x00000001;
        private const int MONITOR_DEFAULTTONEAREST = 0x00000002;

        /// <summary>
        /// Gets the DPI of the specified window.
        /// </summary>
        /// <param name="window">The window to retrieve DPI for.</param>
        /// <returns>The DPI of the monitor where the window is displayed.</returns>
        public static double GetWindowScale(Window window)
        {
            return VisualTreeHelper.GetDpi(window).DpiScaleX;
        }

        public static double GetDpiScaleFactorForMousePosition()
        {
            // Get the current mouse position
            POINT mousePoint = new POINT();
            GetCursorPos(ref mousePoint);

            // Get the monitor handle at the mouse position
            IntPtr hMonitor = MonitorFromPoint(mousePoint, MONITOR_DEFAULTTONEAREST);

            uint dpiX = 0, dpiY = 0;
            GetDpiForMonitor(hMonitor, MonitorDpiType.EffectiveDpi, ref dpiX, ref dpiY);

            return dpiX / 96f; // Assuming dpiX and dpiY are the same
        }

    }
}
