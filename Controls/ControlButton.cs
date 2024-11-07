using System.Windows;
using System.Windows.Controls;
using Button = System.Windows.Controls.Button;


namespace SpeedLR.Controls
{
    public abstract class ControlButton : Button
    {
        public ControlButton ()
        {
            ToolTipService.SetInitialShowDelay(this, 0);
            ToolTipService.SetShowDuration(this, int.MaxValue);
        }
    }
}
