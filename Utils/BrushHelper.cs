using System.Windows.Media;
using Color = System.Windows.Media.Color;

namespace SpeedLR.Utils
{
    internal static class BrushHelper
    {
        public static SolidColorBrush GetBrushFromHex(string hex)
        {
            // Ensure the hex string starts with a #
            if (!hex.StartsWith("#"))
            {
                hex = "#" + hex;
            }

            // Convert the hex string to a Color
            Color color = (Color)System.Windows.Media.ColorConverter.ConvertFromString(hex);

            // Create and return a SolidColorBrush
            return new SolidColorBrush(color);
        }
    }
}
