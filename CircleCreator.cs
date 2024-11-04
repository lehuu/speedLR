using System.Windows.Controls;
using System.Windows;

namespace SpeedLR
{
    public class CircleCreator
    {
        private static int START_DISTANCE = 130;
        private static int MENU_DISTANCE = 50;

        public static Thickness CreateButtonsInCircle(Grid centerGrid, int menuNumber, float circleFraction)
        {
            // Get the center position of buttonGrid
            double centerX = centerGrid.Margin.Left + centerGrid.ActualWidth / 2;
            double centerY = centerGrid.Margin.Top + centerGrid.ActualHeight / 2;
            double angle = 2 * Math.PI * circleFraction;

            var distanceDelta = (menuNumber * MENU_DISTANCE);
            var distance = START_DISTANCE + distanceDelta;

            double rotationAngle = (float) menuNumber * (20f - ((menuNumber - 1) * 2.5f)) * Math.PI / 180f;
            angle += rotationAngle;


            // Calculate x and y offsets based on the angle and distance
            double offsetX = distance * Math.Cos(angle);
            double offsetY = distance * Math.Sin(angle);

            return new Thickness(centerX + offsetX, centerY + offsetY, 0, 0);

        }
    }
}
