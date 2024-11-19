using System.Windows.Controls;
using System.Windows;

namespace SpeedLR
{
    public class GridCreator
    {
        public static int MAX_ROWS = 5;
        public static int MAX_COLS = 8;

        public static int BUTTON_SIZE = 30;

        private static int GAP = 10;

        public static Thickness Create(Grid centerGrid, int col, int row)
        {
            var y = row * (BUTTON_SIZE + GAP);

            var test = (MAX_COLS / 2f - 0.5f);
            float x = BUTTON_SIZE * (2 * col - MAX_COLS + 1) - ((MAX_COLS / 2f - 0.5f - col) * 2 * GAP);

            return new Thickness(x, y, 0, 0);
        }
    }
}