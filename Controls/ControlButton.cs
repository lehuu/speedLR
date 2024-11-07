using System.Windows.Controls;
using Button = System.Windows.Controls.Button;
 
namespace SpeedLR.Controls
{
    public abstract class ControlButton : Button
    {
        public ControlButton ()
        {
            Width = 30;
            Height = 30;
            ToolTipService.SetInitialShowDelay(this, 0);
            ToolTipService.SetShowDuration(this, int.MaxValue);
        }
    }
}
