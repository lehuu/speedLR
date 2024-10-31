using System.Windows.Media;
using System.Windows;

namespace SpeedLR
{
    public static class DpiHelper
    {
        /// <summary>
        /// Gets the DPI of the specified window.
        /// </summary>
        /// <param name="window">The window to retrieve DPI for.</param>
        /// <returns>The DPI of the monitor where the window is displayed.</returns>
        public static double GetWindowDpi(Window window)
        {
            var source = PresentationSource.FromVisual(window);
            if (source != null)
            {
                var dpiX = 96.0 * source.CompositionTarget.TransformToDevice.M11;
                return dpiX;
            }
            return 96.0; // Default DPI
        }

        /// <summary>
        /// Adjusts the scale of a window based on the current DPI of the monitor.
        /// </summary>
        /// <param name="window">The window to scale.</param>
        public static void AdjustScaleForDpi(Window window)
        {
            double currentDpi = GetWindowDpi(window);
            double defaultDpi = 96.0; // Standard DPI
            double scale = currentDpi / defaultDpi;

            // Apply scaling transform
            if (window.RenderTransform is ScaleTransform scaleTransform)
            {
                scaleTransform.ScaleX = scale;
                scaleTransform.ScaleY = scale;
            }
            else
            {
                window.RenderTransform = new ScaleTransform(scale, scale);
            }
        }
    }
}
